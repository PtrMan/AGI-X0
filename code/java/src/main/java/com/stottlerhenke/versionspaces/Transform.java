/**
 * Transfom.java
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
public interface Transform<Iparent, Ichild, Oparent, Ochild> extends 
GeneralTransform<Iparent, Ichild, Oparent, Ochild> {

   /**
    * Turn the parent VS's output into the child VS's output.
    * 
    * This is used to transform a training example so that a child VS. 
    * can use it.
    * 
    * @param parentIn The parent VS's input.
    * @param parentOut The parent VS's output.
    * 
    * @return The child VS's output.
    */
   Ochild train(Iparent parentIn, Oparent parentOut);
}
