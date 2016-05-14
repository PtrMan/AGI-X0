/**
 * MinFixedIntVS.java
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

import com.google.common.collect.Iterators;
import com.google.common.collect.UnmodifiableIterator;
import com.stottlerhenke.versionspaces.ConfidentHypothesis;
import com.stottlerhenke.versionspaces.Hypothesis;
import com.stottlerhenke.versionspaces.VS;

/**
 * A version space that learns the smallest fixed value it has been trained with.
 * 
 * This version space ignores input, returning a fixed value that  
 * depends only on the training data provided.
 * 
 * @author rcreswick
 * 
 * @see MaxFixedIntVS
 */
public class MinFixedIntVS extends VS<Object, Integer> {
   private final Integer _lowerBound = Integer.MIN_VALUE;
   
   private Integer _upperBound = Integer.MAX_VALUE;
   
   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.trial.VS#hypotheses()
    */
   @Override
   public
   UnmodifiableIterator<ConfidentHypothesis<Object, Integer>> 
   hypotheses() {
      final Integer value = _upperBound;
      Hypothesis<Object,Integer> h = new Hypothesis<Object, Integer> () {
         @Override
         public Integer eval(final Object in) {
            return value;
         }
      };
      
      return Iterators.singletonIterator(
            new ConfidentHypothesis<Object, Integer>(h, 1.0));
   }

   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.trial.VS#train(java.lang.Object, java.lang.Object)
    */
   @Override
   public
   void train(final Object in, final Integer out) {
      _upperBound = Math.max(_lowerBound, Math.min(_upperBound, out));
   }
}