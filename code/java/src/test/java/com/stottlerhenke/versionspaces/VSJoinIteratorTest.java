/**
 * VSJoinIteratorTest.java
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

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

import java.util.Set;

import org.jmock.Expectations;
import org.jmock.Mockery;
import org.jmock.integration.junit4.JMock;
import org.jmock.integration.junit4.JUnit4Mockery;
import org.jmock.lib.legacy.ClassImposteriser;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;

import com.google.common.collect.Iterators;
import com.google.common.collect.Sets;
import com.stottlerhenke.versionspaces.ConfidentHypothesis;
import com.stottlerhenke.versionspaces.Pair;
import com.stottlerhenke.versionspaces.VS;
import com.stottlerhenke.versionspaces.VSJoinIterator;

/**
 * @author rcreswick
 *
 */
@RunWith(JMock.class)
public class VSJoinIteratorTest {
   private VS<Integer, Integer> _vs1 = null;
   private VS<Integer, Integer> _vs2 = null;
   
   private Mockery _context;
      
   @SuppressWarnings("unchecked") // mocking 
   @Before
   public void setUp() {
      _context = new JUnit4Mockery() {{
         setImposteriser(ClassImposteriser.INSTANCE);
      }};
      
      _vs1 = _context.mock(VS.class, "VS1");
      _vs2 = _context.mock(VS.class, "VS2");
   }

   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSJoinIterator(VS, VS)}.
    */
   @Test
   public void testVSJoinIterator_emptyVS1() {
      fillSimpleHypotheses(new Integer[]{}, new Integer[]{1, 2, 3});
      
      VSJoinIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSJoinIterator<Integer, Integer, Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.",
            0, Iterators.size(iterator));
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSJoinIterator(VS, VS)}.
    */
   @Test
   public void testVSJoinIterator_emptyVS2() {
      fillSimpleHypotheses(new Integer[]{1, 2, 3}, new Integer[]{});
      
      VSJoinIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSJoinIterator<Integer, Integer, Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.",
            0, Iterators.size(iterator));
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSJoinIterator(VS, VS)}.
    */
   @Test
   public void testVSJoinIterator_emptyVS1_and_VS2() {
      fillSimpleHypotheses(new Integer[]{}, new Integer[]{});
      
      VSJoinIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSJoinIterator<Integer, Integer, Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.",
            0, Iterators.size(iterator));
   }
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSJoinIterator(VS, VS)}.
    */
   @Test
   public void testVSJoinIterator_itrSize() {
      fillSimpleHypotheses(new Integer[]{1, 2, 3}, new Integer[]{1, 2, 3});
      
      VSJoinIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSJoinIterator<Integer, Integer, Integer, Integer>(_vs1, _vs2);
      
      int hypCount = Iterators.size(_vs1.hypotheses()) * 
                        Iterators.size(_vs2.hypotheses());
      assertEquals("Wrong count of hypotheses.",
            hypCount, Iterators.size(iterator));
   }

   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#next()}.
    */
   @SuppressWarnings("unchecked") // generic types in var args.
   @Test
   public void testNext() {
      fillSimpleHypotheses(new Integer[]{1, 2, 3}, new Integer[]{1, 2, 3});
      
      VSJoinIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSJoinIterator<Integer, Integer, Integer, Integer>(_vs1, _vs2);
      
      Set<Pair<Integer, Integer>> oracle = Sets.<Pair<Integer,Integer>>newHashSet(
                        new Pair(1, 1), new Pair(1, 2), new Pair(1, 3),
                        new Pair(2, 1), new Pair(2, 2), new Pair(2, 3),
                        new Pair(3, 1), new Pair(3, 2), new Pair(3, 3));
      
      while (iterator.hasNext()) {
          ConfidentHypothesis<Pair<Integer,Integer>, Pair<Integer, Integer>> 
          ch = iterator.next();
          
          Pair<Integer, Integer> result = ch.getHypothesis().eval(new Pair(0,0));
          if (!oracle.remove(result) ){
             fail("oracle did not contain expected entry: "+ch);
          }
      }
      
      assertEquals("oracle should now be empty.", 0, oracle.size());
   }
   
   /**
    * 
    */
   private <T> void fillSimpleHypotheses(final T[] vals1, final T[] vals2) {
      _context.checking(new Expectations() {{
         allowing(_vs1).iterator();
         will(new ReturnNewHypothesesIteratorAction<T>(vals1));
         
         allowing(_vs1).hypotheses();
         will(new ReturnNewHypothesesIteratorAction<T>(vals1));
         
         // vs2 needs to be invoked multiple times, with a fresh iterator
         // each time:
         allowing(_vs2).iterator();
         will(new ReturnNewHypothesesIteratorAction<T>(vals2));
         
         allowing(_vs2).hypotheses();
         will(new ReturnNewHypothesesIteratorAction<T>(vals2));

      }});
   }
}
