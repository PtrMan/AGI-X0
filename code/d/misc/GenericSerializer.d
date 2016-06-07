module misc.GenericSerializer;

import std.traits :
   traitsIsStaticArray = isStaticArray,
   traitsIsDynamicArray = isDynamicArray,
   traitsIsAggregateType = isAggregateType,
   traitsIsArray = isArray,
   traitsIsBoolean = isBoolean,
   Unqual;

version(unittest) import std.stdio;

import serialisation.BitstreamWriter;
import serialisation.BitstreamReader;

private void serializeArray(Type, BitstreamDestinationType)(Type array, BitstreamWriter!BitstreamDestinationType bitstreamWriter, ref bool successChained) {
   alias DataType = Unqual!(typeof(array[0]));

   const bool isStaticArray = __traits(isStaticArray, Type);
   static if( !isStaticArray ) {
      if( array.length > (1<<16) - 1 ) {
         successChained = false;
         return;
      }

      bitstreamWriter.addUint_2__4_8_12_16(array.length, successChained);
   }

   foreach( iterationElement; array ) {
      serialize(iterationElement, successChained, bitstreamWriter);
   }
}

private Type deserializeArray(Type, BitstreamSourceType)(BitstreamReader!BitstreamSourceType bitstreamReader, ref bool successChained) {
   Type tempResult;
   alias DataType = Unqual!(typeof(tempResult[0]));

   const bool isStaticArray = __traits(isStaticArray, Type);
   static if( isStaticArray ) {
      Type result;

      foreach( i; 0..result.length ) {
         DataType data;
         deserialize(data, successChained, bitstreamReader);
         result[i] = data;
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
         DataType data;
         deserialize(data, successChained, bitstreamReader);
         result[i] = data;
      }

      return cast(Type)result;
   }

}





void serialize(StructName, BitstreamDestinationType)(ref StructName structParameter, ref bool successChained, BitstreamWriter!BitstreamDestinationType bitstreamWriter) {
   const bool isScalar = __traits(isScalar, StructName);
   const bool isAggregateType = traitsIsAggregateType!StructName;
   const bool isDynamicArray = traitsIsDynamicArray!StructName;
   const bool isStaticArray = traitsIsStaticArray!StructName;

   version(unittest) {
      writeln("isScalar        ", isScalar);
      writeln("isAggregateType ", isAggregateType);
      writeln("isDynamicArray  ", isDynamicArray);
      writeln("isStaticArray   ", isStaticArray);
   }
   

   static if( isScalar ) {
      static if( traitsIsBoolean!StructName ) {
         bitstreamWriter.addBoolean(structParameter);
      }
      else {
         enum attributes = __traits(getAttributes, structParameter);

         version(unittest) writeln("length of attributes ", attributes.length);

         static if( attributes.length == 2 ) {
            const string type = __traits(getAttributes, structParameter)[0];

            static if( type == "u" ) {
               version(unittest) writeln("yes");

               const uint bitCount = __traits(getAttributes, structParameter)[1];

               // TODO< check bitcount for plausibility >

               version(unittest) writeln(bitCount);

               bitstreamWriter.addUint__n(structParameter, bitCount, successChained);
            }
         }
         else if( attributes.length == 0 ) {
            bitstreamWriter.addUint__n(structParameter, StructName.sizeof*8, successChained);
         }
      }
   }
   else static if( isAggregateType ) {
      foreach( membername; __traits(allMembers, StructName) ) {
         serialize(__traits(getMember, structParameter, membername), successChained, bitstreamWriter);
      }
   }
   else static if( isStaticArray || isDynamicArray ) {
      serializeArray(structParameter, bitstreamWriter, successChained);
   }
   else {
      //static assert(false);
   }

}



