module Engine.Lang.Parser;

import Engine.Lang.Nullable : Nullable;
import Engine.Lang.Token : Token;
import Engine.Lang.Lexer : Lexer;
import Engine.Lang.Line : Line;

// just for debugging
import std.stdio : writeln;

abstract class Parser
{
   private enum EnumRecursionReturn
   {
      ERROR, // if some error happened, will be found in ErrorMessage
      OK,
      BACKTRACK // if backtracking should be used from the caller
   }

   static class Arc
   {
      enum EnumType
      {
         TOKEN,
         OPERATION,  // TODO< is actualy symbol? >
         ARC,        // another arc, info is the index of the start
         KEYWORD,    // Info is the id of the Keyword

         END,        // Arc end
         NIL,        // Nil Arc

         ERROR       // not used Arc
      }

      public EnumType Type;

      public void delegate(Parser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage) Callback;
      public uint Next;
      public Nullable!uint Alternative;

      public uint Info; // Token Type, Operation Type and so on

      this(EnumType Type, uint Info, void delegate(Parser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage) Callback, uint Next, Nullable!uint Alternative)
      {
         this.Type        = Type;
         this.Info        = Info;
         this.Callback    = Callback;
         this.Next        = Next;
         this.Alternative = Alternative;
      }
   }

   this()
   {
      this.fillArcs();
   }

   /** \brief get called from the constructor to fill the Arc Tables required for parsing
    *
    */
   /* pure virtual */protected void fillArcs();

   /** \brief gets called before the actual parsing
    *
    */
   /* pure virtual */protected void setupBeforeParsing();

   /** \brief 
    *
    * \param ErrorMessage will hold the error message on error
    * \param ArcTableIndex is the index in the ArcTable
    * \return
    */
   // NOTE< this is written recursive because it is better understandable that way and i was too lazy to reformulate it >
   final private EnumRecursionReturn parseRecursive(ref string ErrorMessage, uint ArcTableIndex)
   {
      bool AteAnyToken;
      EnumRecursionReturn ReturnValue;

      AteAnyToken = false;
      ReturnValue = EnumRecursionReturn.BACKTRACK;

      for(;;)
      {
         writeln("ArcTableIndex ", ArcTableIndex);

         switch( this.Arcs[ArcTableIndex].Type )
         {
            ///// NIL
            case Parser.Arc.EnumType.NIL:
            ReturnValue = EnumRecursionReturn.OK;
            break;

            ///// OPERATION
            case Parser.Arc.EnumType.OPERATION:
            if( this.Arcs[ArcTableIndex].Info == this.CurrentToken.ContentOperation )
            {
               ReturnValue = EnumRecursionReturn.OK;
            }
            else
            {
               ReturnValue = EnumRecursionReturn.BACKTRACK;
            }
            break;

            ///// TOKEN
            case Parser.Arc.EnumType.TOKEN:
            if( this.Arcs[ArcTableIndex].Info == this.CurrentToken.Type )
            {
               ReturnValue = EnumRecursionReturn.OK;
            }
            else
            {
               ReturnValue = EnumRecursionReturn.BACKTRACK;
            }
            break;

            ///// ARC
            case Parser.Arc.EnumType.ARC:
            ReturnValue = this.parseRecursive(ErrorMessage, this.Arcs[ArcTableIndex].Info);
            break;

            ///// END
            case Parser.Arc.EnumType.END:

            // TODO< check if we really are at the end of all tokens >

            writeln("end");

            return EnumRecursionReturn.OK;

            break;


            default:
            ErrorMessage = "Internal Error!";
            return EnumRecursionReturn.ERROR;
         }



         if( ReturnValue == EnumRecursionReturn.ERROR )
         {
            return EnumRecursionReturn.ERROR;
         }

         if( ReturnValue == EnumRecursionReturn.OK )
         {
            bool CallbackSuccess;
            string CallbackErrorMessage;

            this.Arcs[ArcTableIndex].Callback(this, this.CurrentToken, CallbackSuccess, CallbackErrorMessage);

            if( !CallbackSuccess )
            {
               // ASK< sould we return the error or continue? >

               // i decided to just return an error

               // TODO< pass over the ErrorMessage and build describing string and so on >
               ErrorMessage = CallbackErrorMessage;

               return EnumRecursionReturn.ERROR;
            }

            ReturnValue = EnumRecursionReturn.OK;
         }

         if( ReturnValue == EnumRecursionReturn.BACKTRACK )
         {
            // we try alternative arcs
            writeln("backtracking");

            if( !this.Arcs[ArcTableIndex].Alternative.isNull() )
            {
               ArcTableIndex = this.Arcs[ArcTableIndex].Alternative.Value;
            }
            else if( AteAnyToken )
            {
               return EnumRecursionReturn.ERROR;
            }
            else
            {
               return EnumRecursionReturn.BACKTRACK;
            }
         }
         else
         {
            // accept formaly the token

            if(
               this.Arcs[ArcTableIndex].Type == Parser.Arc.EnumType.OPERATION ||
               this.Arcs[ArcTableIndex].Type == Parser.Arc.EnumType.TOKEN
            )
            {
               bool CalleeSuccess;

               writeln("eat token");

               this.eatToken(CalleeSuccess);

               if( !CalleeSuccess )
               {
                  ErrorMessage = "Internal Error!\n";
                  return EnumRecursionReturn.ERROR;
               }

               AteAnyToken = true;
            }

            ArcTableIndex = this.Arcs[ArcTableIndex].Next;
         }
      }
   }

