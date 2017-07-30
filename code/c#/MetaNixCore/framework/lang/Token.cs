
namespace MetaNix.framework.lang {
    public class Token {
        public enum EnumType {
            NUMBER = 0,
            IDENTIFIER,
            KEYWORD,       // example: if do end then
            OPERATION,     // example: := > < >= <=
      
            ERROR,         // if Lexer found an error
            INTERNALERROR, // if token wasn't initialized by Lexer
            STRING,        // "..."
      
            EOF,            // end of file
            // TODO< more? >
        }

        /* commented because not yet required because its just used for debugging
        public void debugIt() {
          writeln("Type: " ~ to!string(type));

          if( type == EnumType.OPERATION ) {
             writeln("Operation: " ~ to!string(contentOperation));
          }
          else if( type == EnumType.NUMBER ) {
             writeln(contentNumber);
          }
          else if( type == EnumType.IDENTIFIER ) {
             writeln(contentString);
          }
          else if( type == EnumType.STRING ) {
             writeln(contentString);
          }

          writeln("Line   : ", line);
          writeln("Column : ", column);

          writeln("===");
       }
       

       final public string getRealString() {
          if( type == EnumType.OPERATION ) {
             return to!string(this.contentOperation);
          }
          else if( type == EnumType.IDENTIFIER ) {
             return this.contentString;
          }
          else if( type == EnumType.NUMBER ) {
             // TODO< catch exceptions >
             return to!string(contentNumber);
          }
          else if( type == EnumType.STRING ) {
             return contentString;
          }
      

          return "";
       }*/

        public Token copy() {
            Token result = new Token();
            result.contentString = contentString;
            result.contentOperation = contentOperation;
            result.contentNumber = contentNumber;
            result.type = type;
            result.line = line;
            result.column = column;
            return result;
        }

        public string contentString;
        public uint contentOperation = 0; // set to internalerror, is enum
        public long contentNumber = 0;

        public EnumType type = EnumType.INTERNALERROR;
        public uint line = 0;
        public uint column = 0; // Spalte
        // public string Filename;
    }
}
