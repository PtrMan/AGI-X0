module misc.GenericSerilizer;

import std.traits : traitsIsArray = isArray, Unqual;

version(unittest) import std.stdio;

import serialisation.BitstreamWriter;
import serialisation.BitstreamReader;


private void serializeStaticArray(Type, size_t Size, BitstreamDestinationType)(Type[Size] array, BitstreamWriter!BitstreamDestinationType bitstreamWriter, ref bool successChained) {
   foreach( iterationByte; array ) {
      bitstreamWriter.addUint__n(iterationByte, Type.sizeof*8, successChained);
   }
}

private void serializeArray(Type, BitstreamDestinationType)(Type[] array, BitstreamWriter!BitstreamDestinationType bitstreamWriter, ref bool successChained) {
   if( array.length > (1<<16) - 1 ) {
      successChained = false;
      return;
   }

   const bool isStaticArray = __traits(isStaticArray, Type);
   writeln(isStaticArray);

   bitstreamWriter.addUint_2__4_8_12_16(array.length, successChained);

   foreach( iterationByte; array ) {
      bitstreamWriter.addUint__n(iterationByte, Type.sizeof*8, successChained);
   }
}

/*
private void serializeArray(BitstreamDestinationType)(string array, BitstreamWriter!BitstreamDestinationType bitstreamWriter, ref bool successChained) {
   
   foreach( iterationByte; array ) {
      bitstreamWriter.addUint__n(iterationByte, char.sizeof*8, successChained);
   }
}*/

/+
private Type deserializeArray(Type, BitstreamSourceType)(BitstreamReader!BitstreamSourceType bitstreamReader, ref bool successChained) {
   /*Type tempResult;
   alias DataType = Unqual!(typeof(tempResult[0]));//typeof(result[0]);
   DataType[] result;

   if( !successChained ) {
      return [];
   }

   size_t lengthOfResult = bitstreamReader.getUint_2__4_8_12_16(successChained);

   result.length = lengthOfResult;

   foreach( i; 0..lengthOfResult ) {
      result[i] = cast(DataType)bitstreamReader.getUint__n(DataType.sizeof*8, successChained);
   }

   return cast(Type)result;*/
   Type result;

   return result;
}+/


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




template GenericSerilizer(StructName, BitstreamDestinationType) {
   void serilize(ref StructName structParameter, ref bool successChained, BitstreamWriter!BitstreamDestinationType bitstreamWriter) {
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
         else if( isArray ) {
            static if( isStaticArray ) {
               serializeStaticArray(__traits(getMember, structParameter, membername), bitstreamWriter, successChained);
            }
            else {
               serializeArray(__traits(getMember, structParameter, membername), bitstreamWriter, successChained);
            }
         }
      }
   }
}

template GenericDeserilizer(StructName, BitstreamSourceType) {
   void deserilize(ref StructName structParameter, ref bool successChained, BitstreamReader!BitstreamSourceType bitstreamReader) {
      foreach( membername; __traits(allMembers, StructName) ) {
         const bool isScalar = __traits(isScalar, __traits(getMember, StructName, membername));
         const bool isArray = traitsIsArray!(typeof(__traits(getMember, StructName, membername)));

         static if( isScalar ) {
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
      GenericSerilizer!(XYZ, BitstreamDestination).serilize(serilizeStruct, successChained, bitstreamWriter);
      assert(successChained);
   }

   BitstreamSource bitstreamSource = new BitstreamSource();
   bitstreamSource.resetToArray(bitstreamDestination.dataAsUbyte);
   {
      XYZ deserilizeStruct;

      BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(bitstreamSource);
      bool successChained = true;
      GenericDeserilizer!(XYZ, BitstreamSource).deserilize(deserilizeStruct, successChained, bitstreamReader);
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
