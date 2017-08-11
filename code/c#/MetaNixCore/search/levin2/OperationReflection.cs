using System;
using System.Linq;

namespace MetaNix.search.levin2 {
    // uses C# reflection facilities to create Operations
    public static class OperationReflection {
        
        // from which class should the method be taken
        public enum EnumFunctionClassType {
            ARRAYOPERATIONS,
            OPERATIONS,
            ARRAYOPERATIONSTWOARGUMENTWRAPPER,
            OPERATIONSTWOARGUMENTWRAPPER,
        }

        public static TwoArgumentOperationCaller create(EnumFunctionClassType classType, string functionName, int value0, int value1) {
            Type type = null;
            
            switch(classType) {
                case EnumFunctionClassType.ARRAYOPERATIONS:
                type = typeof(ArrayOperations);
                break;

                case EnumFunctionClassType.OPERATIONS:
                type = typeof(Operations);
                break;

                case EnumFunctionClassType.ARRAYOPERATIONSTWOARGUMENTWRAPPER:
                type = typeof(ArrayOperationsTwoArgumentWrapper);
                break;

                case EnumFunctionClassType.OPERATIONSTWOARGUMENTWRAPPER:
                type = typeof(OperationsTwoArgumentWrapper);
                break;
            }

            var methodInfos = type.GetMethods(System.Reflection.BindingFlags.Static);
            var methodInfo = methodInfos.Where(v => v.Name == functionName).GetEnumerator().Current;
            var @delegate = (TwoArgumentOperationCaller.FunctionDelegateType)Delegate.CreateDelegate(type, methodInfo);
            
            return new TwoArgumentOperationCaller(@delegate, value0, value1);
        }
    }
}
