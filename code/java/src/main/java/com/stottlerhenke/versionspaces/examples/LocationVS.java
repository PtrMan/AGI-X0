/**
 * LocationVS.java
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

import com.google.common.base.Predicate;
import com.google.common.collect.ImmutableList;
import com.stottlerhenke.versionspaces.CompositeVS;
import com.stottlerhenke.versionspaces.Transform;
import com.stottlerhenke.versionspaces.VS;
import com.stottlerhenke.versionspaces.examples.data.Anchor;
import com.stottlerhenke.versionspaces.examples.data.LocationSpec;
import com.stottlerhenke.versionspaces.examples.data.Region1D;

/**
 * A Version Space that represents all locations of "things" in a region that
 * can be positioned relative to their leading edge, trailing edge, or center.
 * 
 * @author rcreswick
 */
public class LocationVS extends CompositeVS<Region1D, LocationSpec> {

   /**
    * Transform that pulls out the size of a region, and the offset of a
    * LocationSpec.
    */
   private static class LSpecTransform implements
   Transform<Region1D, Integer, LocationSpec, Integer> {
      private final Anchor _anchor;

      /**
       * @param anchor
       */
      public LSpecTransform(final Anchor anchor) {
         super();
         _anchor = anchor;
      }

      @Override
      public Integer in(final Region1D parentIn) {
         return parentIn.getSize();
      }

      @Override
      public LocationSpec out(final Region1D parentIn, final Integer childOut) {
         return new LocationSpec(childOut, _anchor);
      }

      @Override
      public Integer train(final Region1D parentIn, final LocationSpec parentOut) {
         return parentOut.getOffset();
      }
   
   };

   /** A Predicate that filters LocationSpecs based on the anchor. */
   private static class LSpecFilter implements Predicate<LocationSpec> {

      /** The anchor to filter on */
      private final Anchor _anchor;

      /**
       * Constructor.
       * @param anchor The anchor to allow.
       */
      public LSpecFilter(final Anchor anchor) {
         super();
         _anchor = anchor;
      }

      /* (non-Javadoc)
       * @see com.google.common.base.Predicate#apply(java.lang.Object)
       */
      @Override
      public boolean apply(final LocationSpec lspec) {
         return (lspec.getAnchor().equals(_anchor));
      }
      
   }

   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.CompositeVS#buildBackingVS()
    */
   @Override
   protected VS<Region1D, LocationSpec> buildBackingVS() {  
      VS<Region1D, LocationSpec> leadingVS = 
         VS.filter(
               VS.transform(new OffsetVS(), 
                     new LSpecTransform(Anchor.Leading)),
               new LSpecFilter(Anchor.Leading));

      VS<Region1D, LocationSpec> trailingVS = 
         VS.filter(
               VS.transform(new OffsetVS(), 
                     new LSpecTransform(Anchor.Trailing)),
               new LSpecFilter(Anchor.Trailing));
      
      VS<Region1D, LocationSpec> centeredVS = 
         VS.filter(
               VS.transform(new OffsetVS(), 
                     new LSpecTransform(Anchor.Centered)),
               new LSpecFilter(Anchor.Centered));
      
      return VS.union(ImmutableList.of(leadingVS, trailingVS, centeredVS));
   }

}
