package ptrman.agix0.src.java.Common.Evironment;

import org.apache.commons.math3.linear.ArrayRealVector;

/**
 *
 */
public class Entity {
    public Physics2dBody body;

    public float speed = 0.0f; // speed in direction
    public float angle2d = 0.0f; // in rads

    public ArrayRealVector getDirection() {
        return new ArrayRealVector(new double[]{java.lang.Math.cos(angle2d), java.lang.Math.sin(angle2d)});
    }
}
