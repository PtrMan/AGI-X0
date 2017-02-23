
using System;
using System.Collections.Generic;

namespace MetaNix {
    // from https://github.com/PtrMan/ai2/blob/master/ai2/ProgramRepresentation/Parser/ProgramsParser.cs
    /**
     * 
     * parses a program, which consists of many programs
     * 
     */
    class ProgramParser {
        private enum EnumCommentState {
            NOTHING,
            INSTRING
        }

        private enum EnumParsingState {
            NOTHING,
            PARAMETERSEXPECTED,
            PARAMETERS,
            CODE
        }

        public class Parameter {
            public string name;
            public string description;

            public Parameter(string name, string description) {
                this.name = name;
                this.description = description;
            }
        }

        public class Program {
            public string code;
            public List<Parameter> parameters = new List<Parameter>();
            public List<string> path;
        }

        public List<Program> parse(List<string> lines) {
            EnumParsingState parsingState;
            int i;
            Program currentProgram = new Program();
            List<Program> resultPrograms = new List<Program>();


            parsingState = EnumParsingState.NOTHING;

            for (i = 0; i < lines.Count; i++) {
                string readLine;
                string line;

                readLine = lines[i];

                line = removeComment(readLine);
                line = line.Trim();

                if (parsingState == EnumParsingState.NOTHING) {
                    if (line.Length > 7 && line.Substring(0, 7) == "program") {
                        currentProgram.path = parseStrings(line.Substring(7, line.Length - 7));
                        parsingState = EnumParsingState.PARAMETERS;
                    }
                    // else it is fine
                }
                else if (parsingState == EnumParsingState.PARAMETERS) {
                    if (line != "parameters") {
                        throw new Exception("parameters expected!");
                    }
                    parsingState = EnumParsingState.PARAMETERSEXPECTED;
                }
                else if (parsingState == EnumParsingState.PARAMETERSEXPECTED) {
                    if (line.Length == 0) {
                        throw new Exception("parameters must be a list without spaces");
                    }
                    // else we are here

                    // if there is at the beginning a space -> it must be a parameter
                    if (readLine[0] == ' ') {
                        List<string> parameterWithDescription;

                        parameterWithDescription = parseStrings(line);
                        if (parameterWithDescription.Count != 2) {
                            throw new Exception("parameter must be two strings (name and description)");
                        }

                        currentProgram.parameters.Add(new Parameter(parameterWithDescription[0], parameterWithDescription[1]));
                    }
                    else {
                        i--;
                        parsingState = EnumParsingState.CODE;
                    }
                }
                else if (parsingState == EnumParsingState.CODE) {
                    if (line == "") {
                        parsingState = EnumParsingState.NOTHING;

                        resultPrograms.Add(currentProgram);
                        currentProgram = new Program();
                    }
                    else {
                        currentProgram.code = currentProgram.code + readLine + " ";
                    }
                }
                else {
                    throw new Exception("Internal Error");
                }
            }

            if (parsingState != EnumParsingState.NOTHING) {
                throw new Exception("state is not startstate!");
            }

            return resultPrograms;
        }

        static private string removeComment(string line) {
            int i;
            EnumCommentState parsingState;
            string resultString;

            parsingState = EnumCommentState.NOTHING;

            resultString = "";

            for (i = 0; i < line.Length; i++) {
                if (parsingState == EnumCommentState.NOTHING) {
                    if (line[i] == ';') {
                        break;
                    }
                    else if (line[i] == ' ') {
                        continue;
                    }
                    else if (Misc.isLetter(line[i])) {
                        resultString += line[i];
                    }
                    else if (line[i] == '"') {
                        parsingState = EnumCommentState.INSTRING;
                        resultString += '"';
                    }
                    else {
                        resultString += line[i];
                    }
                }
                else if (parsingState == EnumCommentState.INSTRING) {
                    if (line[i] == '"') {
                        parsingState = EnumCommentState.NOTHING;
                        resultString += '"';
                    }
                    else {
                        resultString += line[i];
                    }
                }
                else {
                    throw new Exception("Internal Error!");
                }
            }

            return resultString;
        }

        static private List<string> parseStrings(string line) {
            string stringContent;
            int i;
            bool insideString;
            List<string> resultList;

            resultList = new List<string>();
            insideString = false;
            stringContent = ""; // to make the compiler happy

            for (i = 0; i < line.Length; i++) {
                if (insideString) {
                    if (line[i] == '"') {
                        insideString = false;
                        resultList.Add(stringContent);
                    }
                    else {
                        stringContent += line[i];
                    }
                }
                else {
                    if (line[i] == '"') {
                        insideString = true;
                        stringContent = "";
                    }
                    // else do nothing
                }
            }

            if (insideString) {
                throw new Exception("parsing error, open string!");
            }

            return resultList;
        }
    }
}