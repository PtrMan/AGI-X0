/**
 * VS.java
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

import java.util.Iterator;
import java.util.Map;

import com.google.common.base.Predicate;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.Maps;
import com.google.common.collect.UnmodifiableIterator;

/**
 * General purpose Version Space definition.   
 * 
 * @author rcreswick
 */
public abstract class VS<I, O> 
implements Iterable<ConfidentHypothesis<I, O>>{
   
   /**
    * Train this version space on the specified input and output.
    * 
    * @param in The example input.
    * @param out The desired output.
    */
   public abstract void train(I in, O out);

   /**
    * Accessor for the set of valid hypotheses in this version space.
    * 
    * Implementations may choose a non-exhaustive return value to represent the
    * fully un-constrained version space.
    * 
    * @return The collection of valid hypotheses, paired with the confidence in
    *         each hypothesis.
    */
   public abstract UnmodifiableIterator<ConfidentHypothesis<I, O>> hypotheses();

   @Override
   public Iterator<ConfidentHypothesis<I,O>> iterator() {
      return hypotheses();
   }
   
   /**
    * Join two Version Spaces to generate a third composite version space.
    * 
    * @param <I1>
    *           The input type of the first version space.
    * @param <I2>
    *           The input type of the second version space.
    * @param <O1>
    *           The output type of the first version space.
    * @param <O2>
    *           The output type of the second version space.
    * @param vs1
    *           The first version space.
    * @param vs2
    *           The second version space.
    * @return A composite version space that acts on, and returns ordered pairs
    *         of input and outputs.
    */
   public static <I1, I2, O1, O2> VS<Pair<I1, I2>, Pair<O1, O2>> 
   join(final VS<I1, O1> vs1, final VS<I2, O2> vs2) {
      return new VS<Pair<I1, I2>, Pair<O1, O2>>() {
         @Override
         public
         void 
         train(final Pair<I1, I2> in, final Pair<O1, O2> out) {
            vs1.train(in.a, out.a);
            vs2.train(in.b, out.b);
         }

         @Override
         public
         UnmodifiableIterator<ConfidentHypothesis<Pair<I1, I2>, Pair<O1, O2>>> 
         hypotheses() {
            return new VSJoinIterator<I1, I2, O1, O2>(vs1, vs2);
         }
      };
   }

   /**
    * Unions two Version Spaces to generate a third composite version space.
    * 
    * @param <I>
    *           Input type for all three version spaces involved
    * @param <O>
    *           Output type for all three version spaces involved
    * @param vs1
    *           The first component version space
    * @param vs2
    *           The second component version space.
    * @return A composite version space that encapsulates the two component
    *         version spaces.
    */
   public static <I,O>  VS<I, O> 
   union(final VS<I, O> vs1, final VS<I, O> vs2) {
      return new VS<I, O>() {
         @Override
         public
         void train(final I in, final O out) {
            vs1.train(in, out);
            vs2.train(in, out);
         }

         @Override
         public
         UnmodifiableIterator<ConfidentHypothesis<I, O>> 
         hypotheses() {
            return new VSUnionIterator<I, O>(vs1, vs2);
         }
      };
   }
   
   /**
    * Unions 1-N Version Spaces to generate a third composite version space.
    * 
    * @param <I>
    *           Input type for all three version spaces involved
    * @param <O>
    *           Output type for all three version spaces involved
    * @param vss
    *           The {@code Iterable} collection of Version Spaces to union.
    * @return A composite version space that encapsulates the two component
    *         version spaces.
    */
   public static <I,O>  VS<I, O> 
   union(final Iterable<VS<I, O>> vss) {
      return new VS<I, O>() {
         @Override
         public
         void train(final I in, final O out) {
            for (VS<I, O> vs : vss) {
               vs.train(in, out);
            }
         }

         @Override
         public
         UnmodifiableIterator<ConfidentHypothesis<I, O>> 
         hypotheses() {
            return new VSUnionIterator<I, O>(vss);
         }
      };
   }
   
   /**
    * Transforms the input and output types of a version space.
    * 
    * Given a version space that operates on an input type <code>Ic</code> and
    * produces results of type <code>Oc</code>, transform generates a new
    * version space that operates on input type <code>Ip</code> and produces
    * results of type <code>Op</code>. The details of these type conversions are
    * given by the provided transform object <code>tr</code>
    * 
    * Note that a transform is not restricted to performing type conversions.
    * The input and output types of the new version space <emph>can</emph> be
    * the same as the input version space. A transform can do any combination of
    * type and value transformations.
    * 
    * @param <Ip>
    *           The input type of the returned version space.
    * @param <Op>
    *           The output type of the returned version space.
    * @param <Ic>
    *           The input type of the provided version space.
    * @param <Oc>
    *           The output type of the provided version space.
    * @param vs
    *           The version space to transform.
    * @param tr
    *           The transformation to apply.
    * @return A new version space that operates on new input and output types.
    */
   public static <Ip, Op, Ic, Oc> VS<Ip, Op> 
   transform(final VS<Ic, Oc> vs, final Transform<Ip, Ic, Op, Oc> tr) {
      return new VS<Ip, Op>() {
         @Override
         public
         void train(final Ip in, final Op out) {
            vs.train(tr.in(in), tr.train(in, out));
         }

         @Override
         public
         UnmodifiableIterator<ConfidentHypothesis<Ip, Op>> 
         hypotheses() {
            return new VSTransformIterator<Ip, Op, Ic, Oc>(vs, tr);
         }
      };
   }
   
   /**
    * Transforms the input and output types of a version space.
    * 
    * This overload uses a multitransform, which allows each training example to 
    * be broken up into multiple training examples for a composite version space.
    * This is therefore a more general way to perform a transform, however, it 
    * is somewhat more complex.
    * 
    * @param <Ip>
    *           The input type of the returned version space.
    * @param <Op>
    *           The output type of the returned version space.
    * @param <Ic>
    *           The input type of the provided version space.
    * @param <Oc>
    *           The output type of the provided version space.
    * @param vs
    *           The version space to transform.
    * @param tr
    *           The transformation to apply.
    * @return A new version space that operates on new input and output types.
    */
   public static <Ip, Op, Ic, Oc> VS<Ip, Op> 
   transform(final VS<Ic, Oc> vs, final MultiTransform<Ip, Ic, Op, Oc> tr) {
      return new VS<Ip, Op>() {
         @Override
         public
         void train(final Ip in, final Op out) {
            for (Pair<Ic, Oc> example : tr.multitrain(in, out)) {
               vs.train(example.a, example.b);               
            }
         }

         @Override
         public
         UnmodifiableIterator<ConfidentHypothesis<Ip, Op>> 
         hypotheses() {
            return new VSTransformIterator<Ip, Op, Ic, Oc>(vs, tr);
         }
      };
   }
   
   /**
    * Generates a version space that only trains on examples that pass a
    * supplied filter.
    * 
    * @param <In>
    *           Input type for all version spaces involved
    * @param <Out>
    *           Output type for all version spaces involved
    * @param vs
    *           The version space to filter training examples for.
    * @param filter
    *           The filter to apply to training examples.
    * @return A version space that ignores certain training examples.
    */
   public static <In, Out> VS<In, Out> 
   filter(final VS<In, Out> vs, final Predicate<Out> filter) {
      return new VS<In, Out>() {
         @Override
         public
         void train(final In in, final Out out) {
            if (filter.apply(out)) {
               vs.train(in, out);
            }
         }

         @Override
         public
         UnmodifiableIterator<ConfidentHypothesis<In, Out>> 
         hypotheses() {
            return vs.hypotheses();
         }
      };
   }
   
   /**
    * Executes a version space on the given input, returning a collection of
    * results.
    * 
    * @param in
    *           The input to act on.
    * @return A mapping of results to confidences in those results.
    */
   public ImmutableMap<O, Double> execute(final I in) {
      final Map<O, Double> m = Maps.newHashMap();
      
      for (final ConfidentHypothesis<I, O> entry : this) {
         final Hypothesis<I, O> h = entry.getHypothesis();

         // increment by confidence if h.eval(in) exists, else just store conf.
         store(m, h.eval(in), entry.getConfidence());
      }
      return ImmutableMap.copyOf(m);
   }

   /**
    * Helper method to increment the value in a map by the given confidence.
    * 
    * @param m The map to work on
    * @param result The key.
    * @param c The change in value.
    */
   private void store(final Map<O, Double> m, final O result, final Double c) {
      double value = c;
      if (m.containsKey(result)) {
         value = value + m.get(result);
      }

      m.put(result, value);
   }


}