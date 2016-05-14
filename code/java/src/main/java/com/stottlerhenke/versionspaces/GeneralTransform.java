/**
 * GeneralTransform.java
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

/**
 * @author rcreswick
 *
 */
interface GeneralTransform<Iparent, Ichild, Oparent, Ochild> {
   
   /**
    * Turn the parent VS's input into the child VS's input.
    * 
    * This is used to transform a problem input both for training and execution.
    * 
    * @param parentIn The parent's input.
    * @return The child's input.
    */
   Ichild in(Iparent parentIn);
   
   /**
    * Turn the child VS's output into parent VS's output
    * 
    * Used to translate the result of a VS execution into the 
    * desired higher-level result.
    * 
    * @param parentIn The parent VS's input.
    * @param childOut The output from the child VS.
    * 
    * @return The output to be returned by a parent VS.
    */
   Oparent out(Iparent parentIn, Ochild childOut);
}
