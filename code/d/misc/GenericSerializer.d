module misc.GenericSerializer;

import std.traits : traitsIsArray = isArray, traitsIsBoolean = isBoolean,  Unqual;

version(unittest) import std.stdio;

import serialisation.BitstreamWriter;
import serialisation.BitstreamReader;


private void serializeStaticArray(Type, size_t Size, BitstreamDestinationType)(Type[Size] array, BitstreamWriter!BitstreamDestinationType bitstreamWriter, ref bool successChained) {
   import std.stdio;
   writeln("serializeStaticArray() array length=", Size);

   foreach( iterationElement; array ) {
      bitstreamWriter.addUint__n(iterationElement, Type.sizeof*8, successChained);
   }
}


private void serializeArray(Type, BitstreamDestinationType)(Type[] array, BitstreamWriter!BitstreamDestinationType bitstreamWriter, ref bool successChained) {
   if( array.length > (1<<16) - 1 ) {
      successChained = false;
      return;
   }

   const bool isStaticArray = __traits(isStaticArray, Type);

   bitstreamWriter.addUint_2__4_8_12_16(array.length, successChained);

   foreach( iterationElement; array ) {
      bitstreamWriter.addUint__n(iterationElement, Type.sizeof*8, successChained);
   }
}


private Type deserializeArray(Type, BitstreamSourceType)(BitstreamReader!BitstreamSourceType bitstreamReader, ref bool successChained) {
   Type tempResult;
   alias DataType = Unqual!(typeof(tempResult[0]));

   const bool isStaticArray = __traits(isStaticArray, Type);
   static if( isStaticArray ) {
      Type result;

      foreach( i; 0..result.length ) {
         result[i] = cast(DataType)bitstreamReader.getUint__n(DataType.sizeof*8, successChained);
      }

      return result;
   }
   else {
      DataType[] result;

      if( !successChained ) {
         return [];
      }

      size_t lengthOfResult = bitstreamReader.getUint_2__4_8_12_16(successChained);

      result.length = lengthOfResult;

      foreach( i; 0..lengthOfResult ) {
         result[i] = cast(DataType)bitstreamReader.getUint__n(DataType.sizeof*8, successChained);
      }

      return cast(Type)result;
   }

}




template GenericSerializer(StructName, BitstreamDestinationType) {
   void serialize(ref StructName structParameter, ref bool successChained, BitstreamWriter!BitstreamDestinationType bitstreamWriter) {
      foreach( membername; __traits(allMembers, StructName) ) {
         version(unittest) writeln("===");

         const bool isScalar = __traits(isScalar, __traits(getMember, StructName, membername));
         const bool isStaticArray = __traits(isStaticArray, __traits(getMember, StructName, membername));
         const bool isArray = traitsIsArray!(typeof(__traits(getMember, StructName, membername)));

         version(unittest) {
            writeln("membername ", membername);
            writeln("isScalar   ", isScalar);
            writeln("isArray    ", isArray);
         }
         

         static if( isScalar ) {
            static if( traitsIsBoolean!(typeof(__traits(getMember, StructName, membername))) ) {
               bitstreamWriter.addBoolean(__traits(getMember, structParameter, membername));
            }
            else {
               enum attributes = __traits(getAttributes, __traits(getMember, StructName, membername));

               version(unittest) writeln("length of attributes ", attributes.length);

               static if( attributes.length == 2 ) {
                  const string type = __traits(getAttributes, __traits(getMember, StructName, membername))[0];

                  static if( type == "u" ) {
                     version(unittest) writeln("yes");

                     const uint bitCount = __traits(getAttributes, __traits(getMember, StructName, membername))[1];

                     // TODO< check bitcount for plausibility >

                     version(unittest) writeln(bitCount);

                     bitstreamWriter.addUint__n(__traits(getMember, structParameter, membername), bitCount, successChained);
                  }
               }
               else if( attributes.length == 0 ) {
                  bitstreamWriter.addUint__n(__traits(getMember, structParameter, membername), typeof(__traits(getMember, structParameter, membername)).sizeof*8, successChained);
               }
            }
         }
         else if( isArray ) {
            static if( isStaticArray ) {
               serializeStaticArray(__traits(getMember, structParameter, membername), bitstreamWriter, successChained);

               version(unittest) writeln("serialize static array");
            }
            else {
               serializeArray(__traits(getMember, structParameter, membername), bitstreamWriter, successChained);
            }
         }
      }
   }
}

