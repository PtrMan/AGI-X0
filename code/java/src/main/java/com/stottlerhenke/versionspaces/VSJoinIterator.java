/**
 * VSJoinIterator.java
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

import com.google.common.collect.UnmodifiableIterator;

/**
 * @author rcreswick
 *
 */
public class VSJoinIterator<I1, I2, O1, O2>
extends UnmodifiableIterator<ConfidentHypothesis<Pair<I1, I2>, Pair<O1, O2>>> {

   private final Iterator<ConfidentHypothesis<I1, O1>> _itrOne;
   private Iterator<ConfidentHypothesis<I2, O2>> _itrTwo;

   private ConfidentHypothesis<I1, O1> _curValOne;
   
   /** reference to the second version space, used to reset the iterator. */
   private final VS<I2, O2> _vs2;
   
   /**
    * @param vs1
    * @param vs2
    */
   public VSJoinIterator(final VS<I1, O1> vs1, final VS<I2, O2> vs2) {
      this._vs2 = vs2;
      
      this._itrOne = vs1.iterator();
      this._itrTwo = vs2.iterator();
      
      // start of by initiating _curValOne, if possible:
      if (_itrOne.hasNext()) {
         _curValOne = _itrOne.next();
      }
   }

   /* (non-Javadoc)
    * @see java.util.Iterator#hasNext()
    */
   @Override
   public boolean hasNext() {
      // the iterators' states are checked on each next() call, and update 
      // _curValOne to reflect the end of iterators.
      return (null != _curValOne && _itrTwo.hasNext());
   }

   /* (non-Javadoc)
    * @see java.util.Iterator#next()
    */
   @Override
   public ConfidentHypothesis<Pair<I1, I2>, Pair<O1, O2>> next() {
      ConfidentHypothesis<I2, O2> curValTwo = _itrTwo.next();
      final Hypothesis<I1, O1> h1 = _curValOne.getHypothesis();
      final Hypothesis<I2, O2> h2 = curValTwo.getHypothesis();
      
      Hypothesis<Pair<I1, I2>, Pair<O1, O2>> h =
            new Hypothesis<Pair<I1, I2>, Pair<O1, O2>>() {
               public Pair<O1, O2> eval(final Pair<I1, I2> in) {
                  return new Pair<O1, O2>(h1.eval(in.a), h2.eval(in.b));
               }
      };

      // TODO Consider whether these hypotheses need to be ordered by decreasing
      // confidence.  If so, we'll need to implement something like the 
      // Cartesian product sorting merge found here:
      //  http://www.haskell.org/pipermail/haskell-cafe/2009-April/060034.html
      // or here:
      //  http://porg.es/blog/sorted-sums-of-a-sorted-list
      ConfidentHypothesis<Pair<I1, I2>, Pair<O1, O2>> pair = 
         new ConfidentHypothesis<Pair<I1, I2>, Pair<O1, O2>>(h,
               _curValOne.getConfidence() * curValTwo.getConfidence());
      
      
      if (_itrTwo.hasNext()) {
         // keep going. (We don't need to step Itr1.)
         return pair;
      }
      
      if (_itrOne.hasNext()) {
         // itrTwo is exhausted, but itrOne is not, so reset two, and step one:
         _itrTwo = _vs2.iterator();
         _curValOne = _itrOne.next();
      } else {
         // we are done (both iterators are exhausted)
         _curValOne = null;
      }
      
      return pair;
   }
}