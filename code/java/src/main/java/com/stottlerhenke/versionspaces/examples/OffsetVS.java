/**
 * OffsetVS.java
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
import com.stottlerhenke.versionspaces.Transform;
import com.stottlerhenke.versionspaces.VS;

/**
 * A version space that learns an offset in a given range.
 * 
 * The range is specified by the input Integer (0 to the input). The set of
 * hypotheses considered covers offsets from the front, offsets from the back,
 * and relative offsets with respect to the size of the input region.
 * 
 * @author rcreswick
 */
public class OffsetVS extends CompositeVS<Integer, Integer> {

   private static final Transform<Integer, Double, Integer, Double> REL_TR =
      new Transform<Integer, Double, Integer, Double>(){

         @Override
         public Double in(final Integer parentIn) {
            return parentIn.doubleValue();
         }

         @Override
         public Integer out(final Integer parentIn, final Double childOut) {
            return (int) Math.round(childOut);
         }

         @Override
         public Double train(final Integer parentIn, final Integer parentOut) {
            return parentOut.doubleValue();
         }
   };

   private static final Transform<Integer, Object, Integer, Integer> FRONT_INT_TR = 
      new Transform<Integer, Object, Integer, Integer>() {
      @Override
      public Object in(final Integer parentIn) {
         return parentIn;
      }

      @Override
      public Integer out(final Integer parentIn, final Integer childOut) {
         return childOut;
      }

      @Override
      public Integer train(final Integer parentIn, final Integer parentOut) {
         return parentOut;
      }
   };
   
   private static final Transform<Integer, Object, Integer, Integer> BACK_INT_TR = 
      new Transform<Integer, Object, Integer, Integer>() {
      @Override
      public Object in(final Integer parentIn) {
         return parentIn;
      }

      @Override
      public Integer out(final Integer parentIn, final Integer childOut) {
         return parentIn - childOut;
      }

      @Override
      public Integer train(final Integer parentIn, final Integer parentOut) {
         return parentIn - parentOut;
      }
   };

   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.CompositeVS#buildBackingVS()
    */
   @Override
   protected VS<Integer, Integer> buildBackingVS() {
      VS<Integer, Integer> front = VS.transform(new IntVS(), FRONT_INT_TR);
      VS<Integer, Integer> back = VS.transform(new IntVS(), BACK_INT_TR);
      VS<Integer, Integer> relative = VS.transform(new RatioVS(), REL_TR);
      
      return VS.union(ImmutableList.of(front, back, relative));
   }
}
