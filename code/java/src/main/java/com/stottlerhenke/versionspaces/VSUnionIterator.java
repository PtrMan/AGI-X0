/**
 * VSUnionIterator.java
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

import java.util.Iterator;

import com.google.common.base.Function;
import com.google.common.collect.ImmutableList;
import com.google.common.collect.Iterators;
import com.google.common.collect.UnmodifiableIterator;

/**
 * Takes two version spaces and generates an iterator that covers all 
 * hypotheses in both version spaces, scaling the confidences linearly so that
 * the sum of all confidences still equals 1.0 (modulo rounding error).
 * 
 * @author rcreswick
 * 
 * @see VSJoinIterator
 * @see VSTransformIterator
 * @see VS
 */
public class VSUnionIterator<In, Out> extends 
UnmodifiableIterator<ConfidentHypothesis<In, Out>> {
   
   /**
    * The backing iterator.
    */
   private Iterator<ConfidentHypothesis<In, Out>> _itr;

   
   /**
    * A function that scales the confidence of hypotheses by a given constant.
    */
   private class UnionFn implements
         Function<ConfidentHypothesis<In, Out>, ConfidentHypothesis<In, Out>> {
      /** The scaling factor */
      private final double _totalConfidence;

      /**
       * @param totalConfidence
       *           The total confidence of all incoming version spaces.
       */
      public UnionFn(final double totalConfidence) {
         _totalConfidence = totalConfidence;
      }

      @Override
      public ConfidentHypothesis<In, Out> apply(
            final ConfidentHypothesis<In, Out> in) {
         return new ConfidentHypothesis<In, Out>(
               in.getHypothesis(), 
               in.getConfidence() / _totalConfidence);
      }
   };

   /**
    * Constructor.
    * 
    * @param vs1 The first version spaces to union.
    * @param vs2 The second version space to union.
    */
   public VSUnionIterator(final VS<In, Out> vs1, final VS<In, Out> vs2) {
      this(ImmutableList.of(vs1, vs2));
   }
   
   /**
    * Constructor.
    * 
    * @param vss The version spaces to union.
    */
   public VSUnionIterator(final Iterable<VS<In, Out>> vss) {
      Iterator<ConfidentHypothesis<In, Out>> tmpItr = Iterators.emptyIterator();
      
      int totalConfidence = 0;
      
      for (VS<In, Out> vs : vss) {
         if (vs.iterator().hasNext()) {
            totalConfidence++;
         }
         tmpItr = Iterators.concat(tmpItr, vs.iterator());
      }
      
      // transform the compound iterator so that the confidences sum to 1.0
      _itr = Iterators.transform(tmpItr, new UnionFn(totalConfidence));
   }
   
   /* (non-Javadoc)
    * @see java.util.Iterator#hasNext()
    */
   @Override
   public boolean hasNext() {
      return _itr.hasNext();
   }

   /* (non-Javadoc)
    * @see java.util.Iterator#next()
    */
   @Override
   public ConfidentHypothesis<In, Out> next() {
      return _itr.next();
   }

}
