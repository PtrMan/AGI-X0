module Engine.Lang.Lexer;

import std.stdio : writeln;
import std.conv : convertTo = parse, ConvOverflowException;
import std.string : stringCompare = icmp, stringIndexOf = indexOf;

import Engine.Lang.Token : Token;
import Engine.Lang.EscapedString : EscapedString;

// TODO<
// - generalize the termination action
// - uml
// - comment
// >
abstract class Lexer
{
   static class TableElement
   {
      enum EnumWriteType
      {
         NOWRITE,
         NORMAL,
         ESCAPED
      }

      public bool Terminate;
      public EnumWriteType Write;
      public bool Read;

      public uint FollowState;

      this(bool Read, uint Write, bool Terminate, uint FollowState)
      {
         this.Read        = Read;

         if( Write == 0 )
         {
            this.Write = EnumWriteType.NOWRITE;
         }
         else if( Write == 1 )
         {
            this.Write = EnumWriteType.NORMAL;
         }
         else
         {
            this.Write = EnumWriteType.ESCAPED;
         }

         this.Terminate   = Terminate;
         this.FollowState = FollowState;
      }
   }

   public enum EnumLexerCode
   {
      OK,
      INTERNALERROR
   }
   
   /*
   final void setLexerTable(TableElement []LexerTable, out bool Success)
   {
      Success = false;

      if( (LexerTable.length % LEXERTABLE_WIDTH) != 0 )
      {
         return;
      }

      this.LexerTableSet = true;
      this.LexerTable = LexerTable;

      Success = true;
   }
   */

   this()
   {
      this.fillTable();

      assert((this.LexerTable.length % LEXERTABLE_WIDTH) == 0, "Lexer.fillTable() failed!");

      this.configure();

      this.TypeTable =
      [/* 00 */ 7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,
       /* 10 */ 7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,
       /* 20 */ 7,  0,  8,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 10,
       /* 30 */ 1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  3,  0,  5,  4,  6,  0,
       /* 40 */ 0,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,
       /* 50 */ 2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  0,  9,  0,  0,  0,
       /* 60 */ 0,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,
       /* 70 */ 2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  0,  0,  0,  0,  0
      ];
   }
   
   final public void setSource(string Source)
   {
      this.Source = Source;

      this.Index = 0; // reset index
   }
   
