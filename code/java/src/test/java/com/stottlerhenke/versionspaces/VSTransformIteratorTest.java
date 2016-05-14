/**
 * VSTransformIteratorTest.java
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

import org.jmock.Expectations;
import org.jmock.Mockery;
import org.jmock.integration.junit4.JMock;
import org.jmock.integration.junit4.JUnit4Mockery;
import org.jmock.lib.legacy.ClassImposteriser;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;

import com.google.common.collect.Iterators;
import com.google.common.collect.Multiset;
import com.google.common.collect.TreeMultiset;


/**
 * @author rcreswick
 *
 */
@RunWith(JMock.class)
public class VSTransformIteratorTest {

   /** Constants to identfy the transform functions by their return values */
   private static final Integer IN_VAL = 0;
   private static final Integer OUT_EXEC = 1;
   private static final Integer OUT_TRAIN = 2;
   
   /** Simple transform */
   private final Transform<Integer, Integer, Integer, Integer> _tr = 
      new Transform<Integer, Integer, Integer, Integer>(){
         @Override
         public Integer in(final Integer parentIn) {
            return IN_VAL;
         }

         @Override
         public Integer out(final Integer parentIn, final Integer childOut) {
            return OUT_EXEC;
         }

         @Override
         public Integer train(final Integer parentIn, final Integer parentOut) {
            return OUT_TRAIN;
         }
   };
   
   private VS<Integer, Integer> _vs1 = null;
   
   private Mockery _context;
      
   @SuppressWarnings("unchecked") // mocking 
   @Before
   public void setUp() {
      _context = new JUnit4Mockery() {{
         setImposteriser(ClassImposteriser.INSTANCE);
      }};
      
      _vs1 = _context.mock(VS.class, "VS1");
   }

   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSTransformIterator_emptyVS() {
      fillSimpleHypotheses(new Integer[]{});
      
      VSTransformIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSTransformIterator<Integer, Integer, Integer, Integer>(_vs1, _tr);

      assertEquals("Wrong count of hypotheses.", 0, Iterators.size(iterator));
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSTransformIterator_simpleVS() {
      // fill the VS. with hypotheses that do *not* match the constants used 
      // in the transform:
      fillSimpleHypotheses(new Integer[]{4, 5, 6});
      
      VSTransformIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSTransformIterator<Integer, Integer, Integer, Integer>(_vs1, _tr);

      assertEquals("Wrong count of hypotheses.", 3, Iterators.size(iterator));
   }
   
   /**
    * Test method for {@link com.stottlerhenke.versionspaces.VSJoinIterator#VSUnionIterator(VS, VS)}.
    */
   @Test
   public void testVSTransformIterator_simpleVS_checkValues() {
      // fill the VS. with hypotheses that do *not* match the constants used 
      // in the transform:
      fillSimpleHypotheses(new Integer[]{4, 5, 6});
      
      VSTransformIterator<Integer, Integer, Integer, Integer> iterator = 
         new VSTransformIterator<Integer, Integer, Integer, Integer>(_vs1, _tr);
      
      Multiset<Integer> oracle = TreeMultiset.create();
      oracle.add(OUT_EXEC, 3);
      
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
    * 
    */
   private <T> void fillSimpleHypotheses(final T[] vals1) {
      _context.checking(new Expectations() {{
         allowing(_vs1).iterator();
         will(new ReturnNewHypothesesIteratorAction<T>(vals1));
         
         allowing(_vs1).hypotheses();
         will(new ReturnNewHypothesesIteratorAction<T>(vals1));
      }});
   }
}
