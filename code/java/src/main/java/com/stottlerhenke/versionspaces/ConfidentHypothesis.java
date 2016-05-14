/**
 * ConfidentHypothesis.java
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

package com.stottlerhenke.versionspaces;


import com.google.common.collect.Iterators;
import com.google.common.collect.UnmodifiableIterator;

/**
 * @author rcreswick
 *
 */
public class ConfidentHypothesis<Input, Output> {
   private final Hypothesis<Input, Output> _hypothesis;
   
   private final Double _confidence;

   static public <Input, Output> UnmodifiableIterator<ConfidentHypothesis<Input, Output>> 
   confidentHypothesisIter(final Hypothesis<Input, Output> hypothesis) {
      return Iterators.singletonIterator(confidentHypothesis(hypothesis, 1.0));
   }
   
   static public <Input, Output> ConfidentHypothesis<Input, Output> 
   confidentHypothesis(
         final Hypothesis<Input, Output> hypothesis,
         final Double confidence) {
      return new ConfidentHypothesis<Input, Output>(hypothesis, confidence);
   }
   
   /**
    * @param hypothesis
    * @param confidence
    */
   public ConfidentHypothesis(final Hypothesis<Input, Output> hypothesis,
         final Double confidence) {
      super();
      _hypothesis = hypothesis;
      _confidence = confidence;
   }

   /**
    * @return the hypothesis
    */
   public Hypothesis<Input, Output> getHypothesis() {
      return _hypothesis;
   }

   /**
    * @return the confidence
    */
   public Double getConfidence() {
      return _confidence;
   }
}
