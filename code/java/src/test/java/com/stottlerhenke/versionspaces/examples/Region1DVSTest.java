/**
 * Region1DVSTest.java
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
import com.stottlerhenke.versionspaces.examples.data.Region1D;

/**
 * @author rcreswick
 *
 */
public class Region1DVSTest {

   private static final Region1D R_20_to_40 = new Region1D(20, 40);
   private static final Region1D R_30_to_50 = new Region1D(30, 50);
   private static final Region1D R_40_to_60 = new Region1D(40, 60);
   private static final Region1D R_44_to_56 = new Region1D(44, 56);
   private static final Region1D R_45_to_55 = new Region1D(45, 55);
   private static final Region1D R_45_to_65 = new Region1D(45, 65);
   private static final Region1D R_46_to_54 = new Region1D(46, 54);
   private static final Region1D R_50_to_70 = new Region1D(50, 70);
   
   private static final Region1D R_0_TO_80  = new Region1D(0, 80);
   private static final Region1D R_0_TO_100 = new Region1D(0,100);
   private static final Region1D R_0_TO_110 = new Region1D(0, 110);
   private static final Region1D R_0_TO_120 = new Region1D(0, 120);
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.Region1DVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.Region1D)}.
    * 
    * Verify that the VS can learn a simple front offset of 40.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrain_simpleFrontOffset() {
      VSTest.testVS(new Region1DVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, Region1D>(R_0_TO_100, R_40_to_60),
                  new Pair<Region1D, Region1D>(R_0_TO_80, R_40_to_60)),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<Region1D, Double>>(
                  R_0_TO_120, ImmutableMap.of(R_40_to_60, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.Region1DVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.Region1D)}.
    * 
    * Verify that the VS can learn a simple back-offset of 40.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrain_simpleBackOffset() {
      VSTest.testVS(new Region1DVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, Region1D>(R_0_TO_100, R_40_to_60), 
                  new Pair<Region1D, Region1D>(R_0_TO_80, R_20_to_40)),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<Region1D, Double>>(
                  R_0_TO_110, ImmutableMap.of(R_50_to_70, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.Region1DVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.Region1D)}.
    * 
    * Verify that the VS can learn a simple relative center offset of 0.5,
    * with a width of 20.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrain_simpleRelativeOffset() {
      VSTest.testVS(new Region1DVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, Region1D>(R_0_TO_100, R_40_to_60), 
                  new Pair<Region1D, Region1D>(R_0_TO_80, R_30_to_50)),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<Region1D, Double>>(
                  R_0_TO_110, ImmutableMap.of(R_45_to_65, 1.0)
            ))
      );
   }
   
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.Region1DVS#train(com.stottlerhenke.versionspaces.examples.data.Region1D, com.stottlerhenke.versionspaces.examples.data.Region1D)}.
    * 
    * Verify that the VS can learn a simple fixed center offset of 50,
    * with a relative width of 0.1.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrain_simpleFixedCenterOffset() {
      VSTest.testVS(new Region1DVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Region1D, Region1D>(R_0_TO_100, R_45_to_55), 
                  new Pair<Region1D, Region1D>(R_0_TO_80,  R_46_to_54)),
            // execution tests:
            ImmutableSet.of(new Pair<Region1D, Map<Region1D, Double>>(
                  R_0_TO_120, ImmutableMap.of(R_44_to_56, 1.0)
            ))
      );
   }
}