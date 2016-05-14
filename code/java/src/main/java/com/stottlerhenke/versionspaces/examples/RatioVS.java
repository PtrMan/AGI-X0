/**
 * RatioVS.java
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
 * A version space that learns to multiply an input value by a scaling factor
 * to generate a new result.
 * 
 * @author rcreswick
 */
public class RatioVS extends VS<Double, Double> {

   /** 
    * The required degree of similarity between example ratios before 
    * collapsing the VS.
    */
   public static final double EPSILON = 0.0001;

   /** The learned ratio */
   private Double _ratio = null;
   
   /**
    * Flag to differentiate "no examples" from a collapsed version space.
    */
   private boolean _untrained = true;
   
   
   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.VS#hypotheses()
    */
   @Override
   public UnmodifiableIterator<ConfidentHypothesis<Double, Double>> hypotheses() {

      if (null == _ratio) {
         return Iterators.emptyIterator();
      }
      
      final Double value = _ratio;
      return Iterators.singletonIterator(
            new ConfidentHypothesis<Double, Double>(new Hypothesis<Double, Double>(){
               @Override
               public Double eval(final Double in) {
                  return in * value;
               }
            }, 1.0));
   }

   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.VS#train(java.lang.Object, java.lang.Object)
    */
   @Override
   public void train(final Double in, final Double out) {
      if (_untrained) {
         _ratio = out / in;
         _untrained = false;
         return;
      }
      
      if (null != _ratio && Math.abs(_ratio - (out / in)) < EPSILON){
         return;
      }
      
      _ratio = null;
   }

}
