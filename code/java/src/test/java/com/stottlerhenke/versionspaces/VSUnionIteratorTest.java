/**
 * VSUnionIteratorTest.java
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

import static com.stottlerhenke.versionspaces.TestConstants.EPSILON;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

import org.jmock.Expectations;
import org.jmock.Mockery;
import org.jmock.integration.junit4.JMock;
import org.jmock.integration.junit4.JUnit4Mockery;
import org.jmock.lib.legacy.ClassImposteriser;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;

import com.google.common.collect.ImmutableList;
import com.google.common.collect.Iterators;
import com.google.common.collect.Multiset;
import com.google.common.collect.TreeMultiset;

/**
 * @author rcreswick
 *
 */
@RunWith(JMock.class)
public class VSUnionIteratorTest {

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
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSUnionIterator_scaleConfidence() {
      fillSimpleHypotheses(new Integer[]{1}, new Integer[]{2});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.", 2, Iterators.size(iterator));
      
      iterator = new VSUnionIterator<Integer, Integer>(_vs1, _vs2);
      
      while (iterator.hasNext()) {
         ConfidentHypothesis<Integer, Integer> h = iterator.next();
         assertEquals("Wrong confidence", 0.5, h.getConfidence().doubleValue(), EPSILON);
      }
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSUnionIterator_scaleConfidenceCollapse() {
      fillSimpleHypotheses(new Integer[]{ 2 }, new Integer[]{});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.", 1, Iterators.size(iterator));
      
      iterator = new VSUnionIterator<Integer, Integer>(_vs1, _vs2);
      
      while (iterator.hasNext()) {
         ConfidentHypothesis<Integer, Integer> h = iterator.next();
         assertEquals("Wrong confidence", 1.0, h.getConfidence().doubleValue(), EPSILON);
      }
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSUnionIterator_emptyVS1() {
      fillSimpleHypotheses(new Integer[]{}, new Integer[]{1, 2, 3});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.", 3, Iterators.size(iterator));
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSUnionIterator#VSJoinIterator(VS, VS)}.
    */
   @Test
   public void testVSUnionIterator_emptyVS2() {
      fillSimpleHypotheses(new Integer[]{1, 2, 3}, new Integer[]{});
      
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.", 3, Iterators.size(iterator));
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSUnionIterator#VSJoinIterator(VS, VS)}.
    */
   @Test
   public void testVSUnionIterator_emptyVS1_and_VS2() {
      fillSimpleHypotheses(new Integer[]{}, new Integer[]{});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);

      assertEquals("Wrong count of hypotheses.", 0, Iterators.size(iterator));
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSUnionIterator#VSUnionIterator(com.stottlerhenke.versionspaces.VS, com.stottlerhenke.versionspaces.VS)}.
    */
   @Test
   public void testVSUnionIterator() {
      fillSimpleHypotheses(new Integer[]{1, 2, 3}, new Integer[]{1, 2, 3});

      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);

      int hypCount = Iterators.size(_vs1.hypotheses()) +
                        Iterators.size(_vs2.hypotheses());

      assertEquals("Wrong count of hypotheses.",
            hypCount, Iterators.size(iterator));
   }

   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSUnionIterator#next()}.
    */
   @Test
   public void testNext_sameHypotheses() {
      fillSimpleHypotheses(new Integer[]{1, 2, 3}, new Integer[]{1, 2, 3});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);
      
      Multiset<Integer> oracle = TreeMultiset.create();
      oracle.add(1);
      oracle.add(2);
      oracle.add(3);
      oracle.add(1);
      oracle.add(2);
      oracle.add(3);
      
      while (iterator.hasNext()) {
          ConfidentHypothesis<Integer, Integer> ch = iterator.next();
          
          Integer result = ch.getHypothesis().eval(0);
          if (!oracle.remove(result) ){
             fail("oracle did not contain expected entry: "+ch);
          }
      }
      
      assertEquals("oracle should now be empty.", 0, oracle.size());
   }


   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSUnionIterator#next()}.
    */
   @Test
   public void testNext_differentHypotheses() {
      fillSimpleHypotheses(new Integer[]{1, 2, 3}, new Integer[]{4, 5, 6});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(_vs1, _vs2);
      
      Multiset<Integer> oracle = TreeMultiset.create();
      oracle.add(1);
      oracle.add(2);
      oracle.add(3);
      oracle.add(4);
      oracle.add(5);
      oracle.add(6);
      
      while (iterator.hasNext()) {
          ConfidentHypothesis<Integer, Integer> ch = iterator.next();
          
          Integer result = ch.getHypothesis().eval(0);
          if (!oracle.remove(result) ){
             fail("oracle did not contain expected entry: "+ch);
          }
      }
      
      assertEquals("oracle should now be empty.", 0, oracle.size());
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSUnionIterator_iterable_empties() {
      fillSimpleHypotheses(new Integer[]{}, new Integer[]{1, 2, 3});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(ImmutableList.of(_vs1, _vs2, _vs1));

      assertEquals("Wrong count of hypotheses.", 3, Iterators.size(iterator));
   }

   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSUnionIterator_iterable() {
      fillSimpleHypotheses(new Integer[]{}, new Integer[]{1, 2, 3});
      
      VSUnionIterator<Integer, Integer> iterator = 
         new VSUnionIterator<Integer, Integer>(ImmutableList.of(_vs1, _vs2, _vs2));

      assertEquals("Wrong count of hypotheses.", 6, Iterators.size(iterator));
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
