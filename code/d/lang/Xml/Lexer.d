module Engine.Lang.Xml.Lexer;

import Engine.Lang.Lexer : AbstractLexer = Lexer;
import Engine.Lang.Token;

// for debugging
import std.stdio : writeln;

// TOUML
// TODOCU
class Lexer : AbstractLexer
{
   override protected void inEndstate(uint State, ref Token OutputToken, EscapedString TempContent)
   {
      if( State == 1 ) // identifier
      {
         OutputToken.Type = Token.EnumType.IDENTIFIER;
         OutputToken.ContentString = TempContent.convertToString();
      }
      else if( State == 2 ) // <
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.SMALLER;
      }
      else if( State == 3 ) // >
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.GREATER;
      }
      else if( State == 4 ) // /
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.DIV;
      }
      else if( State == 5 ) // inside text
      {
         // parsing error

         OutputToken.Type = Token.EnumType.ERROR;
      }
      else if( State == 6 ) // "..."
      {
         OutputToken.Type = Token.EnumType.STRING;
         OutputToken.ContentEscapedString = TempContent;
      }
      else if( State == 7 ) // =
      {
         OutputToken.Type = Token.EnumType.OPERATION;
         OutputToken.ContentOperation = Token.EnumOperation.EQUAL;
      }
      else
      {
         OutputToken.Type = Token.EnumType.INTERNALERROR;
      }
   }

   override protected void fillTable()
   {
      this.LexerTable = [
      //                            /-----           special character       -----\  /-----             number             -----\  /-----            letter              -----\  /-----              :                 -----\  /-----                =               -----\  /-----              <                 -----\  /-----                >               -----\  /-----          controlsign           -----\  /-----               "                -----\  /-----                \               -----\  /-----               /                -----\
      /*   0 start                */new Lexer.TableElement(false ,    0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(true ,  1, false,  1), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(true ,  0, false,  7), new Lexer.TableElement(true ,  0, false,  2), new Lexer.TableElement(false,  0, false,  3), new Lexer.TableElement(true ,  0, false,  0), new Lexer.TableElement(true ,  0, false,  5), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, false,  4),
      /*   1 identifier           */new Lexer.TableElement(false ,    0, true ,  0), new Lexer.TableElement(true ,  1, false,  1), new Lexer.TableElement(true ,  1, false,  1), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0),
      /*   2 <  read              */new Lexer.TableElement(false ,    0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), 
      /*   3 >  read              */new Lexer.TableElement(true  ,    0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0),
      /*   4 /  read              */new Lexer.TableElement(true  ,    0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0), new Lexer.TableElement(true ,  0, true ,  0),
      /*   5 "  read, inside text */new Lexer.TableElement(true  ,    1, false,  5), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  0, false,  6), new Lexer.TableElement(true ,  1, false,  5), new Lexer.TableElement(true ,  1, false,  5),
      /*   6 "..." read           */new Lexer.TableElement(false ,    0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0),
      /*   7 =  read              */new Lexer.TableElement(false ,    0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0), new Lexer.TableElement(false,  0, true ,  0)
      ];
   }

   override protected void configure()
   {
      this.ConfigurationCaseInsensitive = false;
   }
}
