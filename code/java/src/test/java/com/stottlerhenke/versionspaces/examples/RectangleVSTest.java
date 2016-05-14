/**
 * RectangleVSTest.java
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

import java.awt.Rectangle;
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
public class RectangleVSTest {

   private static final Rectangle RECT_0_0_50_50 = new Rectangle(0, 0, 50, 50);
   
   private static final Rectangle RECT_1024_768 = new Rectangle(0,0, 1024, 768);
   private static final Rectangle RECT_800_600 = new Rectangle(0,0, 800, 600);
   private static final Rectangle RECT_640_480 = new Rectangle(0,0, 640, 480);

   private static final Rectangle RECT_0_0_50_256 = new Rectangle(0, 0, 50, 256);
   private static final Rectangle RECT_0_0_50_200 = new Rectangle(0, 0, 50, 200);
   private static final Rectangle RECT_0_0_50_160 = new Rectangle(0, 0, 50, 160);
   

   /**
    * Test method for {@link com.stottlerhenke.versionspaces.CompositeVS#train(java.lang.Object, java.lang.Object)}.
    * 
    * Verify that this VS can learn fixed location / size rectangles.
    */
   @SuppressWarnings("unchecked")
   @Test
   public void testTrain_simpleRectangle() {
      VSTest.testVS(new RectangleVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Rectangle, Rectangle>(RECT_1024_768, RECT_0_0_50_50),
                  new Pair<Rectangle, Rectangle>(RECT_800_600, RECT_0_0_50_50)),
            // execution tests:
            ImmutableSet.of(new Pair<Rectangle, Map<Rectangle, Double>>(
                  RECT_640_480, ImmutableMap.of(RECT_0_0_50_50, 1.0)
            ))
      );
   }

   /**
    * Test method for {@link com.stottlerhenke.versionspaces.CompositeVS#train(java.lang.Object, java.lang.Object)}.
    * 
    * Verify that we can learn rectangles with fixed locations, fixed width, and 
    * relative heights.
    */
   @SuppressWarnings("unchecked")
   @Test
   public void testTrain_fixedW_relH() {
      VSTest.testVS(new RectangleVS(), 
            // training examples:
            ImmutableSet.of(
                  new Pair<Rectangle, Rectangle>(RECT_1024_768, RECT_0_0_50_256),
                  new Pair<Rectangle, Rectangle>(RECT_800_600, RECT_0_0_50_200)),
            // execution tests:
            ImmutableSet.of(new Pair<Rectangle, Map<Rectangle, Double>>(
                  RECT_640_480, ImmutableMap.of(RECT_0_0_50_160, 1.0)
            ))
      );
   }
}
