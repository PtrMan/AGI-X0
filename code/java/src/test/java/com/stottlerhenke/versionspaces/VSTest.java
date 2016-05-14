/**
 * VSTest.java
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

import java.util.Arrays;
import java.util.Comparator;
import java.util.Map;
import java.util.Set;
import java.util.Map.Entry;

import org.jmock.Expectations;
import org.jmock.Mockery;
import org.jmock.integration.junit4.JUnit4Mockery;
import org.jmock.lib.legacy.ClassImposteriser;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;

import com.google.common.base.Predicates;
import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.Sets;
import com.google.common.collect.ImmutableList.Builder;


/**
 * @author rcreswick
 *
 */
public class VSTest {
   
   /**
    * Compartor used to compare doubles for testing equality.
    * 
    * (uses EPSILON for a delta)
    */
   private static final Comparator<Double> SAFE_DOUBLE_COMPARATOR =
      new Comparator<Double>(){
         @Override
         public int compare(final Double o1, final Double o2) {
            if (Math.abs(o1 - o2) < EPSILON) {
               return 0;
            } else {
               return o1.compareTo(o2);
            }
         }
   };
      
   /** Mocked version spaces */
   private VS<Integer, String> _vs1 = null;
   private VS<Integer, String> _vs2 = null;
   
   private Mockery _context;
      
   /**
    * Set up the mocks before every test.
    */
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
    * Ensure that evaluating a joined version space generates the proper set of 
    * pair objects.
    */
   @SuppressWarnings("unchecked") // generics with varargs.
   @Test
   public void testJoin_exec() {
      
      // mock up some version spaces, and provide the fixed hypothesis results
      // to be used in the assertions below:
      String[] vs1Output = {"a", "b", "c"};
      String[] vs2Output = {"d", "e", "f"};
      fillSimpleHypotheses(vs1Output, vs2Output);
      
      VS<Pair<Integer, Integer>, Pair<String, String>> vs3 = VS.join(_vs1, _vs2);
      
      Set<Pair<String, String>> oracle = Sets.<Pair<String,String>>newHashSet(
            new Pair("a", "d"), new Pair("a", "e"), new Pair("a", "f"),
            new Pair("b", "d"), new Pair("b", "e"), new Pair("b", "f"),
            new Pair("c", "d"), new Pair("c", "e"), new Pair("c", "f"));

      for(ConfidentHypothesis<Pair<Integer,Integer>, Pair<String, String>> 
         ch : vs3) {

         Pair<String, String> result = ch.getHypothesis().eval(new Pair(0,0));
         if (!oracle.remove(result) ){
            fail("oracle did not contain expected entry: "+ch);
         }
      }

      assertEquals("oracle should now be empty.", 0, oracle.size());
   }
   
   /**
    * Ensure that the joined vs. splits its training info properly between
    * the two version spaces.
    */
   @Test
   public void testJoin_train() {
      String[] vs1Output = {"a", "b", "c"};
      String[] vs2Output = {"d", "e", "f"};
      
      fillSimpleHypotheses(vs1Output, vs2Output);
      
      _context.checking(new Expectations() {{
         oneOf(_vs1).train(0, "x");
         oneOf(_vs2).train(1, "y");
      }});
      
      VS<Pair<Integer, Integer>, Pair<String, String>> vs3 = VS.join(_vs1, _vs2);

      // run the training step.  
      // Mock expectations will cause failure if needed.
      vs3.train(new Pair<Integer, Integer>(0, 1), 
                new Pair<String, String>("x", "y"));
   }
   
   @Test
   public void testUnion_train() {
      String[] vs1Output = {"a", "b", "c"};
      String[] vs2Output = {"d", "e", "f"};
      
      fillSimpleHypotheses(vs1Output, vs2Output);
      
      _context.checking(new Expectations() {{
         oneOf(_vs1).train(0, "x");
         oneOf(_vs2).train(0, "x");
      }});
      
      VS<Integer, String> vs3 = VS.union(_vs1, _vs2);

      // run the training step.  
      // Mock expectations will cause failure if needed.
      vs3.train(0, "x");
   }
   
