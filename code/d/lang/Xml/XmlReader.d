module Engine.Lang.Xml.XmlReader;

import std.file : readText;

import Engine.Lang.Xml.Lexer : Lexer;
import Engine.Lang.Xml.Parser : Parser;

import Engine.Xml.Document : XmlDocument = Document;

class XmlReader
{
   final static public XmlDocument read(string Filename, ref bool Success, ref string ErrorMessage)
   {
      string FileContent;
      Lexer OfLexer;
      Parser OfParser;
      bool ParsingSuccess;

      Success = false;
      ErrorMessage = "";

      FileContent = readText(Filename);

      OfLexer = new Lexer();
      OfParser = new Parser();

      OfLexer.setSource(FileContent);
      
      OfParser.setLexer(OfLexer);
   
      ParsingSuccess = OfParser.parse(ErrorMessage);

      if( !ParsingSuccess )
      {
         ErrorMessage = "Parsing Failed: " ~ ErrorMessage;
         return null;
      }
      
      Success = true;

      return OfParser.getDocument();
   }
}
