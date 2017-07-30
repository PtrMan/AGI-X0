using System;
using System.Collections.Generic;

namespace MetaNix.framework.lang {
    public class ParsingException : Exception {
        public ParsingException(string message) {
            this.message = message;
        }

        public readonly string message;
    }

    // translated from D from my library called "misced"
    public abstract class Parser {
        private enum EnumRecursionReturn {
            OK,
            BACKTRACK, // if backtracking should be used from the caller
        }

        public class Arc {
            public enum EnumType {
                TOKEN,
                OPERATION,  // TODO< is actualy symbol? >
                ARC,        // another arc, info is the index of the start
                KEYWORD,    // Info is the id of the Keyword

                END,        // Arc end
                NIL,        // Nil Arc

                ERROR,       // not used Arc
            }

            public delegate void CallbackType(Parser ParserObj, Token CurrentToken);
          
            public EnumType Type;
            public CallbackType Callback;
            public uint Next;
            public uint? Alternative;

            public uint Info; // Token Type, Operation Type and so on

            public Arc(EnumType Type, uint Info, CallbackType Callback, uint Next, uint? Alternative) {
                this.Type        = Type;
                this.Info        = Info;
                this.Callback    = Callback;
                this.Next        = Next;
                this.Alternative = Alternative;
            }
        }

        public Parser() {
            this.fillArcs();

            this.lines.Add(new Line());
        }

        /** \brief get called from the constructor to fill the Arc Tables required for parsing
         *
         */
        abstract protected void fillArcs();

        /** \brief gets called before the actual parsing
        *
        */
        abstract protected void setupBeforeParsing();

        /** \brief 
        *
        * \param ErrorMessage will hold the error message on error
        * \param ArcTableIndex is the index in the ArcTable
        * \return
        */
        // NOTE< this is written recursive because it is better understandable that way and i was too lazy to reformulate it >
        EnumRecursionReturn parseRecursive(int arcTableIndex) {
            bool AteAnyToken;
            EnumRecursionReturn ReturnValue;

            AteAnyToken = false;
            ReturnValue = EnumRecursionReturn.BACKTRACK;

            for(;;) {
                if( debug )  Console.WriteLine("arcTableIndex={0}", arcTableIndex);
                
                // with(Parser.Arc.EnumType)
                switch( this.arcs[arcTableIndex].Type )  {
                ///// NIL
                case Parser.Arc.EnumType.NIL:
                // if the alternative is null we just go to next, if it is not null we follow the alternative
                // we do this to simplify later rewriting of the rule(s)
                if( this.arcs[arcTableIndex].Alternative == null ) {
                    ReturnValue = EnumRecursionReturn.OK;
                }
                else {
                    ReturnValue = EnumRecursionReturn.BACKTRACK;
                }
                break;

                ///// OPERATION
                case Parser.Arc.EnumType.OPERATION:
                if( this.currentToken.type == Token.EnumType.OPERATION && this.arcs[arcTableIndex].Info == this.currentToken.contentOperation ) {
                    ReturnValue = EnumRecursionReturn.OK;
                }
                else {
                    ReturnValue = EnumRecursionReturn.BACKTRACK;
                }
                break;

                ///// TOKEN
                case Parser.Arc.EnumType.TOKEN:
                if( (Token.EnumType)this.arcs[arcTableIndex].Info == this.currentToken.type ) {
                    ReturnValue = EnumRecursionReturn.OK;
                }
                else {
                    ReturnValue = EnumRecursionReturn.BACKTRACK;
                }
                break;

                ///// ARC
                case Parser.Arc.EnumType.ARC:
                ReturnValue = this.parseRecursive((int)this.arcs[arcTableIndex].Info);
                break;

                ///// END
                case Parser.Arc.EnumType.END:

                // TODO< check if we really are at the end of all tokens >

                if( debug )  Console.WriteLine("end");

                return EnumRecursionReturn.OK;
                break;

                default:
                throw new Exception("Internal Error");
                }


                

                if( ReturnValue == EnumRecursionReturn.OK ) {
                this.arcs[arcTableIndex].Callback(this, this.currentToken);
                ReturnValue = EnumRecursionReturn.OK;
                }

                if( ReturnValue == EnumRecursionReturn.BACKTRACK ) {
                    // we try alternative arcs
                    //writeln("backtracking");

                    if( this.arcs[arcTableIndex].Alternative != null ) {
                        arcTableIndex = (int)this.arcs[arcTableIndex].Alternative.Value;
                    }
                    else if( AteAnyToken ) {
                        throw new ParsingException("");
                    }
                    else {
                        return EnumRecursionReturn.BACKTRACK;
                    }
                }
                else {
                // accept formaly the token

                    if(
                        this.arcs[arcTableIndex].Type == Parser.Arc.EnumType.OPERATION ||
                        this.arcs[arcTableIndex].Type == Parser.Arc.EnumType.TOKEN
                    ) {
                        bool CalleeSuccess;

                        if( debug )  Console.WriteLine("eat token");

                        this.eatToken(out CalleeSuccess);

                        if( !CalleeSuccess ) {
                            throw new Exception("Internal error");
                        }

                        AteAnyToken = true;
                    }

                    arcTableIndex = (int)this.arcs[arcTableIndex].Next;
                }
            }
        }

       /** \brief do the parsing
        *
        * \param ErrorMessage is the string that will contain the error message when an error happened
        * \return true on success
        */
       public void parse() {
          EnumRecursionReturn RecursionReturn;
          bool CalleeSuccess;

          this.currentToken = new Token();

          this.setupBeforeParsing();

          // read first token
          this.eatToken(out CalleeSuccess);
          if( !CalleeSuccess ) {
                throw new Exception("Internal error");
          }

          //this.CurrentToken.debugIt();

          RecursionReturn = this.parseRecursive(0);
            
          if( RecursionReturn == EnumRecursionReturn.BACKTRACK ) {
             throw new Exception("Internal error");
          }

          // check if the last token was an EOF
          if( currentToken.type != Token.EnumType.EOF ) {
             // TODO< add line information and marker >

             // TODO< get the string format of the last token >
             throw new ParsingException("Unexpected Tokens after (Last) Token");
          }
       }

        void eatToken(out bool success) {
            Lexer.EnumLexerCode lexerReturnValue = lexer.nextToken(out currentToken);
        
            success = lexerReturnValue == Lexer.EnumLexerCode.OK;
            if( !success ) {
                return;
            }

            //this.CurrentToken.debugIt();

            this.addTokenToLines(this.currentToken.copy());

            return;
        }
        
        public void addTokenToLines(Token token) {
            if( token.line != this.currentLineNumber ) {
                currentLineNumber = token.line;
                this.lines.Add(new Line());
            }

            this.lines[this.lines.Count-1].Tokens.Add(token);
        }


        private Token currentToken;

        protected IList<Arc> arcs = new List<Arc>();
        public  Lexer lexer;

        public bool debug = true;
        
        private IList<Line> lines = new List<Line>();
        private uint currentLineNumber = 0;
    }
}
