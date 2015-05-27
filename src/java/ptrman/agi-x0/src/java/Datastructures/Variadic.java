package Datastructures;

import ptrman.misc.Assert;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.ArrayList;
import java.util.List;

public class Variadic {
    public enum EnumType {
        INT,
        VECTOR2FLOAT,
        FLOAT,
        STORAGEALGORITHMICCONCEPTTREE,
        VOID,
        BOOL,
        ARRAY
    }

    public Variadic(EnumType type) {
        this.type = type;
    }

    public Variadic deepCopy() {
        Variadic result;
        result = new Variadic(type);
        result.valueInt = valueInt;
        result.valueVector2Float = valueVector2Float;
        result.valueFloat = valueFloat;
        result.valueBool = valueBool;
        if (type == EnumType.ARRAY) {
            result.valueArray = new ArrayList<Variadic>();
            for (Object __dummyForeachVar0 : valueArray) {
                Variadic iterationValue = (Variadic)__dummyForeachVar0;
                result.valueArray.add(iterationValue.deepCopy());
            }
        }
         
        if (valueTree != null) {
            result.valueTree = valueTree.deepCopy();
        }
         
        return result;
    }

    public EnumType type = EnumType.INT;
    public int valueInt;
    public Vector2d<Float> valueVector2Float;
    public float valueFloat;
    public TreeNode valueTree;
    public boolean valueBool;
    public List<Variadic> valueArray;
    public static boolean isEqual(Variadic a, Variadic b, double epsilon) {
        Assert.Assert(a.type == b.type, "");
        switch(a.type) {
            case BOOL: 
                return a.valueBool == b.valueBool;
            case FLOAT: 
                return java.lang.Math.abs(a.valueFloat - b.valueFloat) < epsilon;
            case INT: 
                return a.valueInt == b.valueInt;
            default: 
                throw new RuntimeException("isEqual is not applicable to lists, vectors, etc.");
        
        }
    }

    public String toString() {
        switch(type) {
            case BOOL: 
                return Boolean.toString(valueBool);
            case FLOAT: 
                return Float.toString(valueFloat);
            case INT: 
                return Integer.toString(valueInt);
            default: 
                throw new NotImplementedException();
        }
    }


    // throws an error when the type is not a numeric
    public static Variadic.EnumType highestNumericType(final List<Variadic.EnumType> types) {
        for( final Variadic.EnumType iterationType : types ) {
            if( iterationType == Variadic.EnumType.FLOAT ) {
                return Variadic.EnumType.FLOAT;
            }
            else if( iterationType == Variadic.EnumType.INT ) {
            }
            else {
                throw new RuntimeException("highestNumericType called with invalid type");
            }
        }

        return Variadic.EnumType.INT;
    }

    public static Variadic convertNumericTo(final Variadic value, final Variadic.EnumType targetType) {
        Variadic result = new Variadic(targetType);

        if( value.type == targetType ) {
            return value;
        }
        else if( value.type == EnumType.INT && targetType == EnumType.FLOAT ) {
            result.valueFloat = (float)value.valueInt;
        }
        else if( value.type == EnumType.FLOAT && targetType == EnumType.INT ) {
            result.valueInt = (int)value.valueFloat;
        }
        else {
            throw new RuntimeException("convertNumericTo with nonnumeric!");
        }

        return result;
    }
}
