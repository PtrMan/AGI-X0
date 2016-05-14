/**
 * FixedValueVS.java
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
 * Version Space that learns a constant function.
 * 
 * @author anovstrup
 * 
 * @param <O> The type of the value to learn
 */
public class FixedValueVS<O>
      extends VS<Object, O>
{
   /** flag to differentiate between "no examples" and a collapsed VS. */
   private boolean _untrained = true;
   
   /** The value being learned. */
   private O _value = null;
   
   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.VS#hypotheses()
    */
   @Override
   public UnmodifiableIterator<ConfidentHypothesis<Object, O>> hypotheses() {
      if (null == _value) {
         return Iterators.emptyIterator();
      }
      
      final O value = _value;
      return Iterators.singletonIterator(
            new ConfidentHypothesis<Object, O>(new Hypothesis<Object, O>(){
               @Override
               public O eval(final Object in) {
                  return value;
               }
            }, 1.0));
      
   }
   
   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.VS#train(java.lang.Object, java.lang.Object)
    */
   @Override
   public void train(Object in, O out)
   {
      if (_untrained) {
         _value = out;
         _untrained = false;
         return;
      }

      if (null != _value && !_value.equals(out)) {
         _value = null;
      }
   }
}