void deserialize(StructName, BitstreamSourceType)(ref StructName structParameter, ref bool successChained, BitstreamReader!BitstreamSourceType bitstreamReader) {
   const bool isScalar = __traits(isScalar, StructName);
   const bool isAggregateType = traitsIsAggregateType!StructName;
   const bool isDynamicArray = traitsIsDynamicArray!StructName;
   const bool isStaticArray = traitsIsStaticArray!StructName;

   static if( isScalar ) {
      static if( traitsIsBoolean!StructName ) {
         structParameter = cast(typeof(structParameter))bitstreamReader.getBoolean(successChained);
      }
      else {
         enum attributes = __traits(getAttributes, structParameter);

         version(unittest) writeln("length of attributes ", attributes.length);

         static if( attributes.length == 2 ) {
            const string type = __traits(getAttributes, structParameter)[0];

            static if( type == "u" ) {
               version(unittest) writeln("yes");

               const uint bitCount = __traits(getAttributes, structParameter)[1];

               // TODO< check bitcount for plausibility >

               version(unittest) writeln(bitCount);

               structParameter = bitstreamReader.getUint__n(bitCount, successChained);
            }
         }
         else if( attributes.length == 0 ) {
            structParameter = cast(StructName)bitstreamReader.getUint__n(structParameter.sizeof*8, successChained);
         }
      }
   }
   else static if( isAggregateType ) {
      foreach( membername; __traits(allMembers, StructName) ) {
         deserialize(__traits(getMember, structParameter, membername), successChained, bitstreamReader);
      }
   }
   else static if( isStaticArray || isDynamicArray ) {
      structParameter = deserializeArray!(typeof(structParameter), BitstreamSourceType)(bitstreamReader, successChained);
   }
   else {
      //static assert(false);
   }
}


// serialisation

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
      serialize(serilizeStruct, successChained, bitstreamWriter);
      assert(successChained);
   }

   
   BitstreamSource bitstreamSource = new BitstreamSource();
   bitstreamSource.resetToArray(bitstreamDestination.dataAsUbyte);
   {
      XYZ deserilizeStruct;

      BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(bitstreamSource);
      bool successChained = true;
      deserialize(deserilizeStruct, successChained, bitstreamReader);
      assert(successChained);

      
      foreach(i; 0..16) {
         assert(serilizeStruct.x[i] == i+1);
      }

      writeln(deserilizeStruct.array0.length);

      assert(deserilizeStruct.array0.length == 2);
      assert(deserilizeStruct.array0[0] == 5);
      assert(deserilizeStruct.array0[1] == 9);

      assert(deserilizeStruct.xx == 42);
      assert(deserilizeStruct.xy == 43);
      assert(deserilizeStruct.testString == "test005");
   }
}

// nested structs

unittest {
   import misc.BitstreamDestination;
   import misc.BitstreamSource;

   struct Nested {
      uint[] array0;
      string testString;
      uint xx;
   }

   struct XYZ {
      Nested nested;
   }

   XYZ serilizeStruct;

   serilizeStruct.nested.array0 ~= 5;
   serilizeStruct.nested.array0 ~= 9;

   serilizeStruct.nested.xx = 42;
   serilizeStruct.nested.testString = "test005";

   
   BitstreamDestination bitstreamDestination = new BitstreamDestination();
   {
      // serialize
      BitstreamWriter!BitstreamDestination bitstreamWriter = new BitstreamWriter!BitstreamDestination(bitstreamDestination);
      bool successChained = true;
      serialize(serilizeStruct, successChained, bitstreamWriter);
      assert(successChained);
   }

   
   BitstreamSource bitstreamSource = new BitstreamSource();
   bitstreamSource.resetToArray(bitstreamDestination.dataAsUbyte);
   {
      XYZ deserilizeStruct;

      BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(bitstreamSource);
      bool successChained = true;
      deserialize(deserilizeStruct, successChained, bitstreamReader);
      assert(successChained);

      assert(deserilizeStruct.nested.array0.length == 2);
      assert(deserilizeStruct.nested.array0[0] == 5);
      assert(deserilizeStruct.nested.array0[1] == 9);

      assert(deserilizeStruct.nested.xx == 42);
      assert(deserilizeStruct.nested.testString == "test005");
   }
}
