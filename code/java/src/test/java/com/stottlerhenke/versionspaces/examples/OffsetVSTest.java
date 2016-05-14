/**
 * OffsetVSTest.java
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
 */
public class OffsetVSTest {
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.OffsetVS#train(Integer, Integer)}.
    * 
    * Test with a relative offset of 25%.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainIntegerInteger_rel() {
      VSTest.testVS(new OffsetVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Integer, Integer>(100, 25),
                  new Pair<Integer, Integer>(200, 50)
               ),
            // execution tests:
            ImmutableSet.of(new Pair<Integer, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  80, ImmutableMap.of(20, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.OffsetVS#train(Integer, Integer)}.
    * 
    * Test with a back offset of 75.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainIntegerInteger_back() {
      VSTest.testVS(new OffsetVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Integer, Integer>(100, 25),
                  new Pair<Integer, Integer>(200, 125)
               ),
            // execution tests:
            ImmutableSet.of(
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        80, ImmutableMap.of(5, 1.0)),
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        5, ImmutableMap.of(-70, 1.0))
            )
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.examples.OffsetVS#train(Integer, Integer)}.
    * 
    * Test with a front offset of 25.
    */
   @SuppressWarnings("unchecked") // generics in varargs
   @Test
   public void testTrainIntegerInteger_front() {
      VSTest.testVS(new OffsetVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Integer, Integer>(100, 25),
                  new Pair<Integer, Integer>(200, 25)
               ),
            // execution tests:
            ImmutableSet.of(
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        80, ImmutableMap.of(25, 1.0)),
                  new Pair<Integer, Map<Integer, Double>>(
                        // input , [outputs->confidences]
                        5, ImmutableMap.of(25, 1.0))
            )
      );
   }
}
