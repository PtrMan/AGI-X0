/**
 * MultiTransform.java
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

import com.google.common.collect.ImmutableList;


/**
 * Transform to apply to a version space that may generate multiple training 
 * examples for each individual input from an encompassing version space.
 * 
 * @author rcreswick
 */
public interface MultiTransform<Iparent, Ichild, Oparent, Ochild> extends
      GeneralTransform<Iparent, Ichild, Oparent, Ochild> {

   /**
    * Turn the parent VS's input and output into the child VS's input and
    * output.
    * 
    * Used when one parent example corresponds to multiple child examples.
    * 
    * @param parentIn
    * @param parentOut
    * @return A collection of input, output pairs to train the child version
    *         space with.
    */
   ImmutableList<Pair<Ichild, Ochild>> multitrain(Iparent parentIn, Oparent parentOut);
}
