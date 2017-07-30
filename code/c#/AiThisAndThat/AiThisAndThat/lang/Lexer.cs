using System;
using System.Text.RegularExpressions;

namespace AiThisAndThat.lang {
    public abstract class Lexer {
        public enum EnumLexerCode {
            OK,
            INVALID,
        }
        
        public void setSource(string source) {
            this.remainingSource = source;
        }
   
        public EnumLexerCode nextToken(out Token resultToken) {
            for(;;) {
                int index;
                EnumLexerCode lexerCode = nextTokenInternal(out resultToken, out index);
                if( lexerCode != EnumLexerCode.OK )   return lexerCode;
                if( resultToken.type == Token.EnumType.EOF )  return lexerCode;
                if( index == 0 )  continue;

                return lexerCode;
            }
        }

        protected EnumLexerCode nextTokenInternal(out Token resultToken, out int index) {
            index = 0;
            resultToken = null;

            bool endReached = remainingSource.Length == 0;
            if( endReached ) {
                resultToken = new Token();
                resultToken.type = Token.EnumType.EOF;
                return EnumLexerCode.OK;
            }
            
            for( int iindex = 0; iindex < tokenRules.Length; iindex++ ) {
                var iTokenRule = tokenRules[iindex];

                Regex myRegex = new Regex(iTokenRule.regularExpression);
                Match m = myRegex.Match(remainingSource);
                
                if( m.Success ) {
                    string matchedString = m.Groups[1].Value;

                    Console.WriteLine(matchedString);

                    string completeMatchedString = m.Groups[0].Value;

                    remainingSource = remainingSource.Substring(completeMatchedString.Length);

                    index = iindex;
                    resultToken = createToken(iindex, matchedString);
                    return EnumLexerCode.OK;
                }
            }

            return EnumLexerCode.INVALID;
        }

        public Lexer() {
            fillRules();
        }

        abstract protected Token createToken(int ruleIndex, string matchedString);

        abstract protected void fillRules();

        public class Rule {
            public string regularExpression; // regular expression its matched with

            public Rule(string regularExpression) {
                this.regularExpression = regularExpression;
            }
        }


        public Rule[] tokenRules;
        // token rule #0 is ignored, because it contains the pattern for spaces

        private string remainingSource;

        // position in Source File
        private string actualFilename = "<stdin>";
        private uint actualLine = 1;
        private uint actualColumn = 0;
    }
}
