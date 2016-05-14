/**
 * SizeVSTest.java
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
public class SizeVSTest {
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.SizeVS#train(Integer, Integer)}.
    * 
    * Verify that a simple fixed size can be learned.
    */
   @SuppressWarnings("unchecked") // generics with varargs.
   @Test
   public void testTrain_fixedSize() {
      VSTest.testVS(new SizeVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Integer, Integer>(100, 50),
               new Pair<Integer, Integer>(80, 50)),
            // execution tests:
            ImmutableSet.of(
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        120, ImmutableMap.of(50, 1.0)),
                  // check zero:
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        0, ImmutableMap.of(50, 1.0))
            )
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.SizeVS#train(Integer, Integer)}.
    * 
    * Verify that a relative size can be learned.
    */
   @SuppressWarnings("unchecked") // generics with varargs.
   @Test
   public void testTrain_ratioSize() {
      VSTest.testVS(new SizeVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Integer, Integer>(100, 50),
               new Pair<Integer, Integer>(80, 40)),
            // execution tests:
            ImmutableSet.of(
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        120, ImmutableMap.of(60, 1.0)),
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        140, ImmutableMap.of(70, 1.0)),
                  // check negative values:
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        -140, ImmutableMap.of(-70, 1.0)),
                  // check zero:
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        0, ImmutableMap.of(0, 1.0))
            )
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.SizeVS#train(Integer, Integer)}.
    * 
    * Verify that a fixed and relative sizes can be learned.
    */
   @SuppressWarnings("unchecked") // generics with varargs.
   @Test
   public void testTrain_multiHypotheses() {
      VSTest.testVS(new SizeVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Integer, Integer>(100, 50)),
            // execution tests:
            ImmutableSet.of(
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        120, ImmutableMap.of(60, 0.5, 
                                             50, 0.5)),
                  // test compounding hypotheses (equal results should be combined)
                  new Pair<Integer, Map<Integer, Double>>(
                        100, ImmutableMap.of(50, 1.0)),
                  // check zero:
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        0, ImmutableMap.of(0, 0.5, 
                                          50, 0.5))
               )
         );
   }
}