   /** \brief do the parsing
    *
    * \param ErrorMessage is the string that will contain the error message when an error happened
    * \return true on success
    */
   final public bool parse(ref string ErrorMessage)
   {
      EnumRecursionReturn RecursionReturn;
      bool CalleeSuccess;

      this.CurrentToken = new Token();

      this.setupBeforeParsing();

      // read first token
      this.eatToken(CalleeSuccess);
      if( !CalleeSuccess )
      {
         ErrorMessage = "Internal Error!\n";
         return false;
      }

      this.CurrentToken.debugIt();

      RecursionReturn = this.parseRecursive(ErrorMessage, 0);

      if( RecursionReturn == EnumRecursionReturn.ERROR )
      {
         return false;
      }
      else if( RecursionReturn == EnumRecursionReturn.BACKTRACK )
      {
         ErrorMessage = "Internal Error!\n";
         return false;
      }

      // check if the last token was an EOF
      if( CurrentToken.Type != Token.EnumType.EOF )
      {
         // TODO< add line information and marker >

         // TODO< get the string format of the last token >
         ErrorMessage = "Unexpected Tokens after (Last) Token";
         return false;
      }

      return true;
   }

   final private void eatToken(ref bool Success)
   {
      Lexer.EnumLexerCode LexerReturnValue;

      LexerReturnValue = this.OfLexer.getNextToken(this.CurrentToken);

      this.CurrentToken.debugIt();

      Success = (LexerReturnValue == Lexer.EnumLexerCode.OK);

      this.addTokenToLines(this.CurrentToken.copy());

      return;
   }

   final public void setLexer(Lexer OfLexer)
   {
      this.OfLexer = OfLexer;
   }

   final public void addTokenToLines(Token TokenObject)
   {
      if( TokenObject.Line != this.CurrentLineNumber )
      {
         CurrentLineNumber = TokenObject.Line;
         this.Lines ~= new Line();
      }

      this.Lines[this.Lines.length-1].Tokens ~= TokenObject;
   }


   private Token CurrentToken;

   protected Arc []Arcs;
   public  Lexer OfLexer;

   //private Token []TokensOnLine;


   //private uint LineCounter = 0;

   private Line []Lines;
   private uint CurrentLineNumber = 0;
}
