using System;
using System.Collections.Generic;

namespace Datastructures
{
    class Variadic
    {
        public enum EnumType
        {
            INT,
            VECTOR2FLOAT,
            FLOAT,
            STORAGEALGORITHMICCONCEPTTREE,
            VOID,
            BOOL,
            ARRAY
        }

        public Variadic(EnumType type)
        {
            this.type = type;
        }

        public Variadic deepCopy()
        {
            Variadic result;

            result = new Variadic(type);
            result.valueInt = valueInt;
            result.valueVector2Float = valueVector2Float;
            result.valueFloat = valueFloat;
            result.valueBool = valueBool;

            if( type == EnumType.ARRAY )
            {
                result.valueArray = new List<Variadic>();

                foreach( Variadic iterationValue in valueArray )
                {
                    result.valueArray.Add(iterationValue.deepCopy());
                }
            }

            if( valueTree != null )
            {
                result.valueTree = valueTree.deepCopy();
            }
            
            return result;
        }

        public EnumType type;
        public int valueInt;
        public Vector2<float> valueVector2Float;
        public float valueFloat;
        public TreeNode valueTree;
        public bool valueBool;
        public List<Variadic> valueArray;

        public static bool isEqual(Variadic a, Variadic b, double epsilon)
        {
            System.Diagnostics.Debug.Assert(a.type == b.type);

            switch( a.type )
            {
                case EnumType.BOOL:
                return a.valueBool == b.valueBool;

                case EnumType.FLOAT:
                return System.Math.Abs(a.valueFloat - b.valueFloat) < epsilon;

                case EnumType.INT:
                return a.valueInt == b.valueInt;

                default:
                throw new Exception("isEqual is not applicable to lists, vectors, etc.");
            }
        }

        public string toString()
        {
            switch( type )
            {
                case EnumType.BOOL:
                return valueBool.ToString();

                case EnumType.FLOAT:
                return valueFloat.ToString();

                case EnumType.INT:
                return valueInt.ToString();

                default:
                // TODO
                throw new NotImplementedException();
            }
        }
    }
}
