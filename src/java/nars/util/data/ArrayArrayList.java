package nars.util.data;

import org.apache.commons.lang3.ArrayUtils;

import java.io.Serializable;
import java.util.AbstractList;
import java.util.Deque;
import java.util.Iterator;
import java.util.RandomAccess;

/* High-performance Circular (Ring) Buffer. Not thread safe, and sacrifices safety for speed in other ways. */
public class ArrayArrayList<E> extends AbstractList<E> implements RandomAccess, Deque<E>, Serializable {

    public E[] array;


    public ArrayArrayList(E[] init) {
        this.array = init;
    }

    @Override
    public void clear() {
        array = null;
    }

    public boolean add(E e) {
        this.array = ArrayUtils.add(array, e);
        return true;
    }

    @Override
    public Iterator<E> iterator() {
        throw new RuntimeException("dont use iterators");
    }

    @Override
    public Iterator<E> descendingIterator() {
        throw new RuntimeException("dont use iterators");
    }


    @Override
    public int size() {
        return array.length;
        //return tail - head + (tail < head ? n : 0);
    }

    @Override
    public E get(final int i) {
        //same as the original function below but avoid another function call to help guarante inlining
        //int m = ;
        //if (m < 0) m += n;
        return array[i];

        //original code:
        //return buf[wrapIndex(head + i)];
    }

    public void setFast(final int i, final E e) {
        array[i] = e;
    }

    @Override
    public E set(final int m, final E e) {

        E existing = array[m];
        array[m] = e;
        return existing;
    }

    @Override
    public void add(final int i, final E e) {
        throw new RuntimeException("unsupported");
    }


    @Override
    public E remove(final int i) {
        E e = get(i);
        this.array = (E[]) ArrayUtils.remove(array, i);
        return e;
    }

    public boolean remove(Object o) {
        return remove(indexOf(o))!=null;
    }


    @Override
    public void addFirst(final E e) {
        add(0, e);
    }

    @Override
    public E getLast() {
        return get(size()-1);
    }


    @Override
    public void addLast(final E e) {
        this.array = (E[]) ArrayUtils.add(array, e);
    }



    @Override
    public E getFirst() {
        return get(0);
    }


    @Override
    public E removeFirst() {
        return remove(0);
    }


    @Override
    public E removeLast() {
        return remove(size()-1);
    }


    @Override
    final public boolean isEmpty() {
        return array!=null && array.length!=0;
    }

    @Override
    public boolean offerFirst(E e) {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public boolean offerLast(E e) {
        throw new UnsupportedOperationException("Not supported yet.");
    }



    @Override
    public E pollFirst() {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public E pollLast() {
        throw new UnsupportedOperationException("Not supported yet.");
    }



    @Override
    public E peekFirst() {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public E peekLast() {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public boolean removeFirstOccurrence(Object o) {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public boolean removeLastOccurrence(Object o) {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public boolean offer(E e) {
        return add(e);
    }

    @Override
    public E remove() {
        return removeLast();
    }

    @Override
    public E poll() {
        return remove();
    }

    @Override
    public E element() {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public E peek() {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public void push(E e) {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    @Override
    public E pop() {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    public E getModulo(int i) {
        return get(i % size());
    }

}
