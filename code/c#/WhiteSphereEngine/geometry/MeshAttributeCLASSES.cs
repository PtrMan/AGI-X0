using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WhiteSphereEngine.geometry {
    // A isomporphism (in enlish it is equal to) for a buffer for a attribute of an vertex of an mesh
    // can be mutable or immutable to allow the engine to optimize certain things
    public abstract class AbstractMeshAttribute {
        public enum EnumDataType {
            FLOAT4,
            FLOAT2,
            DOUBLE4,
            UINT32,
        }

        public enum EnumType {
            MUTABLE,
            IMMUTABLE
        }


        protected AbstractMeshAttribute(EnumDataType dataType, EnumType type) {
            this.dataType = dataType;
            this.type = type;
        }

        public abstract class Float4Accessor {
            public abstract float[] this[int index] {
                get; set;
            }
        }

        public abstract class Float2Accessor {
            public abstract float[] this[int index] {
                get; set;
            }
        }

        public abstract class Double4Accessor {
            public abstract double[] this[int index] {
                get; set;
            }
        }

        public abstract class Uint32Accessor {
            public abstract uint[] this[int index] {
                get; set;
            }
        }

        public abstract Float4Accessor getFloat4Accessor();
        public abstract Float2Accessor getFloat2Accessor();
        public abstract Double4Accessor getDouble4Accessor();
        public abstract Uint32Accessor getUint32Accessor();

        public abstract uint length {
            get;
        }
        
        public readonly EnumDataType dataType;
        public readonly EnumType type;
    }

    // A isomporphism (in enlish it is equal to) for a buffer for a attribute of an vertex of an mesh
    // is immutable, which allows the engine to potentially do some optimisations
    public class MeshAttributeCLASSES : AbstractMeshAttribute {
        // not static
        public class Double4Accessor : AbstractMeshAttribute.Double4Accessor {
            public Double4Accessor(MeshAttributeCLASSES parent) {
                this.parent = parent;
            }

            public override double[] this[int index] {
                get {
                    Debug.Assert(index < parent.length);
                    // no need to check type because this accessor can only get retrived by getDouble4Accessor

                    double[] result = new double[4];

                    IntPtr indexedPtr = IntPtr.Add(parent.pointer, SIZEOFDOUBLE * 4 * index);
                    Marshal.Copy(indexedPtr, result, 0, 4);

                    return result;
                }

                set {
                    throw new Exception("set accessor invalid because ImmutableMeshComponent!");
                }
            }

            MeshAttributeCLASSES parent;
        }

        public static MeshAttributeCLASSES makeDouble4(double[][] arr) {
            uint length_ = (uint)arr.Length;

            
            MeshAttributeCLASSES created = new MeshAttributeCLASSES(AbstractMeshAttribute.EnumDataType.DOUBLE4, length_);
            created.pointer = Marshal.AllocHGlobal(SIZEOFDOUBLE * 4 * (int)length_);

            // copy to unmanaged memory
            for (int i = 0; i < arr.Length; i++) {
                Trace.Assert(arr[i].Length == 4);

                IntPtr indexedPtr = IntPtr.Add(created.pointer, SIZEOFDOUBLE * i * 4);
                Marshal.Copy(arr[i], 0, indexedPtr, 4);
            }

            return created;
        }
        
        public override AbstractMeshAttribute.Float4Accessor getFloat4Accessor() {
            throw new NotImplementedException();
        }

        public override AbstractMeshAttribute.Float2Accessor getFloat2Accessor() {
            throw new NotImplementedException();
        }

        public override AbstractMeshAttribute.Double4Accessor getDouble4Accessor() {
            if( dataType != EnumDataType.DOUBLE4 ) {
                throw new Exception("Invalid datatype");
            }

            return new Double4Accessor(this);
        }

        public override AbstractMeshAttribute.Uint32Accessor getUint32Accessor() {
            throw new NotImplementedException();
        }

        private MeshAttributeCLASSES(AbstractMeshAttribute.EnumDataType datatype, uint length_) : base(datatype, EnumType.IMMUTABLE) {
            privateLength = length_;
        }

        IntPtr pointer;
        uint privateLength;

        const int SIZEOFDOUBLE = 8;

        public override uint length => privateLength;
    }


    // A isomporphism (in enlish it is equal to) for a buffer for a attribute of an vertex of an mesh
    public class MutableMeshAttribute : AbstractMeshAttribute {
        // not static
        public class Double4Accessor : AbstractMeshAttribute.Double4Accessor {
            public Double4Accessor(MutableMeshAttribute parent) {
                this.parent = parent;
            }

            public override double[] this[int index] {
                get {
                    Debug.Assert(index < parent.length);
                    // no need to check type because this accessor can only get retrived by getDouble4Accessor

                    double[] result = new double[4];

                    IntPtr indexedPtr = IntPtr.Add(parent.pointer, SIZEOFDOUBLE * 4 * index);
                    Marshal.Copy(indexedPtr, result, 0, 4);

                    return result;
                }

                set {
                    Debug.Assert(index < parent.length);
                    // no need to check type because this accessor can only get retrived by getDouble4Accessor
                    
                    IntPtr indexedPtr = IntPtr.Add(parent.pointer, SIZEOFDOUBLE * 4 * index);
                    Marshal.Copy(value, 0, indexedPtr, 4);
                }
            }

            MutableMeshAttribute parent;
        }

        public static MutableMeshAttribute makeDouble4(double[][] arr) {
            uint length_ = (uint)arr.Length;


            MutableMeshAttribute created = new MutableMeshAttribute(AbstractMeshAttribute.EnumDataType.DOUBLE4, length_);
            created.pointer = Marshal.AllocHGlobal(SIZEOFDOUBLE * 4 * (int)length_);

            // copy to unmanaged memory
            for (int i = 0; i < arr.Length; i++) {
                Trace.Assert(arr[i].Length == 4);

                IntPtr destinationPtr = IntPtr.Add(created.pointer, SIZEOFDOUBLE * i * 4);
                Marshal.Copy(arr[i], 0, destinationPtr, 4);
            }

            return created;
        }
        
        public static MutableMeshAttribute makeDouble4ByLength(uint length_) {
            MutableMeshAttribute created = new MutableMeshAttribute(AbstractMeshAttribute.EnumDataType.DOUBLE4, length_);
            created.pointer = Marshal.AllocHGlobal(SIZEOFDOUBLE * 4 * (int)length_);

            double[] initArr = new double[4];

            // copy to unmanaged memory
            for (int i = 0; i < length_; i++) {
                IntPtr indexedPtr = IntPtr.Add(created.pointer, SIZEOFDOUBLE * i * 4);
                Marshal.Copy(initArr, 0, indexedPtr, 4);
            }

            return created;
        }

        public override AbstractMeshAttribute.Float4Accessor getFloat4Accessor() {
            throw new NotImplementedException();
        }

        public override AbstractMeshAttribute.Float2Accessor getFloat2Accessor() {
            throw new NotImplementedException();
        }

        public override AbstractMeshAttribute.Double4Accessor getDouble4Accessor() {
            if (dataType != EnumDataType.DOUBLE4) {
                throw new Exception("Invalid datatype");
            }

            return new Double4Accessor(this);
        }

        public override AbstractMeshAttribute.Uint32Accessor getUint32Accessor() {
            throw new NotImplementedException();
        }

        private MutableMeshAttribute(AbstractMeshAttribute.EnumDataType datatype, uint length_) : base(datatype, EnumType.MUTABLE) {
            privateLength = length_;
        }

        IntPtr pointer;
        uint privateLength;

        const int SIZEOFDOUBLE = 8;

        public override uint length => privateLength;
    }
}
