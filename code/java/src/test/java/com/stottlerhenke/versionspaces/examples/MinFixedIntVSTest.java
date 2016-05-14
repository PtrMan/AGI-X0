/**
 * MinFixedIntVSTest.java
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

import junit.framework.Assert;

import org.junit.Test;

import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableSet;
import com.google.common.collect.Iterators;
import com.google.common.collect.UnmodifiableIterator;
import com.stottlerhenke.versionspaces.ConfidentHypothesis;
import com.stottlerhenke.versionspaces.Hypothesis;
import com.stottlerhenke.versionspaces.Pair;
import com.stottlerhenke.versionspaces.VSTest;

/**
 * @author rcreswick
 *
 */
public class MinFixedIntVSTest {
   
   /**
    * Verifies the initial state of the hypothesis space. (starts with
    * Integer.Max_Value)
    */
   @Test
   public void testHypotheses_initial() {
      MinFixedIntVS vs = new MinFixedIntVS();
      
      Assert.assertEquals("Should have one hypothesis", 
            1, Iterators.size(vs.hypotheses()));
      
      UnmodifiableIterator<ConfidentHypothesis<Object, Integer>> hypItr = 
         vs.hypotheses();
      Hypothesis<Object, Integer> h = hypItr.next().getHypothesis();
      
      Assert.assertEquals("Wrong hypothesis.", Integer.MAX_VALUE, h.eval(0).intValue());
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.OffsetVS#train(com.stottlerhenke.versionspaces.Region1D, java.lang.Integer)}.
    * 
    * Verify that a simple one example / one hypothesis test works. 
    */
   @Test
   public void testTrain_simpleExample() {
      VSTest.testVS(new MinFixedIntVS(), 
            // training examples:
            ImmutableSet.of(new Pair<Object, Integer>(100, 50)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  10, ImmutableMap.of(50, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.OffsetVS#train(com.stottlerhenke.versionspaces.Region1D, java.lang.Integer)}.
    * 
    * Verify that the execution input does not need to match the training input 
    * to learn a fixed offset. 
    */
   @Test
   public void testTrain_simpleNewExeInput() {
      VSTest.testVS(new MinFixedIntVS(), 
            // training examples:
            ImmutableSet.of(new Pair<Object, Integer>(100, 50)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  60, ImmutableMap.of(50, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.OffsetVS#train(com.stottlerhenke.versionspaces.Region1D, java.lang.Integer)}.
    * 
    * Verify that a value can be learned that may exceed the execution input. 
    */
   @Test
   public void testTrain_simpleOverlargeExample() {
      VSTest.testVS(new MinFixedIntVS(), 
            // training examples:
            ImmutableSet.of(new Pair<Object, Integer>(100, 50)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  40, ImmutableMap.of(50, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.OffsetVS#train(com.stottlerhenke.versionspaces.Region1D, java.lang.Integer)}.
    * 
    * Verify that subsequent training examples move the learned value down. 
    */
   @Test
   public void testTrain_simpleReducingExample() {
      final MinFixedIntVS vs = new MinFixedIntVS();
      VSTest.testVS(vs, 
            // training examples:
            ImmutableSet.of(new Pair<Object, Integer>(100, 50)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  100, ImmutableMap.of(50, 1.0)
            ))
      );
      
      VSTest.testVS(vs, 
            // training examples:
            ImmutableSet.of(new Pair<Object, Integer>(100, 40)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  100, ImmutableMap.of(40, 1.0)
            ))
      );
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.OffsetVS#train(com.stottlerhenke.versionspaces.Region1D, java.lang.Integer)}.
    * 
    * Verify that subsequent training examples move the learned value down, but 
    * never up.
    */
   @SuppressWarnings("unchecked") // generics with varargs.
   @Test
   public void testTrain_multiStepReducingExample() {
      VSTest.testVS(new MinFixedIntVS(), 
            // training examples:
            ImmutableSet.of(
               new Pair<Object, Integer>(100, 50),
               new Pair<Object, Integer>(100, 40),
               new Pair<Object, Integer>(100, 80)),
            // execution tests:
            ImmutableSet.of(new Pair<Object, Map<Integer, Double>>(
                  // input , [outputs->confidences]
                  100, ImmutableMap.of(40, 1.0)
            ))
      );
   }
}
