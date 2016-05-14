/**
 * Pair.java
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
public class Pair<T1, T2> {
   public final T1 a;
   public final T2 b;
   
   public static <T1, T2> Pair<T1, T2> pair(final T1 a, final T2 b) {
      return new Pair<T1, T2>(a, b);
   }
   
   /**
    * @param a
    * @param b
    */
   public Pair(final T1 a, final T2 b) {
      this.a = a;
      this.b = b;
   }
   
   
   
   /* (non-Javadoc)
    * @see java.lang.Object#hashCode()
    */
   @Override
   public int hashCode() {
      final int prime = 31;
      int result = 1;
      result = prime * result + ((a == null) ? 0 : a.hashCode());
      result = prime * result + ((b == null) ? 0 : b.hashCode());
      return result;
   }

   /* (non-Javadoc)
    * @see java.lang.Object#equals(java.lang.Object)
    */
   @SuppressWarnings("unchecked")
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
      Pair<T1,T2> other = (Pair<T1,T2>) obj;
      if (a == null) {
         if (other.a != null) {
            return false;
         }
      } else if (!a.equals(other.a)) {
         return false;
      }
      if (b == null) {
         if (other.b != null) {
            return false;
         }
      } else if (!b.equals(other.b)) {
         return false;
      }
      return true;
   }

   @Override
   public String toString() {
      return "("+a+","+b+")";
   }
}
