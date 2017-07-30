using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConsoleApplication1 {
    public class ReflectingSerialization {
        public static void serialize(object @object, BitstreamWriter sink, ref bool successChained, Serialization.EnumType serializationType) {
            FieldInfo[] fields = @object.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            var fieldsWithSerializationAttributes = fields.Where(v => v.GetCustomAttributes<Serialization>().Count() > 0);
            var fieldsWithSerializationAttributesSorted = fieldsWithSerializationAttributes.OrderBy(v => v.Name);
            foreach (var iField in fieldsWithSerializationAttributes) {
                // filter for serialization type
                var filteredSerializableAttributes = iField.GetCustomAttributes<Serialization>().Where(v => v.type == serializationType);
                
                var serializationAttributes = new List<Serialization>(filteredSerializableAttributes);
                if (serializationAttributes.Count == 0) {
                    continue;
                }
                else if (serializationAttributes.Count > 1) {
                    throw new Exception("Only up to one serialization attribute is supported!"); // until now
                }

                Serialization serializationAttribute = serializationAttributes[0];

                // serialize
                serialize(@object, iField, serializationAttribute, sink, successChained);
            }
        }

        private static void serialize(object @object, FieldInfo field, Serialization serializationAttribute, BitstreamWriter sink, bool successChained) {
            object value = field.GetValue(@object);

            if (value is float) {
                sink.addFloat32((float)value, ref successChained);
            }
            else if (value is double) {
                sink.addFloat64((double)value, ref successChained);
            }
            else if (value is uint) {
                if (serializationAttribute.numberOfBits <= 0) {
                    throw new Exception("Number of bits must be set to an valid value!");
                }
                
                sink.addUint__n((uint)value, (uint)serializationAttribute.numberOfBits, ref successChained);
            }
            else if (value is int) {
                if (serializationAttribute.numberOfBits <= 0) {
                    throw new Exception("Number of bits must be set to an valid value!");
                }
                
                sink.addInt__n((int)value, (uint)serializationAttribute.numberOfBits, ref successChained);
            }
            // TODO < string >
            else {
                throw new Exception("Unsupported type for serialization!");
            }
        }
    }

    public class Serialization : System.Attribute {
        public int numberOfBits = -1;

        public enum EnumType {
            DISK,
            // NETWORK, 
        }

        public Serialization() {}
        public Serialization(EnumType type, int numberOfBits = -1) {
            this.type = type;
            this.numberOfBits = numberOfBits;
        }

        public readonly EnumType type;
    }
}
