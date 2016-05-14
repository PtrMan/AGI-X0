/**
 * IntVSTest.java
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
public class IntVSTest {
   
   /**
    * Test method for {@link IntVS#train(Object, Integer)}.
    *
    * Verify that a simple integer can be learned from one example.
    */
   @Test
   public void testTrain_simple() {
      VSTest.testVS(new IntVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Object, Integer>(100, 80)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  90, ImmutableMap.of(80, 1.0)
            ))
      );
   }

   /**
    * Test method for {@link IntVS#train(Object, Integer)}.
    * 
    * Verify that a simple integer can be learned from multiple matching
    * examples.
    */
   @SuppressWarnings("unchecked") // generics w/ varargs
   @Test
   public void testTrain_simpleMatchingExamples_simple() {
      VSTest.testVS(new IntVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Object, Integer>(50, 8), 
               new Pair<Object, Integer>(100, 8)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  1, ImmutableMap.of(8, 1.0)
            ))
      );
   }
   
   
   /**
    * Test method for {@link IntVS#train(Object, Integer)}.
    *
    * Verify that the version space collapses after a contradictory example.
    */
   @SuppressWarnings("unchecked") // generics w/ varargs
   @Test
   public void testTrain_collapse() {
      VSTest.testVS(new IntVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Object, Integer>(100, 40),
               new Pair<Object, Integer>(100, 80)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  1, ImmutableMap.<Integer,Double>of()
            ))
      );
   }
   
   /**
    * Test method for {@link IntVS#train(Object, Integer)}.
    *
    * Collapse the VS, then train again. (should not throw a NPE)
    */
   @SuppressWarnings("unchecked") // generics w/ varargs
   @Test
   public void testTrain_collapseNoNPE() {
      VSTest.testVS(new IntVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Object, Integer>(100, 40),
               new Pair<Object, Integer>(100, 80),
               new Pair<Object, Integer>(100, 70)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  1, ImmutableMap.<Integer,Double>of()
            ))
      );
   }
}
