/**
 * LocationVSTest.java
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

import java.util.Map;

import org.junit.Test;

import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableSet;
import com.stottlerhenke.versionspaces.Pair;
import com.stottlerhenke.versionspaces.VSTest;
import com.stottlerhenke.versionspaces.examples.data.Anchor;
import com.stottlerhenke.versionspaces.examples.data.LocationSpec;
import com.stottlerhenke.versionspaces.examples.data.Region1D;

/**
 * @author rcreswick
 *
 */
public class LocationVSTest {
   
   private static final Region1D R_0_TO_80  = new Region1D(0, 80);
   private static final Region1D R_0_TO_100 = new Region1D(0,100);
   private static final Region1D R_0_TO_120 = new Region1D(0, 120);
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.LocationVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.LocationSpec)}.
    * 
    * Test with a 50-wide region, centered on the middle of the line.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainRegion1DLocationSpec_fixedWidthCentered() {
      VSTest.testVS(new LocationVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(25, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(50, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(75, Anchor.Trailing)),
                  
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(35, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(60, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(85, Anchor.Trailing))
               ),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<LocationSpec, Double>>(
                  // input , [outputs->confidences]
                  R_0_TO_80, ImmutableMap.of(lSpec(40, Anchor.Centered), 1.0)
            ))
      );
   }



   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.LocationVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.LocationSpec)}.
    * 
    * Test with a region that is 50% of the region width, centered on the middle of the line.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainRegion1DLocationSpec_relativeWidthCentered() {
      VSTest.testVS(new LocationVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(25, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(50, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(75, Anchor.Trailing)),
                  
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(30, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(60, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(90, Anchor.Trailing))
               ),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<LocationSpec, Double>>(
                  // input , [outputs->confidences]
                  R_0_TO_80, ImmutableMap.of(
                        lSpec(20, Anchor.Leading),  0.3333,
                        lSpec(40, Anchor.Centered), 0.3333,
                        lSpec(60, Anchor.Trailing), 0.3333)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.LocationVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.LocationSpec)}.
    * 
    * Test with a region that is 50 wide, 25 from the end of the line..
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainRegion1DLocationSpec_backOffset_fixedSize() {
      VSTest.testVS(new LocationVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(25, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(50, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(75, Anchor.Trailing)),
                  
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(45, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(70, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(95, Anchor.Trailing))
               ),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<LocationSpec, Double>>(
                  // input , [outputs->confidences]
                  R_0_TO_80, ImmutableMap.of(
                        lSpec(5, Anchor.Leading),  0.3333,
                        lSpec(30, Anchor.Centered), 0.3333,
                        lSpec(55, Anchor.Trailing), 0.3333)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.LocationVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.LocationSpec)}.
    * 
    * Test with a region that is 50% wide, 25 from the end of the line..
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainRegion1DLocationSpec_backOffset_relSize() {
      VSTest.testVS(new LocationVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(25, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(50, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(75, Anchor.Trailing)),
                  
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(35, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(65, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(95, Anchor.Trailing))
               ),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<LocationSpec, Double>>(
                  // input , [outputs->confidences]
                  R_0_TO_80, ImmutableMap.of(
                        lSpec(55, Anchor.Trailing), 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.LocationVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.LocationSpec)}.
    * 
    * Test with a region that is 50% wide, 25% from the end of the line..
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainRegion1DLocationSpec_relBackOffset_fixedSize() {
      VSTest.testVS(new LocationVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(25, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(50, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_100, lSpec(75, Anchor.Trailing)),
                  
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(40, Anchor.Leading)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(65, Anchor.Centered)),
                  new Pair<Region1D, LocationSpec>(R_0_TO_120, lSpec(90, Anchor.Trailing))
               ),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<LocationSpec, Double>>(
                  // input , [outputs->confidences]
                  R_0_TO_80, ImmutableMap.of(
                        lSpec(60, Anchor.Trailing), 1.0)
            ))
      );
   }
   
   /**
    * Helper method to create location specs more succinctly.
    * 
    * @param i The value
    * @param anchor The anchor.
    * @return A new LocationSpec
    */
   private LocationSpec lSpec(final int i, final Anchor anchor) {
      return new LocationSpec(i, anchor);
   }
}
