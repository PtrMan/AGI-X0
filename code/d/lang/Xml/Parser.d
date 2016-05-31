module Engine.Lang.Xml.Parser;

// or debugging
import std.stdio : writeln;

import Engine.Lang.Parser : AbstractParser = Parser;
import Engine.Lang.Token : Token;
import Engine.Lang.Nullable : Nullable;

import Engine.Xml.Document;
import Engine.Xml.Attribute;

class Parser : AbstractParser
{
   /** \brief returns the XML document with the content
    *
    * \return ...
    */
   final public Document getDocument()
   {
      return this.OfDocument;
   }

   override protected void fillArcs()
   {
      // TODO functions

      void nothing(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Success = true;
      }

      void endTreeName(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Parser OfParser;

         OfParser = cast(Parser)ParserObj;

         Success = false;

         // check if top tree Element is root
         if( OfParser.OfDocument.RootNode is OfParser.TopNode )
         {
            ErrorMessage = "Termination of a Subtree at root level!";
            return;
         }

         if( OfParser.TopNode.Name != CurrentToken.ContentString )
         {
            ErrorMessage = "Can't terminate a Subtree with that name!";
            return;
         }

         // move top Node element to parent
         OfParser.TopNode = OfParser.TopNode.getParent();

         Success = true;
      }

      void newTerminalElement(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Parser OfParser;
         Node TerminalElementNode;

         OfParser = cast(Parser)ParserObj;

         Success = true;

         TerminalElementNode = new Node(Node.EnumType.TERMINAL, OfParser.TopNode);
         TerminalElementNode.Name = OfParser.TempName;

         TerminalElementNode.Attributes = OfParser.TempAttributes;

         OfParser.TempAttributes.length = 0;

         OfParser.TopNode.Childrens ~= TerminalElementNode;
      }

      void saveTempName(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Parser OfParser;

         OfParser = cast(Parser)ParserObj;

         Success = true;

         OfParser.TempName = CurrentToken.ContentString;
      }

      void createNewNode(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Parser OfParser;
         Node NewlyNode;

         OfParser = cast(Parser)ParserObj;

         Success = true;
         
         NewlyNode = new Node(Node.EnumType.NORMAL, OfParser.TopNode);
         NewlyNode.Name = OfParser.TempName;

         NewlyNode.Attributes = OfParser.TempAttributes;

         OfParser.TempAttributes.length = 0;

         OfParser.TopNode.Childrens ~= NewlyNode;
         OfParser.TopNode = NewlyNode;
      }

      void createString(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Parser OfParser;
         Node NewlyNode;

         OfParser = cast(Parser)ParserObj;

         Success = true;
         
         NewlyNode = new Node(Node.EnumType.STRING, OfParser.TopNode);
         NewlyNode.StringContent = CurrentToken.ContentEscapedString.convertToString();

         OfParser.TopNode.Childrens ~= NewlyNode;
      }

      void saveAttributeName(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Parser OfParser;

         OfParser = cast(Parser)ParserObj;

         Success = true;

         OfParser.TempAttributeName = CurrentToken.ContentString;
      }

      void saveStringAttribute(AbstractParser ParserObj, ref Token CurrentToken, ref bool Success, ref string ErrorMessage)
      {
         Parser OfParser;
         Attribute CreatedAttribute;

         OfParser = cast(Parser)ParserObj;

         Success = false;

         writeln("here");
         if( OfParser.TopNode.hasAttribute(OfParser.TempAttributeName) )
         {
            writeln("out here");

            ErrorMessage = "Node does allready have an attribute with the name \"" ~ OfParser.TempAttributeName ~ "\"!";
            return;
         }

         CreatedAttribute = new Attribute(Attribute.EnumType.STRING);
         CreatedAttribute.Name = OfParser.TempAttributeName;
         CreatedAttribute.ContentString = CurrentToken.ContentEscapedString.convertToString();

         OfParser.TempAttributes ~= CreatedAttribute;

         Success = true;

         writeln("new attribute ", OfParser.TempAttributeName);
      }

      Nullable!uint NullUint = new Nullable!uint(true, 0);

      // Tree
      /*  0 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ARC      , 10/* ElementA */                   , &nothing, 0, new Nullable!uint(false, 1));
      /*  1 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.END      , 0                                         , &nothing,0, NullUint                   );
      /*  2 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR      , 0                                         , &nothing, 0                 , NullUint                     );
      
      /*  3 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                         , &nothing             , 0                     , NullUint                     );
      /*  4 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                         , &nothing             , 0                      , NullUint                     );
      /*  5 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                         , &nothing             , 0                      , NullUint                     );
      /*  6 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                         , &nothing             , 0                      , NullUint                     );
      /*  7 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                         , &nothing             , 0                      , NullUint                     );
      /*  8 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                         , &nothing             , 0                      , NullUint                     );
      /*  9 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                         , &nothing             , 0                      , NullUint                     );

      // ElementA
      /* 10 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.OPERATION, cast(uint)Token.EnumOperation.SMALLER, &nothing           , 11, new Nullable!uint(false, 25));
      /* 11 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.TOKEN    , cast(uint)Token.EnumType.IDENTIFIER  , &saveTempName      , 12, new Nullable!uint(false, 22));
      /* 12 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.TOKEN    , cast(uint)Token.EnumType.IDENTIFIER  , &saveAttributeName , 15, new Nullable!uint(false, 13));
      /* 13 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.OPERATION, cast(uint)Token.EnumOperation.DIV    , &newTerminalElement, 14, new Nullable!uint(false, 18));
      /* 14 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.OPERATION, cast(uint)Token.EnumOperation.GREATER, &nothing           , 21, NullUint);

      /* 15 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.OPERATION, cast(uint)Token.EnumOperation.EQUAL       , &nothing, 16, new Nullable!uint(false, 17));
      /* 16 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.TOKEN    , cast(uint)Token.EnumType.STRING           , &saveStringAttribute, 12, NullUint);
      /* 17 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.TOKEN    , cast(uint)Token.EnumType.NUMBER           , &nothing, 12, NullUint);

      /* 18 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.OPERATION, cast(uint)Token.EnumOperation.GREATER, &createNewNode, 19         , NullUint);
      /* 19 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ARC      , 0/* Tree */                          , &nothing, 21/* end */, NullUint);
      /* 20 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.ERROR    , 0                                    , &nothing,  0         , NullUint);
      /* 21 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.END      , 0                                    , &nothing,  0         , NullUint);
      
      /* 22 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.OPERATION, cast(uint)Token.EnumOperation.DIV    , &nothing, 23         , NullUint);
      /* 23 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.TOKEN    , cast(uint)Token.EnumType.IDENTIFIER  , &endTreeName, 24         , NullUint);
      /* 24 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.OPERATION, cast(uint)Token.EnumOperation.GREATER, &nothing, 21/* end */, NullUint);

      /* 25 */this.Arcs ~= new Arc(AbstractParser.Arc.EnumType.TOKEN    , cast(uint)Token.EnumType.STRING      , &createString, 21 /* end */, NullUint);

      assert(this.Arcs.length == 26, "Arcs list does have wrong length!");
   }

   override protected void setupBeforeParsing()
   {
      this.OfDocument = new Document();
      this.TopNode = this.OfDocument.RootNode;
   }

   private Document OfDocument;
   private Node TopNode;

   private string TempName; /**< Temporary Node name */
   private string TempAttributeName; /**< Temporary Attribute name */
   private Attribute[] TempAttributes;
}
