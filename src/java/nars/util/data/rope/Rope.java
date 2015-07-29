/*
 *  Rope.java
 *  Copyright (C) 2007 Amin Ahmad.
 *
 *  This file is part of Java Ropes.
 *
 *  Java Ropes is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Java Ropes is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Java Ropes.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Amin Ahmad can be contacted at amin.ahmad@gmail.com or on the web at
 *  www.ahmadsoft.org.
 */
package nars.util.data.rope;

import nars.util.data.rope.impl.*;

import java.io.IOException;
import java.io.PrintStream;
import java.io.Serializable;
import java.io.Writer;
import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * <p>
 * A rope represents character strings. Ropes are immutable which means that
 * once they are created, they cannot be changed. This makes them suitable for
 * sharing in multi-threaded environments.
 * </p><p>
 * Rope operations, unlike string operations, scale well to very long character
 * strings. Most mutation operations run in O(log n) time or better. However,
 * random-access character retrieval is generally slower than for a String. By
 * traversing consecutive characters with an iterator instead, performance
 * improves to O(1).
 * </p><p>
 * This rope implementation implements all performance optimizations outlined in
 * "<a
 * href="http://www.cs.ubc.ca/local/reading/proceedings/spe91-95/spe/vol25/issue12/spe986.pdf">Ropes:
 * an Alternative to Strings</a>" by Hans-J. Boehm, Russ Atkinson and Michael
 * Plass, including, notably, deferred evaluation of long substrings and
 * automatic rebalancing.
 * </p>
 * <h4>Immutability (a Caveat)</h4>
 * A rope is immutable. Specifically, calling any mutator function on a rope
 * always returns a modified copy; the original rope is left untouched. However,
 * care must be taken to build ropes from immutable <code>CharSequences</code>
 * such as <code>Strings</code>, or else from mutable <code>CharSequences</code>
 * that your program
 * <emph>guarantees will not change</emph>. Failure to do so will result in
 * logic errors.
 *
 * @author Amin Ahmad
 */
