using System;
using System.Diagnostics;

namespace MetaNix.framework.pattern.withDecoration {
    // convert a string to a decorated pattern
    public static class Conversion {
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

        public static uint[] convertToUintArray(Pattern<Decoration> pattern) {
            Interpreter.vmAssert(pattern.decoration == null, false, "Must not have a decoration");
            Interpreter.vmAssert(pattern.isBranch, false, "Must be array");

            uint[] result = new uint[pattern.referenced.Length];
            for( int i = 0; i < pattern.referenced.Length; i++ ) {
                result[i] = convertToUint(pattern.referenced[i]);
            }
            return result;
        }

        public static int[] convertToIntArray(Pattern<Decoration> pattern) {
            Interpreter.vmAssert(pattern.decoration == null, false, "Must not have a decoration");
            Interpreter.vmAssert(pattern.isBranch, false, "Must be array");

            int[] result = new int[pattern.referenced.Length];
            for (int i = 0; i < pattern.referenced.Length; i++) {
                result[i] = convertToInt(pattern.referenced[i]);
            }
            return result;
        }


        public static int?[] convertToOptionalIntArray(Pattern<Decoration> pattern) {
            Interpreter.vmAssert(pattern.decoration == null, false, "Must not have a decoration");
            Interpreter.vmAssert(pattern.isBranch, false, "Must be array");

            int?[] result = new int?[pattern.referenced.Length];
            for (int i = 0; i < pattern.referenced.Length; i++) {
                result[i] = convertToOptionalInt(pattern.referenced[i]);
            }
            return result;
        }

        public static uint convertToUint(Pattern<Decoration> pattern) {
            Interpreter.vmAssert(pattern.@is(Pattern<Decoration>.EnumType.DECORATEDVALUE), false, "Must have a decoration");
            Interpreter.vmAssert(pattern.decoration != null, false, "Must have a decoration");

            Interpreter.vmAssert(pattern.decoration.type == Decoration.EnumType.VALUE, false, "Must be a value");
            Interpreter.vmAssert(pattern.decoration.value is long, false, "Must be long");

            Interpreter.vmAssert((long)pattern.decoration.value >= 0, false, "Must be >= 0");

            try {
                return Convert.ToUInt32(pattern.decoration.value);
            }
            catch (OverflowException e) {
                throw new Exception("Value overflow!"); // TODO< rewrite to except which is thrown by vmAssert() >
            }
        }

        public static int convertToInt(Pattern<Decoration> pattern) {
            Interpreter.vmAssert(pattern.@is(Pattern<Decoration>.EnumType.DECORATEDVALUE), false, "Must have a decoration");
            Interpreter.vmAssert(pattern.decoration != null, false, "Must have a decoration");

            Interpreter.vmAssert(pattern.decoration.type == Decoration.EnumType.VALUE, false, "Must be a value");
            Interpreter.vmAssert(pattern.decoration.value is long, false, "Must be long");
            
            try {
                return Convert.ToInt32(pattern.decoration.value);
            }
            catch( OverflowException e ) {
                throw new Exception("Value overflow!"); // TODO< rewrite to except which is thrown by vmAssert() >
            }
        }

        public static int? convertToOptionalInt(Pattern<Decoration> pattern) {
            if( pattern.@is(Pattern<Decoration>.EnumType.DECORATEDVALUE) ) {
                Interpreter.vmAssert(pattern.decoration != null, false, "Must have a decoration");

                Interpreter.vmAssert(pattern.decoration.type == Decoration.EnumType.VALUE, false, "Must be a value");
                Interpreter.vmAssert(pattern.decoration.value is long, false, "Must be long");
                
                try {
                    return Convert.ToInt32(pattern.decoration.value);
                }
                catch (OverflowException e) {
                    throw new Exception("Value overflow!"); // TODO< rewrite to except which is thrown by vmAssert() >
                }
            }
            else if( pattern.@is(Pattern<Decoration>.EnumType.SYMBOL) ) { // special symbol for "null"
                // 0 is reserved for the "null" word
                Interpreter.vmAssert(pattern.uniqueId == 0, false, "Special uniqueId equal to 0 expected to encode \"null\"");

                return null;
            }
            else {
                Interpreter.vmAssert(false, false, "expected value or special symbol");
                throw new Exception(); // make compiler happy
            }
        }
    }
}
