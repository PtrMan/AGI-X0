/**
 * RectangleVS.java
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

import com.stottlerhenke.versionspaces.CompositeVS;
import com.stottlerhenke.versionspaces.Pair;
import com.stottlerhenke.versionspaces.Transform;
import com.stottlerhenke.versionspaces.VS;
import com.stottlerhenke.versionspaces.examples.data.Region1D;

/**
 * Version Space that represents the set of rectangles with respect to an 
 * enclosing rectangle.
 * 
 * @author rcreswick
 */
public class RectangleVS extends CompositeVS<Rectangle, Rectangle> {
   
   private static final Transform<Rectangle, Pair<Region1D,Region1D>, 
                                  Rectangle, Pair<Region1D,Region1D>> TR = 
      new Transform<Rectangle, Pair<Region1D,Region1D>, 
                    Rectangle, Pair<Region1D,Region1D>>() {

      @Override // Pair<x out, y out>
      public Pair<Region1D, Region1D> train(final Rectangle parentIn,
                                            final Rectangle parentOut) {
         Region1D xExample = new Region1D(parentOut.x, 
               parentOut.x + parentOut.width);
         
         Region1D yExample = new Region1D(parentOut.y, 
               parentOut.y + parentOut.height);
         
         return new Pair<Region1D, Region1D>(xExample, yExample);
      }

      @Override
      public Pair<Region1D, Region1D> in(final Rectangle parentIn) {
         Region1D xIn = new Region1D(parentIn.x, 
               parentIn.x + parentIn.width);
         
         Region1D yIn = new Region1D(parentIn.y, 
               parentIn.y + parentIn.height);
         
         return new Pair<Region1D, Region1D>(xIn, yIn);
      }

      @Override
      public Rectangle out(final Rectangle parentIn,
                           final Pair<Region1D, Region1D> childOut) {
         Region1D xOut = childOut.a;
         Region1D yOut = childOut.b;
         
         return new Rectangle(xOut.getStart(), yOut.getStart(), 
               xOut.getSize(), yOut.getSize());
      }
   };

   /* (non-Javadoc)
    * @see com.stottlerhenke.versionspaces.CompositeVS#buildBackingVS()
    */
   @Override
   protected VS<Rectangle, Rectangle> buildBackingVS() {
      VS<Region1D, Region1D> xRegion = new Region1DVS();
      VS<Region1D, Region1D> yRegion = new Region1DVS();
      
      return VS.transform(VS.join(xRegion, yRegion), TR);
   }

}
