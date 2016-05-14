/**
 * LocationSpec.java
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
 * @author rcreswick
 *
 */
public class LocationSpec implements Serializable {
   
   private final int _offset;
   private final Anchor _anchor;
   
   private static final long serialVersionUID = 5854254992626410591L;
   
   /**
    * @param offset
    * @param anchor
    */
   public LocationSpec(final int offset, final Anchor anchor) {
      super();
      _offset = offset;
      _anchor = anchor;
   }
   
   /**
    * @return the offset
    */
   public int getOffset() {
      return _offset;
   }
   
   /**
    * @return the anchor
    */
   public Anchor getAnchor() {
      return _anchor;
   }

   /* (non-Javadoc)
    * @see java.lang.Object#toString()
    */
   @Override
   public String toString() {
      return _anchor.name()+": "+_offset;
   }

   /* (non-Javadoc)
    * @see java.lang.Object#hashCode()
    */
   @Override
   public int hashCode() {
      final int prime = 31;
      int result = 1;
      result = prime * result + ((_anchor == null) ? 0 : _anchor.hashCode());
      result = prime * result + _offset;
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
      LocationSpec other = (LocationSpec) obj;
      if (_anchor == null) {
         if (other._anchor != null) {
            return false;
         }
      } else if (!_anchor.equals(other._anchor)) {
         return false;
      }
      if (_offset != other._offset) {
         return false;
      }
      return true;
   }
}