/*@ pure @*/ public interface Rope extends CharSequence, Iterable<Character>, Comparable<CharSequence>, Serializable {

    /**
     * Construct a rope from a character array.
     *
     * @param sequence a character array
     * @return a rope representing the underlying character array.
     */
    public static Rope build(final char[] sequence) {
        return new CharArrayRope(sequence);
    }

    /**
     * Construct a rope from an underlying character sequence.
     *
     * @param sequence the underlying character sequence.
     * @return a rope representing the underlying character sequnce.
     */
    public static Rope build(final CharSequence sequence) {
        if (sequence instanceof Rope) {
            return (Rope) sequence;
        }
        return new FlatCharSequenceRope(sequence);
    }

    /**
     * Builds a FastCharSequenceRope instead of FlatCharSequenceRope
     */
    public static CharSequence rope(final CharSequence sequence) {
        if (sequence instanceof Rope) {
            return sequence;
        }
        /*if ((sequence instanceof String) || (sequence instanceof TextBuilder))
         return new FastCharSequenceRope(sequence);*/

        if (sequence instanceof StringBuilder) {
            return Rope.sequence((StringBuilder) sequence);
        }

        //default for other implementations
        return new FlatCharSequenceRope(sequence);

    }

    /**
     * Returns a new rope created by appending the specified character to this
     * rope.
     *
     * @param c the specified character.
     * @return a new rope.
     */
    //@ ensures \result.length() == length() + 1;
    Rope append(char c);

    /**
     * Returns a new rope created by appending the specified character sequence
     * to this rope.
     *
     * @param suffix the specified suffix.
     * @return a new rope.
     */
    //@ requires suffix != null;
    //@ ensures \result.length() == length() + suffix.length();
    Rope append(CharSequence suffix);

    /**
     * Returns a new rope created by appending the specified character range to
     * this rope.
     *
     * @param csq the specified character.
     * @param start the start index, inclusive.
     * @param end the end index, non-inclusive.
     * @return a new rope.
     */
    //@ requires start <= end && start > -1 && end <= csq.length();
    //@ ensures \result.length() == (length() + (end-start));
    Rope append(CharSequence csq, int start, int end);

    /**
     * Creats a new rope by delete the specified character substring. The
     * substring begins at the specified <code>start</code> and extends to the
     * character at index <code>end - 1</code> or to the end of the sequence if
     * no such character exists. If <code>start</code> is equal to
     * <code>end</code>, no changes are made.
     *
     * @param start The beginning index, inclusive.
     * @param end The ending index, exclusive.
     * @return This object.
     * @throws StringIndexOutOfBoundsException if <code>start</code> is
     * negative, greater than <code>length()</code>, or greater than
     * <code>end</code>.
     */
    //@ requires start <= end && start > -1 && end <= length();
    //@ ensures \result.length() == (length() - (end-start));
    Rope delete(int start, int end);

    /**
     * Returns the index within this rope of the first occurrence of the
     * specified character. If a character with value <code>ch</code> occurs in
     * the character sequence represented by this <code>Rope</code> object, then
     * the index of the first such occurrence is returned -- that is, the
     * smallest value k such that:
     * <p>
     * <code>this.charAt(k) == ch</code>
     * <p>
     * is <code>true</code>. If no such character occurs in this string, then
     * <code>-1</code> is returned.
     *
     * @param ch a character.
     * @return the index of the first occurrence of the character in the
     * character sequence represented by this object, or <code>-1</code> if the
     * character does not occur.
     */
    //@ ensures \result >= -1 && \result < length();
    int indexOf(char ch);

    /**
     * Returns the index within this rope of the first occurrence of the
     * specified character, beginning at the specified index. If a character
     * with value <code>ch</code> occurs in the character sequence represented
     * by this <code>Rope</code> object, then the index of the first such
     * occurrence is returned&#8212;that is, the smallest value k such that:
     * <p>
     * <code>this.charAt(k) == ch</code>
     * <p>
     * is <code>true</code>. If no such character occurs in this string, then
     * <code>-1</code> is returned.
     *
     * @param ch a character.
     * @param fromIndex the index to start searching from.
     * @return the index of the first occurrence of the character in the
     * character sequence represented by this object, or -1 if the character
     * does not occur.
     */
    //@ requires fromIndex > -1 && fromIndex < length();
    //@ ensures \result >= -1 && \result < length();
    int indexOf(char ch, int fromIndex);

    /**
     * Returns the index within this rope of the first occurrence of the
     * specified string. The value returned is the smallest <i>k</i> such that:
     * <pre>
     *     this.startsWith(str, k)
     * </pre> If no such <i>k</i> exists, then -1 is returned.
     *
     * @param sequence the string to find.
     * @return the index of the first occurrence of the specified string, or -1
     * if the specified string does not occur.
     */
    //@ requires sequence != null;
    //@ ensures \result >= -1 && \result < length();
    int indexOf(CharSequence sequence);

    /**
     * Returns the index within this rope of the first occurrence of the
     * specified string, beginning at the specified index. The value returned is
     * the smallest <i>k</i> such that:
     * <pre>
     *     k >= fromIndex && this.startsWith(str, k)
     * </pre> If no such <i>k</i> exists, then -1 is returned.
     *
     * @param sequence the string to find.
     * @param fromIndex the index to start searching from.
     * @return the index of the first occurrence of the specified string, or -1
     * if the specified string does not occur.
     */
    //@ requires sequence != null && fromIndex > -1 && fromIndex < length();
    //@ ensures \result >= -1 && \result < length();
    int indexOf(CharSequence sequence, int fromIndex);

    /**
     * Creates a new rope by inserting the specified <code>CharSequence</code>
     * into this rope.
     * <p>
     * The characters of the <code>CharSequence</code> argument are inserted, in
     * order, into this rope at the indicated offset.
     *
     * <p>
     * If <code>s</code> is <code>null</code>, then the four characters
     * <code>"null"</code> are inserted into this sequence.
     *
     * @param dstOffset the offset.
     * @param s the sequence to be inserted
     * @return a reference to the new Rope.
     * @throws IndexOutOfBoundsException if the offset is invalid.
     */
    //@ requires dstOffset > -1 && dstOffset <= length();
    Rope insert(int dstOffset, CharSequence s);

    /**
     * Returns an iterator positioned to start at the specified index.
     *
     * @param start the start position.
     * @return an iterator positioned to start at the specified index.
     */
    //@ requires start > -1 && start < length();
    Iterator<Character> iterator(int start);

    /**
     * Trims all whitespace as well as characters less than 0x20 from the
     * beginning of this string.
     *
     * @return a rope with all leading whitespace trimmed.
     */
    //@ ensures \result.length() <= length();
    Rope trimStart();

    /**
     * Creates a matcher that will match this rope against the specified
     * pattern. This method produces a higher performance matcher than:
     * <pre>
     * Matcher m = pattern.matcher(this);
     * </pre> The difference may be asymptotically better in some cases.
     *
     * @param pattern the pattern to match this rope against.
     * @return a matcher.
     */
    //@ requires pattern != null;
    Matcher matcher(Pattern pattern);

    /**
     * Returns <code>true</code> if this rope matches the specified
     * <code>Pattern</code>, or <code>false</code> otherwise.
     *
     * @see java.util.regex.Pattern
     * @param regex the specified regular expression.
     * @return <code>true</code> if this rope matches the specified
     * <code>Pattern</code>, or <code>false</code> otherwise.
     */
    public boolean matches(Pattern regex);

    /**
     * Returns <code>true</code> if this rope matches the specified regular
     * expression, or <code>false</code> otherwise.
     *
     * @see java.util.regex.Pattern
     * @param regex the specified regular expression.
     * @return <code>true</code> if this rope matches the specified regular
     * expression, or <code>false</code> otherwise.
     */
    public boolean matches(String regex);

    /**
     * Rebalances the current rope, returning the rebalanced rope. In general,
     * rope rebalancing is handled automatically, but this method is provided to
     * give users more control.
     *
     * @return a rebalanced rope.
     */
    public Rope rebalance();

    /**
     * Reverses this rope.
     *
     * @return a reversed copy of this rope.
     */
    public Rope reverse();

    /**
     * Returns a reverse iterator positioned to start at the end of this rope. A
     * reverse iterator moves backwards instead of forwards through a rope.
     *
     * @return A reverse iterator positioned at the end of this rope.
     * @see Rope#reverseIterator(int)
     */
    Iterator<Character> reverseIterator();

    /**
     * Returns a reverse iterator positioned to start at the specified index. A
     * reverse iterator moves backwards instead of forwards through a rope.
     *
     * @param start the start position.
     * @return a reverse iterator positioned to start at the specified index
     * from the end of the rope. For example, a value of 1 indicates the
     * iterator should start 1 character before the end of the rope.
     * @see Rope#reverseIterator()
     */
    Iterator<Character> reverseIterator(int start);

    /**
     * Trims all whitespace as well as characters less than <code>0x20</code>
     * from the end of this rope.
     *
     * @return a rope with all trailing whitespace trimmed.
     */
    //@ ensures \result.length() <= length();
    Rope trimEnd();

    @Override
    Rope subSequence(int start, int end);

    /**
     * Trims all whitespace as well as characters less than <code>0x20</code>
     * from the beginning and end of this string.
     *
     * @return a rope with all leading and trailing whitespace trimmed.
     */
    Rope trim();

    /**
     * Write this rope to a <code>Writer</code>.
     *
     * @param out the writer object.
     */
    public void write(Writer out) throws IOException;

    /**
     * Write a range of this rope to a <code>Writer</code>.
     *
     * @param out the writer object.
     * @param offset the range offset.
     * @param length the range length.
     */
    public void write(Writer out, int offset, int length) throws IOException;

    /**
     * Increase the length of this rope to the specified length by prepending
     * spaces to this rope. If the specified length is less than or equal to the
     * current length of the rope, the rope is returned unmodified.
     *
     * @param toLength the desired length.
     * @return the padded rope.
     * @see #padStart(int, char)
     */
    public Rope padStart(int toLength);

    /**
     * Increase the length of this rope to the specified length by repeatedly
     * prepending the specified character to this rope. If the specified length
     * is less than or equal to the current length of the rope, the rope is
     * returned unmodified.
     *
     * @param toLength the desired length.
     * @param padChar the character to use for padding.
     * @return the padded rope.
     * @see #padStart(int, char)
     */
    public Rope padStart(int toLength, char padChar);

    /**
     * Increase the length of this rope to the specified length by appending
     * spaces to this rope. If the specified length is less than or equal to the
     * current length of the rope, the rope is returned unmodified.
     *
     * @param toLength the desired length.
     * @return the padded rope.
     * @see #padStart(int, char)
     */
    public Rope padEnd(int toLength);

    /**
     * Increase the length of this rope to the specified length by repeatedly
     * appending the specified character to this rope. If the specified length
     * is less than or equal to the current length of the rope, the rope is
     * returned unmodified.
     *
     * @param toLength the desired length.
     * @param padChar the character to use for padding.
     * @return the padded rope.
     * @see #padStart(int, char)
     */
    public Rope padEnd(int toLength, char padChar);

    /**
     * Returns true if and only if the length of this rope is zero.
     *
     * @return <code>true</code> if and only if the length of this rope is zero,
     * and <code>false</code> otherwise.
     */
    public boolean isEmpty();

    /**
     * Returns <code>true</code> if this rope starts with the specified prefix.
     *
     * @param prefix the prefix to test.
     * @return <code>true</code> if this rope starts with the specified prefix
     * and <code>false</code> otherwise.
     * @see #startsWith(CharSequence, int)
     */
    public boolean startsWith(CharSequence prefix);

    /**
     * Returns <code>true</code> if this rope, beginning from a specified
     * offset, starts with the specified prefix.
     *
     * @param prefix the prefix to test.
     * @param offset the start offset.
     * @return <code>true</code> if this rope starts with the specified prefix
     * and <code>false</code> otherwise.
     */
    public boolean startsWith(CharSequence prefix, int offset);

    /**
     * Returns <code>true</code> if this rope ends with the specified suffix.
     *
     * @param suffix the suffix to test.
     * @return <code>true</code> if this rope starts with the specified suffix
     * and <code>false</code> otherwise.
     * @see #endsWith(CharSequence, int)
     */
    public boolean endsWith(CharSequence suffix);

    /**
     * Returns <code>true</code> if this rope, terminated at a specified offset,
     * ends with the specified suffix.
     *
     * @param suffix the suffix to test.
     * @param offset the termination offset, counted from the end of the rope.
     * @return <code>true</code> if this rope starts with the specified prefix
     * and <code>false</code> otherwise.
     */
    public boolean endsWith(CharSequence suffix, int offset);

    public static final long[] FIBONACCI = {0l, 1l, 1l, 2l, 3l, 5l, 8l, 13l, 21l, 34l, 55l, 89l, 144l, 233l, 377l, 610l, 987l, 1597l, 2584l, 4181l, 6765l, 10946l, 17711l, 28657l, 46368l, 75025l, 121393l, 196418l, 317811l, 514229l, 832040l, 1346269l, 2178309l, 3524578l, 5702887l, 9227465l, 14930352l, 24157817l, 39088169l, 63245986l, 102334155l, 165580141l, 267914296l, 433494437l, 701408733l, 1134903170l, 1836311903l, 2971215073l, 4807526976l, 7778742049l, 12586269025l, 20365011074l, 32951280099l, 53316291173l, 86267571272l, 139583862445l, 225851433717l, 365435296162l, 591286729879l, 956722026041l, 1548008755920l, 2504730781961l, 4052739537881l, 6557470319842l, 10610209857723l, 17167680177565l, 27777890035288l, 44945570212853l, 72723460248141l, 117669030460994l, 190392490709135l, 308061521170129l, 498454011879264l, 806515533049393l, 1304969544928657l, 2111485077978050l, 3416454622906707l, 5527939700884757l, 8944394323791464l, 14472334024676221l, 23416728348467685l, 37889062373143906l, 61305790721611591l, 99194853094755497l, 160500643816367088l, 259695496911122585l, 420196140727489673l, 679891637638612258l, 1100087778366101931l, 1779979416004714189l, 2880067194370816120l, 4660046610375530309l, 7540113804746346429l};
    public static final short MAX_ROPE_DEPTH = 96;
    public static final String SPACES = "                                                                                                                                                                                                        ";

    /**
     * Rebalance a rope if the depth has exceeded MAX_ROPE_DEPTH. If the rope
     * depth is less than MAX_ROPE_DEPTH or if the rope is of unknown type, no
     * rebalancing will occur.
     *
     * @param r the rope to rebalance.
     * @return a rebalanced copy of the specified rope.
     */
    public static Rope autoRebalance(final Rope r) {
        if (r instanceof AbstractRope && ((AbstractRope) r).depth() > MAX_ROPE_DEPTH) {
            return rebalance(r);
        } else {
            return r;
        }
    }

    /**
     * @param c array of terms to concatenate; if an item is null it will be
     * ignored
     */
    public static Rope cat(final CharSequence... c) {
        Rope r = null;
        for (CharSequence a : c) {
            if (a == null) {
                continue;
            }

            if (!(a instanceof Rope)) {
                a = Rope.build(a);
            }

            r = (r == null) ? (Rope) a : new ConcatenationRope(r, (Rope) a);

        }
        return r;
    }

    final static CharArrayRope emptyCharArray = new CharArrayRope(new char[]{});

    /**
     * @param c array of terms to concatenate; if an item is null it will be
     * ignored
     */
    public static Rope catFast(final CharSequence... c) {
        Rope r = null;
        for (CharSequence a : c) {
            if (a == null) {
                a = emptyCharArray; //empty placeholder to maintain structure for FastConcatenationRope hash comparisons
            }
            if (!(a instanceof Rope)) {
                a = Rope.rope(a);
            }

            r = (r == null) ? (Rope) a : new FastConcatenationRope(r, (Rope) a);

        }
        return r;
    }

    /**
     * Concatenate two ropes. Implements all recommended optimizations in
     * "Ropes: an Alternative to Strings".
     *
     * @param left the first rope.
     * @param right the second rope.
     * @return the concatenation of the specified ropes.
     */
    public static Rope cat(final Rope left, final Rope right) {
        if (left.length() == 0) {
            return right;
        }
        if (right.length() == 0) {
            return left;
        }
        if ((long) left.length() + right.length() > Integer.MAX_VALUE) {
            throw new IllegalArgumentException(
                    "Left length=" + left.length() + ", right length=" + right.length()
                    + ". Concatenation would overflow length field.");
        }
        final int combineLength = 17;
        if (left.length() + right.length() < combineLength) {
            return new FlatCharSequenceRope(left.toString() + right.toString());
        }
        if (!(left instanceof ConcatenationRope)) {
            if (right instanceof ConcatenationRope) {
                final ConcatenationRope cRight = (ConcatenationRope) right;
                if (left.length() + cRight.getLeft().length() < combineLength) {
                    return autoRebalance(new ConcatenationRope(new FlatCharSequenceRope(left.toString() + cRight.getLeft().toString()), cRight.getRight()));
                }
            }
        }
        if (!(right instanceof ConcatenationRope)) {
            if (left instanceof ConcatenationRope) {
                final ConcatenationRope cLeft = (ConcatenationRope) left;
                if (right.length() + cLeft.getRight().length() < combineLength) {
                    return autoRebalance(new ConcatenationRope(cLeft.getLeft(), new FlatCharSequenceRope(cLeft.getRight().toString() + right.toString())));
                }
            }
        }

        return autoRebalance(new ConcatenationRope(left, right));
    }

    /**
     * Returns the depth of the specified rope.
     *
     * @param r the rope.
     * @return the depth of the specified rope.
     */
    public static byte depth(final Rope r) {
        if (r instanceof AbstractRope) {
            return ((AbstractRope) r).depth();
        } else {
            return 0;
            //throw new IllegalArgumentException("Bad rope");
        }
    }

    public static boolean isBalanced(final Rope r) {
        final byte depth = depth(r);
        if (depth >= FIBONACCI.length - 2) {
            return false;
        }
        return (FIBONACCI[depth + 2] <= r.length());	// TODO: not necessarily valid w/e.g. padding char sequences.	
    }

    public static Rope rebalance(final Rope r) {
        // get all the nodes into a list

        final ArrayList<Rope> leafNodes = new ArrayList<>();
        final ArrayDeque<Rope> toExamine = new ArrayDeque<>();
        // begin a depth first loop.
        toExamine.add(r);
        while (toExamine.size() > 0) {
            final Rope x = toExamine.pop();
            if (x instanceof ConcatenationRope) {
                toExamine.push(((ConcatenationRope) x).getRight());
                toExamine.push(((ConcatenationRope) x).getLeft());
            } else {
                leafNodes.add(x);
            }
        }
        Rope result = merge(leafNodes, 0, leafNodes.size());
        return result;
    }

    public static Rope merge(ArrayList<Rope> leafNodes, int start, int end) {
        int range = end - start;
        switch (range) {
            case 1:
                return leafNodes.get(start);
            case 2:
                return new ConcatenationRope(leafNodes.get(start), leafNodes.get(start + 1));
            default:
                int middle = start + (range / 2);
                return new ConcatenationRope(merge(leafNodes, start, middle), merge(leafNodes, middle, end));
        }
    }

    /**
     * Visualize a rope.
     *
     * @param r
     * @param out
     */
    public static void visualize(final Rope r, final PrintStream out) {
        visualize(r, out, 0);
    }

    public static void visualize(final Rope r, final PrintStream out, final int depth) {
        if (r instanceof FlatCharSequenceRope) {
            out.print(SPACES.substring(0, depth * 2));
            CharSequence seq = ((FlatCharSequenceRope) r).sequence;
            out.println("\"" + seq + "\" " + System.identityHashCode(seq));
//			out.println(r.length());
        } else if (r instanceof FlatRope) {
            out.print(SPACES.substring(0, depth * 2));
            out.println("\"" + r + '"');
//			out.println(r.length());
        }
        if (r instanceof SubstringRope) {
            out.print(SPACES.substring(0, depth * 2));
            out.println("substring " + r.length() + " \"" + r + '"');
//			this.visualize(((SubstringRope)r).getRope(), out, depth+1);
        }
        if (r instanceof ConcatenationRope) {
            out.print(SPACES.substring(0, depth * 2));
            out.println("concat[left]");
            visualize(((ConcatenationRope) r).getLeft(), out, depth + 1);
            out.print(SPACES.substring(0, depth * 2));
            out.println("concat[right]");
            visualize(((ConcatenationRope) r).getRight(), out, depth + 1);
        }
        if (r instanceof PrePostCharRope) {
            PrePostCharRope p = (PrePostCharRope) r;
            out.print(SPACES.substring(0, depth * 2));
            out.println("\'" + p.pre + '\'');
            Rope.visualize(p.content, out, depth + 1);
            out.println("\'" + p.post + '\'');
        }
    }

    public static void stats(final Rope r, final PrintStream out) {
        int nonLeaf = 0;
        final ArrayList<Rope> leafNodes = new ArrayList<>();
        final ArrayDeque<Rope> toExamine = new ArrayDeque<>();
        // begin a depth first loop.
        toExamine.add(r);
        while (toExamine.size() > 0) {
            final Rope x = toExamine.pop();
            if (x instanceof ConcatenationRope) {
                ++nonLeaf;
                toExamine.push(((ConcatenationRope) x).getRight());
                toExamine.push(((ConcatenationRope) x).getLeft());
            } else {
                leafNodes.add(x);
            }
        }
        out.println("rope(length=" + r.length() + ", leaf nodes=" + leafNodes.size() + ", non-leaf nodes=" + nonLeaf + ", depth=" + depth(r) + ')');
    }

    /**
     * wraps a StringBuilder in CharArrayRope for use as a general purpose immutable CharSequence.
     * StringBuilder lacks hashCode and other support that CharArrayRope provides.
     * CharArrayRope can use the StringBuilder's underlying char[] directly without copy.
     */
    static CharSequence sequence(StringBuilder b) {
        return new CharArrayRope(b);
    }

    /**
     * Warning: don't modify the return char[] because it will beinconsistent with s.hashCode()
     * @param String to invade
     * @return the private char[] field in String class
     */
    public static char[] getCharArray(String s) {
        try {
            return (char[]) StringHack.val.get(s);
        } catch (Exception ex) {
            ex.printStackTrace();
        }
        return null;
    }

    public static char[] getCharArray(StringBuilder s) {
        try {
            return (char[]) StringHack.sbval.get(s);
        } catch (Exception ex) {
            ex.printStackTrace();
        }
        return null;
    }

}