   final public EnumLexerCode getNextToken(ref Token OutputToken)
   {
      EscapedString TempContent;
      
      bool  FirstSign = true;

      EnumLexerCode Return = EnumLexerCode.INTERNALERROR;
      
      // State of our DFA
      uint State = 0;
      
      // init Token
      OutputToken.Type = Token.EnumType.INTERNALERROR;
      OutputToken.ContentOperation = Token.EnumOperation.INTERNALERROR;
      
      // failsafe?
      OutputToken.Line = this.ActualLine;
      OutputToken.Column = this.ActualColumn;

      // check for end of file
      // we do it here because we need to emit a token in the loop
      if( this.Index >= this.Source.length )
      {
         OutputToken.Line = this.ActualLine;
         OutputToken.Column = this.ActualColumn;

         OutputToken.Type = Token.EnumType.EOF;

         return EnumLexerCode.OK;
      }

      for(;;)
      {
         TableElement LexerTableElement;
         uint FollowState;
         
         char Sign;
         uint SignType;
         
         // check for end of file
         if( this.Index >= this.Source.length )
         {
            break;

            OutputToken.Line = this.ActualLine;
            OutputToken.Column = this.ActualColumn;

            OutputToken.Type = Token.EnumType.EOF;

            return EnumLexerCode.OK;
         }
                  
         Sign = this.Source[this.Index];
         
         // Convert Big Sign into Small
         // (because it is case in sensitive)
         if( this.ConfigurationCaseInsensitive )
         {
            if( (Sign >= 65) && (Sign <= 90) )
            {
               Sign += (97-65);
            }
         }

         SignType = this.TypeTable[Sign];
         
         if( SignType == 1337 )
         {
            return Return; // error
         }
         
         //writeln("State: ", State);
         //writeln("SignType: ", SignType);
         //writeln("Sign: ", Sign);
         //writeln("---");

         // lookup the SignType in the LexerTable
         
         LexerTableElement = this.LexerTable[State*this.LEXERTABLE_WIDTH + SignType];

         if( LexerTableElement.Write == TableElement.EnumWriteType.NOWRITE )
         {
            // nothing
         }
         else if( LexerTableElement.Write == TableElement.EnumWriteType.NORMAL )
         {
            TempContent.append(Sign, false);

            if( FirstSign )
            {
               OutputToken.Line = this.ActualLine;
               OutputToken.Column = this.ActualColumn;
            }

            FirstSign = false;
         }
         else if( LexerTableElement.Write == TableElement.EnumWriteType.ESCAPED )
         {
            TempContent.append(Sign, true);

            if( FirstSign )
            {
               OutputToken.Line = this.ActualLine;
               OutputToken.Column = this.ActualColumn;
            }

            FirstSign = false;
         }
         
         if( LexerTableElement.Read )
         {
            this.Index++;

            if( Sign == '\n' )
            {
               this.ActualLine++;
               this.ActualColumn = 0;
            }
            else
            {
               this.ActualColumn++;
            }
         }

         
         // check if we have to terminate the DFA
         if( LexerTableElement.Terminate )
         {
            break;
         }

         State = LexerTableElement.FollowState;
         
      }

      if( State == 0 ) // invalid or Special Character ( = * + - )
      {
         uint Index;

         assert(TempContent.getContent().length == 1, "The Length of TempContent must be 1!");

         Index = stringIndexOf("+-*;,!?.=()#", TempContent.getContent()[0].Char);

         if( Index != -1 )
         {
            OutputToken.Type = Token.EnumType.OPERATION;
            OutputToken.ContentOperation = cast(Token.EnumOperation)Index;

         }
         else
         {
            // return ERROR-Token
         
            OutputToken.Type = Token.EnumType.ERROR;
         }
      }
      else if( State <= this.LexerTable.length/LEXERTABLE_WIDTH )
      {
         this.inEndstate(State, OutputToken, TempContent);
      }
      /+
      else if( State == 1 ) // identifier or keyword
      {
         bool IsKeyword;
         uint KeywordIndex;

         this.getKeyword(TempContent.convertToString(), IsKeyword, KeywordIndex);
         if( !IsKeyword )
         {
            OutputToken.Type = Token.EnumType.IDENTIFIER;
            OutputToken.ContentString = TempContent.convertToString();
         }
         else
         {
            OutputToken.Type = Token.EnumType.KEYWORD;
            OutputToken.ContentKeyword = cast(Token.EnumKeyword)KeywordIndex;
         }
      }
      else if( State == 2 ) // Number
      {
         int Number;
         bool CalleeSuccess;
         
         Number = convertStringToNumber(CalleeSuccess, TempContent.convertToString());

         if( !CalleeSuccess )
         {
            OutputToken.Type = Token.EnumType.ERROR;
         }
         else
         {
            OutputToken.Type = Token.EnumType.NUMBER;
            OutputToken.ContentNumber = Number;
         }
      }
      else if( State == 3 ) // :=
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.ASSIGNMENT;
      }
      else if( State == 4 ) // <
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.SMALLER;
      }
      else if( State == 5 ) // >
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.GREATER;
      }
      else if( State == 6 ) // :=
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.ASSIGNMENT;
      }
      else if( State == 7 ) // <=
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.SMALLEREQUAL;
      }
      else if( State == 8 ) // >=
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.GREATEREQUAL;
      }
      else if( State == 9 )
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.UNEQUAL;
      }
      /*else if( State == 10 )
      {
         // syntax error

         OutputToken.Type = Token.EnumType.ERROR;
      }*/
      else if( State == 10 ) // string , "..."
      {
         OutputToken.Type = Token.EnumType.STRING;
         OutputToken.ContentEscapedString = TempContent;
      }+/
      else
      {
         // internal error
         return Return;
      }
      
      return EnumLexerCode.OK;
   }

   /**
    * \brief is called if a the lexer terminated in a endstate
    *
    */
   /* pure virtual */protected void inEndstate(uint State, ref Token OutputToken, EscapedString TempContent);
   
   /**
    * \brief is called from the constructor
    *
    */
   /* pure virtual */protected void fillTable();

   /** \brief is called from the constructor
    *
    * should only set configure bits to configure the behavior of the Lexer
    *
    */
   /* pure virtual */protected void configure();

   // Action that the DFA does
   
   const uint ActionWrite = 1<<6;
   const uint ActionRead = 1<<7;
   const uint ActionTerminate = 1<<5;
   
   private string Source;
   
   private uint Index = 0;
   
   protected TableElement[] LexerTable;
   private bool LexerTableSet = false;

   private uint[16*8] TypeTable;
   
   // position in Source File
   private string ActualFilename = "<stdin>";
   private uint ActualLine = 1;
   private uint ActualColumn = 0;

   private const uint LEXERTABLE_WIDTH = 11;

   protected bool ConfigurationCaseInsensitive = true;
}
