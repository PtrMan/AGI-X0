using System.Diagnostics;

namespace MetaNix.framework.pattern.withDecoration {
    // convert a string to a decorated pattern
    public static class StringHelper {
        public static Pattern<Decoration> convert(string str, ulong patternUniqueId) {
            Pattern<Decoration> result = Pattern<Decoration>.makeBranch(patternUniqueId);
            result.decoration = new Decoration();
            result.decoration.type = Decoration.EnumType.STRING;
            result.referenced = new Pattern<Decoration>[str.Length];

            // buildstring
            for( int i = 0; i < str.Length; i++ ) {
                result.referenced[i] = new Pattern<Decoration>();
                result.referenced[i].decoration = new Decoration();
                result.referenced[i].decoration.type = Decoration.EnumType.VALUE;
                result.referenced[i].decoration.value = (long)str[i];
            }

            return result;
        }

        public static string convertPatternToString(Pattern<Decoration> pattern) {
            Trace.Assert(pattern.decoration != null, "Must have decoration"); // hard Trace-assert because it is a deep bug if this is violated
            Interpreter.vmAssert(pattern.decoration.type == Decoration.EnumType.STRING, false, "Must be string");
            Interpreter.vmAssert(pattern.isBranch, false, "String must be branch!");

            string result = "";

            for( int i = 0; i < pattern.referenced.Length; i++ ) {
                Pattern<Decoration> letterPattern = pattern.referenced[i];
                long letterValue = Interpreter.retriveLong(letterPattern);
                result += (char)letterValue;
            }

            return result;
        }
    }
}