   /**
    * Verify that all version spaces passed in to the Iterable overload of 
    * union are trained.
    */
   @SuppressWarnings("unchecked") // mock.
   @Test
   public void testUnion_trainItterableArgs() {
      final String[] vs1Output = {"a", "b", "c"};
      final String[] vs2Output = {"d", "e", "f"};
      final String[] vs3Output = {"g", "h", "i"};
      
      final VS<Integer, String> vs3 = _context.mock(VS.class, "vs3");
      
      fillSimpleHypotheses(vs1Output, vs2Output);
      
      _context.checking(new Expectations() {{
         allowing(vs3).iterator();
         will(new ReturnNewHypothesesIteratorAction<String>(vs3Output));
         
         allowing(vs3).hypotheses();
         will(new ReturnNewHypothesesIteratorAction<String>(vs3Output));
         
         oneOf(_vs1).train(0, "x");
         oneOf(_vs2).train(0, "x");
         oneOf(vs3).train(0, "x");
      }});
      
      VS<Integer, String> parentVS = VS.union(ImmutableList.of(_vs1, _vs2, vs3));

      // run the training step.  
      // Mock expectations will cause failure if needed.
      parentVS.train(0, "x");
   }
   
   /**
    * Verify that filter properly prevents version spaces from being trained.
    */
   @Test
   public void testFilter_train() {
      String[] vs1Output = {"a", "b", "c"};
      String[] vs2Output = {"d", "e", "f"};
      
      fillSimpleHypotheses(vs1Output, vs2Output);
      
      _context.checking(new Expectations() {{      
         oneOf(_vs1).train(0, "x");
      }});
      
      VS<Integer, String> vs3 = VS.filter(_vs1, Predicates.<String>notNull());
      
      // run the training step.  
      // Mock expectations will cause failure if needed.
      vs3.train(0, "x");
      
      // passing null should *not* cause train to be invoked:
      vs3.train(0, null);
   }
   
   @Test
   public void testTransform_train() {
      // transform a child<integer,string> to a parent<Double, Character>
      Transform<Double, Integer, Character, String> tr = 
         new Transform<Double, Integer, Character, String>() {
            public Integer in(final Double parentIn) {
               return Integer.valueOf((int)Math.round(parentIn));
            }

            public Character out(final Double parentIn, final String childOut) {
               return childOut.charAt(0);
            }

            public String train(final Double parentIn, final Character parentOut) {
               return parentOut.toString();
            }
      };
     
      _context.checking(new Expectations() {{
         String[] vs1Output = {"a", "b", "c"};
         allowing(_vs1).iterator();
         will(new ReturnNewHypothesesIteratorAction<String>(vs1Output));
         
         allowing(_vs1).hypotheses();
         will(new ReturnNewHypothesesIteratorAction<String>(vs1Output));
         
         // expect the new child types that correspond to the training example.
         oneOf(_vs1).train(0, "x");
      }});
      
      VS<Double, Character> vs3 = VS.transform(_vs1, tr);

      // run the training step.  
      // Mock expectations will cause failure if needed.
      vs3.train(0.0, 'x');
   }
   
   /**
    * Verify that multitransforms invoke the child VS's training method 
    * multiple times.
    */
   @SuppressWarnings("unchecked") // jmock context.
   @Test
   public void testMultiTransform_train() {
      // transform a child<integer,string> to a parent<Double, Character>
      MultiTransform<Double, Integer, String, Character> tr =
         new MultiTransform<Double, Integer, String, Character>() {

            @Override
            public ImmutableList<Pair<Integer, Character>> multitrain(
                  final Double parentIn, final String parentOut) {
               
               Builder<Pair<Integer, Character>> builder = ImmutableList.builder();
               for (char ch : parentOut.toCharArray()) {
                  builder.add(
                        new Pair<Integer, Character>(parentIn.intValue(), ch));
               }
               return builder.build();
            }

            @Override
            public Integer in(final Double parentIn) {
               return parentIn.intValue();
            }

            @Override
            public String out(final Double parentIn, final Character childOut) {
               return childOut.toString();
            }
         
      };

      final VS<Integer, Character> childVS = _context.mock(VS.class, "childvs");

      _context.checking(new Expectations() {{
         String[] vs1Output = {"a", "b", "c"};
         allowing(childVS).iterator();
         will(new ReturnNewHypothesesIteratorAction<String>(vs1Output));
         
         allowing(childVS).hypotheses();
         will(new ReturnNewHypothesesIteratorAction<String>(vs1Output));
         
         // expect the new child types that correspond to the training example.
         oneOf(childVS).train(0, 'x');
         oneOf(childVS).train(0, 'y');
      }});
      
      VS<Double, String> parentVS = VS.transform(childVS, tr);

      // run the training step.  
      // Mock expectations will cause failure if needed.
      parentVS.train(0.0, "xy");
   }
   
