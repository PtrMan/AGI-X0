// typesafe function id's
namespace MetaNix.dispatch {
    public struct PublicFunctionId {
        public long value;

        public static PublicFunctionId make(long value) {
            PublicFunctionId result;
            result.value = value;
            return result;
        }

        public static bool operator ==(PublicFunctionId a, PublicFunctionId b) {
            return a.value == b.value;
        }

        public static bool operator !=(PublicFunctionId a, PublicFunctionId b) {
            return a.value != b.value;
        }
    }

    // hidden function IDs are ot directly callable from the outside
    public struct HiddenFunctionId {
        public long value;

        public static HiddenFunctionId make(long value) {
            HiddenFunctionId result;
            result.value = value;
            return result;
        }

        public static bool operator ==(HiddenFunctionId a, HiddenFunctionId b) {
            return a.value == b.value;
        }

        public static bool operator !=(HiddenFunctionId a, HiddenFunctionId b) {
            return a.value != b.value;
        }
    }
}
