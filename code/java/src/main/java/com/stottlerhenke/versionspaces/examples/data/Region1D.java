/**
 * Region1D.java
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
package com.stottlerhenke.versionspaces.examples.data;

import java.io.Serializable;

/**
 * A one-dimensional immutable region.
 * 
 * Each Region1D object is an immutable region on a number line, represented by 
 * a (start, end) pair of integers. 
 * 
 * @author rcreswick
 */
public class Region1D implements Serializable {

   /** The start of the region */
   private final int _start;
   
   /** The end of the region */
   private final int _end;
   
   private static final long serialVersionUID = 1887342071822353705L;
   
   /**
    * Constructor.
    * 
    * @param start The start of the region.
    * @param end The end of the region.
    */
   public Region1D(final int start, final int end) {
      _start = start;
      _end = end;
   }
   
   /**
    * @return the start
    */
   public int getStart() {
      return _start;
   }
   /**
    * @return the end
    */
   public int getEnd() {
      return _end;
   }

   /**
    * Accessor for the size of this region.
    * @return The region's size.
    */
   public int getSize() {
      return getEnd() - getStart();
   }
   
   public double getCenter() {
      return (getStart() + getEnd()) / 2.0;
   }
   
   /* (non-Javadoc)
    * @see java.lang.Object#toString()
    */
   @Override
   public String toString() {
      return "["+getStart()+", "+getEnd()+"]";
   }

   /* (non-Javadoc)
    * @see java.lang.Object#hashCode()
    */
   @Override
   public int hashCode() {
      final int prime = 31;
      int result = 1;
      result = prime * result + _end;
      result = prime * result + _start;
      return result;
   }

   /* (non-Javadoc)
    * @see java.lang.Object#equals(java.lang.Object)
    */
   @Override
   public boolean equals(final Object obj) {
      if (this == obj) {
         return true;
      }
      if (obj == null) {
         return false;
      }
      if (getClass() != obj.getClass()) {
         return false;
      }
      Region1D other = (Region1D) obj;
      if (_end != other._end) {
         return false;
      }
      if (_start != other._start) {
         return false;
      }
      return true;
   }
}