   /**
    * Trains a supplied version space on the given examples, then runs that 
    * agent on the inputs in the tests set, and compares the result of each run with the
    * supplied output.
    *     
    * For example, to test a version space on two inputs after trainin on three 
    * integer examples, you could do this:
    * 
    * <code>
    *       testVS(new MinFixedIntVS(), 
    *        // training examples:
    *        ImmutableSet.of(
    *           new Pair<Integer,Integer>(100, 50),
    *           new Pair<Integer,Integer>(100, 40),
    *           new Pair<Integer,Integer>(100, 80)),
    *        // execution tests:
    *        ImmutableSet.of(new Pair<Integer, Map<Integer, Double>>(
    *              // input , [outputs->confidences]
    *              100, ImmutableMap.of(40, 1.0),
    *              80, ImmutableMap.of(40, 1.0),
    *        ))
    *  );
    * </code> 
    *     
    * @param <I> The input type for the version space.
    * @param <O> The output type for the version space.
    * @param vs The version space to test
    * @param examples A set of examples to train the version space on.
    * @param tests A set of input,output pairs to test the version space with.
    */
   public static <I, O> void testVS(final VS<I,O> vs, 
                              final Iterable<Pair<I,O>> examples,
                              final Iterable<Pair<I,Map<O,Double>>> tests) {
      // train the version space:
      for (Pair<I,O> ex : examples){
         vs.train(ex.a, ex.b);
      }
      
      int testNo = 0; 
      // For each test, run the VS on the input, then test on the output:
      for (Pair<I, Map<O, Double>> t : tests) {
         ImmutableMap<O, Double> results = vs.execute(t.a);
                     
         String testNumber = "[Test #: "+testNo+"] ";
         Assert.assertEquals(testNumber + "Wrong number of results: (got: " + results + ")", 
                  t.b.size(), results.size());
         testNo++;
                  
         for (Entry<O,Double> e : t.b.entrySet()) {
            O value     = e.getKey();
            Double conf = e.getValue();
            
            if (value instanceof Double) {
               // perform a different search, since we need to find doubles
               // that are *nearly* equal, but not *exactly* equal.
               checkForDouble(results, (Double)value, conf);
            } else {
               Assert.assertTrue(testNumber + "Missing desired result: ("+value+","+conf+") "+
                  "(got: "+results+")", 
                  results.containsKey(value));
               Assert.assertEquals(testNumber + "Wrong confidence for "+value, 
                  conf, results.get(value), EPSILON);
            }
         }
      }
   }

   /**
    * Helper method to find Doubles in result maps with delta equality.
    * 
    * This method *must* be invoked with an ImmutableMap<Double, Double>.  The
    * parameter type is not this specific because the invoking methods should 
    * not have a suppressWarnings annotation (it impairs our ability to find 
    * type errors). 
    */
   private static <O> void checkForDouble(final ImmutableMap<O, Double> results,
         final Double value, final Double conf) {
      try {
         Double[] keys = results.keySet().toArray(new Double[]{});
         int index = Arrays.binarySearch(keys, value, 
                                         SAFE_DOUBLE_COMPARATOR);
         if (index < 0) {
            fail("Missing desired result: ("+value+","+conf+") "+
                  "(got: "+results+")");
         } else {
            Double resultVal = keys[index];
            Assert.assertEquals("Wrong confidence for "+resultVal, 
                  conf, results.get(resultVal), EPSILON);
         }
      } catch (ArrayStoreException e) {
         throw new IllegalArgumentException(
               "results *must* be a map of Double -> Double");
      }
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
