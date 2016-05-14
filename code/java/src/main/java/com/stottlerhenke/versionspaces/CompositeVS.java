/**
 * CompositeVS.java
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

import com.google.common.collect.UnmodifiableIterator;

/**
 * A version space that consists of a join or union of other version spaces.
 * 
 * @author rcreswick
 */
public abstract class CompositeVS<In, Out> extends VS<In, Out> {

   /** Cache for the backing version space. */
   private VS<In, Out> _backingVS;

   /**
    * Creates the backing version space that this composite VS wraps.
    * 
    * This method is only invoked once, the first time the version space is
    * needed.
    * 
    * @return The backing version space.
    */
   protected abstract VS<In, Out> buildBackingVS();
   
   /**
    * Private caching accessor for the backing version space.
    * 
    * @return The backing version space, invoking the creation method if necessary.
    */
   private VS<In, Out> getBackingVS() {
      if (_backingVS == null) {
         _backingVS = buildBackingVS();
      }

      return _backingVS;
   }
   
   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.VS#hypotheses()
    */
   @Override
   public UnmodifiableIterator<ConfidentHypothesis<In, Out>> hypotheses() {
      return getBackingVS().hypotheses();
   }

   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.VS#train(java.lang.Object, java.lang.Object)
    */
   @Override
   public void train(final In in, final Out out) {
      getBackingVS().train(in, out);
   }

}
