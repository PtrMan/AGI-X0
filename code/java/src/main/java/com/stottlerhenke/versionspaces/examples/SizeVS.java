/**
 * SizeVS.java
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

import com.stottlerhenke.versionspaces.CompositeVS;
import com.stottlerhenke.versionspaces.Transform;
import com.stottlerhenke.versionspaces.VS;

/**
 * A version space that can learn a size given a reference bound.
 * 
 * The reference bound provides the context with which to learn a relative size,
 * rather than simply a fixed amount.
 * 
 * @author rcreswick
 */
public class SizeVS extends CompositeVS<Integer, Integer> {
   
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

   private static final Transform<Integer, Object, Integer, Integer> INT_TR = 
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
   
   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.CompositeVS#buildBackingVS()
    */
   @Override
   protected VS<Integer, Integer> buildBackingVS() {
      return VS.union(
               VS.transform(new RatioVS(), REL_TR), // transformed rel. vs.
               VS.transform(new IntVS(), INT_TR));  // type-transformed fixed size vs.
   }


}
