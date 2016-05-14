/**
 * RatioVSTest.java
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


/**
 * @author rcreswick
 *
 */
public class RatioVSTest {
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.RationVS#train(Double, Double)}.
    *
    * Verify that a simple ration can be learned from one example.
    */
   @Test
   public void testTrain_simple() {
      VSTest.testVS(new RatioVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Double, Double>(100.0, 80.0)),
            // execution tests:
            ImmutableSet.of(new Pair<Double, Map<Double, Double>>(
                  // input , [outputs->confidences]
                  1.0, ImmutableMap.of(0.8, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.RationVS#train(Double, Double)}.
    *
    * Verify that a simple ration can be learned from multiple matching examples.
    */
   @SuppressWarnings("unchecked") // generics w/ varargs
   @Test
   public void testTrain_simpleMatchingExamples_simple() {
      VSTest.testVS(new RatioVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Double, Double>(10.0, 8.0), 
               new Pair<Double, Double>(100.0, 80.0)),
            // execution tests:
            ImmutableSet.of(new Pair<Double, Map<Double, Double>>(
                  // input , [outputs->confidences]
                  1.0, ImmutableMap.of(0.8, 1.0)
            ))
      );
   }
   
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.RationVS#train(Double, Double)}.
    *
    * Verify that floating point values that are very similar (but not necessarily
    * exact) do not cause a version space collapse.
    */
   @SuppressWarnings("unchecked") // generics w/ varargs
   @Test
   public void testTrain_simpleMatchingExamples_fickle() {
      // TODO ERC testVS doesn't work well with floating point.
      VSTest.testVS(new RatioVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Double, Double>(10.0, 3.33333333), 
               new Pair<Double, Double>(100.0, 33.3333333)),
            // execution tests:
            ImmutableSet.of(new Pair<Double, Map<Double, Double>>(
                  // input , [outputs->confidences]
                  1.0, ImmutableMap.of(0.3333, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.RationVS#train(Double, Double)}.
    *
    * Verify that the version space collapses after a contradictory example.
    */
   @SuppressWarnings("unchecked") // generics w/ varargs
   @Test
   public void testTrain_collapse() {
      VSTest.testVS(new RatioVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Double, Double>(100.0, 40.0),
               new Pair<Double, Double>(100.0, 80.0)),
            // execution tests:
            ImmutableSet.of(new Pair<Double, Map<Double, Double>>(
                  // input , [outputs->confidences]
                  1.0, ImmutableMap.<Double,Double>of()
            ))
      );
   }
}
