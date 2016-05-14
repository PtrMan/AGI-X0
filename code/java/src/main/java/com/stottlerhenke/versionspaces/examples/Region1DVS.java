/**
 * Region1DVS.java
 * 
 * Copyright (c) 2009 Stottler Henke Associates, Inc.
 * 
 * This file is part of JVersionSpaces.
 *
 * JVersionSpaces is free software: you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation, either
 * version 3 of the License, or (at your option) any later version.
 *
 * JVersionSpaces is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with JVersionSpaces.  If not, see
 * <http://www.gnu.org/licenses/>.
 */
package com.stottlerhenke.versionspaces.examples;

import com.google.common.collect.ImmutableList;
import com.stottlerhenke.versionspaces.CompositeVS;
import com.stottlerhenke.versionspaces.MultiTransform;
import com.stottlerhenke.versionspaces.Pair;
import com.stottlerhenke.versionspaces.VS;
import com.stottlerhenke.versionspaces.examples.data.Anchor;
import com.stottlerhenke.versionspaces.examples.data.LocationSpec;
import com.stottlerhenke.versionspaces.examples.data.Region1D;

/**
 * A version space that represents the collection of possible 1-dimensional 
 * regions  ((start, end) pairs) with respect to another region.  Generally, the output 
 * will be contained by the input, but this is not strictly necessary.  
 * 
 * @author rcreswick
 */
public class Region1DVS extends CompositeVS<Region1D, Region1D> {

   private static final 
   MultiTransform<Region1D, Pair<Region1D, Integer>,
                  Region1D, Pair<LocationSpec, Integer>> 
   _regionTr = new MultiTransform<Region1D, Pair<Region1D,Integer>, 
                                  Region1D, Pair<LocationSpec,Integer>>(){
      @Override
      public Pair<Region1D, Integer> in(final Region1D parentIn) {
         int size = parentIn.getSize();
         return new Pair<Region1D, Integer>(parentIn, size);
      }
   
      @Override
      public Region1D out(final Region1D parentIn,
                                 final Pair<LocationSpec, Integer> childOut) {
         int start = 0;
         int end   = 0;
         
         int size = childOut.b;
         LocationSpec lspec = childOut.a;
         
         switch(childOut.a.getAnchor()){
         case Leading:
            start = lspec.getOffset();
            end   = lspec.getOffset() + size;
            break;
         case Trailing:
            start = lspec.getOffset() - size;
            end   = lspec.getOffset();
            break;
         case Centered:
            start = (int)(lspec.getOffset() - Math.round(size / 2.0));
            end   = (int)(lspec.getOffset() + Math.round(size / 2.0));
            break;
         default:
            assert false : "Unexpected Anchor: "+childOut.a;
         }
         
         return new Region1D(start, end);
      }

      @Override
      public ImmutableList<Pair<Pair<Region1D, Integer>, 
                                Pair<LocationSpec, Integer>>> 
      multitrain(final Region1D parentIn, final Region1D parentOut) {
         
         int outputSize = parentOut.getSize();
         int inputSize  = parentIn.getSize();
         
         int start = parentOut.getStart();
         int end   = parentOut.getEnd();
         int center = start + (int)Math.round(outputSize / 2.0);
         
         Pair<Region1D, Integer> inputPair = 
            new Pair<Region1D, Integer>(parentIn, inputSize);
         
         
         Pair<LocationSpec, Integer> outPair1 = 
            new Pair<LocationSpec, Integer>(
                  new LocationSpec(start, Anchor.Leading), outputSize);

         Pair<LocationSpec, Integer> outPair2 = 
            new Pair<LocationSpec, Integer>(
                  new LocationSpec(end, Anchor.Trailing), outputSize);
         
         Pair<LocationSpec, Integer> outPair3 = 
            new Pair<LocationSpec, Integer>(
                  new LocationSpec(center, Anchor.Centered), outputSize);
         
         return ImmutableList.<Pair<Pair<Region1D, Integer>, 
                                Pair<LocationSpec, Integer>>>of(
               new Pair<Pair<Region1D, Integer>, Pair<LocationSpec, Integer>>(
                     inputPair, outPair1),
               new Pair<Pair<Region1D, Integer>, Pair<LocationSpec, Integer>>(
                     inputPair, outPair2),
               new Pair<Pair<Region1D, Integer>, Pair<LocationSpec, Integer>>(
                     inputPair, outPair3));
      }
   };

   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.CompositeVS#buildBackingVS()
    */
   @Override
   protected VS<Region1D, Region1D> buildBackingVS() {
      VS<Region1D, LocationSpec> location = new LocationVS();
      VS<Integer, Integer> size = new SizeVS();
      
      return VS.transform(VS.join(location, size), _regionTr);
   }
}
