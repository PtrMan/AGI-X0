using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix {
    public struct Variant {
        public enum EnumType {
            INT, // 64 bit
            FLOAT, // double
            STRING,
        }

        Variant(EnumType type) {
            this.privateType = type;
            privateValueInt = 0;
            privateValueFloat = 0;
            privateValueString = "";
        }

        public static Variant makeInt(long value) {
            Variant result = new Variant(EnumType.INT);
            result.valueInt = value;
            return result;
        }

        public static Variant makeFloat(double value) {
            Variant result = new Variant(EnumType.FLOAT);
            result.valueFloat = value;
            return result;
        }

        public static Variant makeString(string value) {
            Variant result = new Variant(EnumType.STRING);
            result.valueString = value;
            return result;
        }

        EnumType privateType;
        long privateValueInt;

        internal bool checkEquality(Variant other) {
            if(type != other.type) {
                return false;
            }

            if(type == EnumType.FLOAT) {
                return valueFloat == other.valueFloat;
            }
            else if(type == EnumType.INT) {
                return valueInt == other.valueInt;
            }
            else if(type == EnumType.STRING) {
                return valueString == other.valueString;
            }
            else {
                throw new Exception("Iternal error");
            }
        }

        double privateValueFloat;
        string privateValueString;

        public long valueInt {
            get {
                Ensure.ensure(privateType == EnumType.INT);
                return privateValueInt;
            }
            set {
                privateType = EnumType.INT;
                privateValueInt = value;
            }
        }

        public double valueFloat {
            get {
                Ensure.ensure(privateType == EnumType.FLOAT);
                return privateValueFloat;
            }
            set {
                privateType = EnumType.FLOAT;
                privateValueFloat = value;
            }
        }

        public string valueString {
            get {
                Ensure.ensure(privateType == EnumType.STRING);
                return privateValueString;
            }
            set {
                privateType = EnumType.STRING;
                privateValueString = value;
            }
        }

        public EnumType type {
            get {
                return privateType;
            }
        }
    }
}