template GenericDeserializer(StructName, BitstreamSourceType) {
   void deserialize(ref StructName structParameter, ref bool successChained, BitstreamReader!BitstreamSourceType bitstreamReader) {
      foreach( membername; __traits(allMembers, StructName) ) {
         const bool isScalar = __traits(isScalar, __traits(getMember, StructName, membername));
         const bool isArray = traitsIsArray!(typeof(__traits(getMember, StructName, membername)));

         static if( isScalar ) {
            static if( traitsIsBoolean!(typeof(__traits(getMember, StructName, membername))) ) {
               __traits(getMember, structParameter, membername) = bitstreamReader.getBoolean(successChained);
            }
            else {
               enum attributes = __traits(getAttributes, __traits(getMember, StructName, membername));

               version(unittest) writeln("length of attributes ", attributes.length);

               static if( attributes.length == 2 ) {
                  const string type = __traits(getAttributes, __traits(getMember, StructName, membername))[0];

                  static if( type == "u" ) {
                     version(unittest) writeln("yes");

                     const uint bitCount = __traits(getAttributes, __traits(getMember, StructName, membername))[1];

                     // TODO< check bitcount for plausibility >

                     version(unittest) writeln(bitCount);

                     __traits(getMember, structParameter, membername) = bitstreamReader.getUint__n(bitCount, successChained);
                  }
               }
               else if( attributes.length == 0 ) {
                  __traits(getMember, structParameter, membername) = bitstreamReader.getUint__n(typeof(__traits(getMember, structParameter, membername)).sizeof*8, successChained);
               }
            }
         }
         else if( isArray ) {
            __traits(getMember, structParameter, membername) = deserializeArray!(typeof(__traits(getMember, structParameter, membername)), BitstreamSourceType)(bitstreamReader, successChained);
         }

      }
   }
}


unittest {
   import misc.BitstreamDestination;
   import misc.BitstreamSource;

   struct XYZ {
      ubyte[16] x;
      uint xy;
      uint[] array0;
      uint xx;
      string testString;
   }

   XYZ serilizeStruct;

   foreach(i; 0..16) {
      serilizeStruct.x[i] = cast(ubyte)(i+1);
   }

   serilizeStruct.array0 ~= 5;
   serilizeStruct.array0 ~= 9;

   serilizeStruct.xx = 42;
   serilizeStruct.xy = 43;
   serilizeStruct.testString = "test005";

   
   BitstreamDestination bitstreamDestination = new BitstreamDestination();
   {
      // serialize
      BitstreamWriter!BitstreamDestination bitstreamWriter = new BitstreamWriter!BitstreamDestination(bitstreamDestination);
      bool successChained = true;
      GenericSerializer!(XYZ, BitstreamDestination).serialize(serilizeStruct, successChained, bitstreamWriter);
      assert(successChained);
   }

   BitstreamSource bitstreamSource = new BitstreamSource();
   bitstreamSource.resetToArray(bitstreamDestination.dataAsUbyte);
   {
      XYZ deserilizeStruct;

      BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(bitstreamSource);
      bool successChained = true;
      GenericDeserializer!(XYZ, BitstreamSource).deserialize(deserilizeStruct, successChained, bitstreamReader);
      assert(successChained);

      
      foreach(i; 0..16) {
         assert(serilizeStruct.x[i] == i+1);
      }

      assert(deserilizeStruct.array0.length == 2);
      assert(deserilizeStruct.array0[0] == 5);
      assert(deserilizeStruct.array0[1] == 9);

      assert(deserilizeStruct.xx == 42);
      assert(deserilizeStruct.xy == 43);
      assert(deserilizeStruct.testString == "test005");
   }
}
