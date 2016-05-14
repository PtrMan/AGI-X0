/**
 * ReturnNewHypothesesIteratorAction.java
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

import java.util.Collection;

import org.hamcrest.Description;
import org.jmock.api.Action;
import org.jmock.api.Invocation;

import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableList.Builder;
import com.stottlerhenke.versionspaces.ConfidentHypothesis;
import com.stottlerhenke.versionspaces.Hypothesis;

/**
 * Returns a new value every invocation
 */
public class ReturnNewHypothesesIteratorAction<T> implements Action {
   private final T[] _values;

   public ReturnNewHypothesesIteratorAction(final T... values) {
      _values = values;
   }

   @Override
   public Object invoke(final Invocation invocation) throws Throwable {
      return hypothesesCollection(_values).iterator();
   }

   @Override
   public void describeTo(final Description description) {
      description.appendText("returns a new iterator on each invocation");
   }
   
   protected <I, O> Collection<ConfidentHypothesis<I, O>> 
   hypothesesCollection(final O... values) {
      Builder<ConfidentHypothesis<I,O>> builder = ImmutableList.builder();
      
      for(final O v : values) {
         builder.add(new ConfidentHypothesis<I, O>(new Hypothesis<I, O>() {
            @Override
            public O eval(final I in) {
               return v;
            }
         }, 1.0 / values.length));
      }
      return builder.build();
   }
}