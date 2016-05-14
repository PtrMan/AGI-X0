/**
 * VSTransformIterator.java
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
import com.google.common.collect.Iterators;
import com.google.common.collect.UnmodifiableIterator;

/**
 * @author rcreswick
 *
 */
public class VSTransformIterator<Ip, Op, Ic, Oc> extends 
UnmodifiableIterator<ConfidentHypothesis<Ip, Op>> {

   private final Iterator<ConfidentHypothesis<Ip, Op>> _itr;

   public VSTransformIterator(final VS<Ic, Oc> vs,
         final GeneralTransform<Ip, Ic, Op, Oc> tr) {
      _itr = Iterators.transform(
             vs.iterator(),
             new Function<ConfidentHypothesis<Ic, Oc>, ConfidentHypothesis<Ip, Op>>() {
                
                @Override
                public ConfidentHypothesis<Ip, Op> 
                apply(final ConfidentHypothesis<Ic, Oc> in) {
                   
                   final Hypothesis<Ic, Oc> h = in.getHypothesis();
                   Double c = in.getConfidence();
                   Hypothesis<Ip, Op> transformedH =
                      new Hypothesis<Ip, Op>() {
                         @Override
                         public Op eval(final Ip in) {
                            return tr.out(in, h.eval(tr.in(in)));
                         }
                   };
                   return new ConfidentHypothesis<Ip, Op>(transformedH, c);
                }
             });
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
   public ConfidentHypothesis<Ip, Op> next() {
      return _itr.next();
   }
}
