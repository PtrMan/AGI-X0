package ptrman.agix0.src.java.SpinglassExplorer.Gui;

import org.apache.commons.math3.linear.ArrayRealVector;
import ptrman.agix0.src.java.Evolvator.Evironment.Entity;
import ptrman.agix0.src.java.Evolvator.Evironment.Playground;

import java.awt.*;

// TODO< draw so the center is realy the center >
/**
 *
 */
public class Environment2dCanvas extends Canvas {
    private final Playground playground;

    public ArrayRealVector cameraCenter = new ArrayRealVector(new double[]{0.0, 0.0});

    public Environment2dCanvas(Playground playground) {
        this.playground = playground;
    }

    public void paint(Graphics graphics) {
        final double DIRECTION_RADIUS = 30.0;

        if( playground == null ) {
            return;
        }

        // just draw a line for each entity for now

        for( Entity iterationEntity : playground.entities ) {
            ArrayRealVector projectedCenter = cameraCenter.subtract(iterationEntity.position);

            ArrayRealVector projectDirectionEnd = projectedCenter.add(iterationEntity.direction.mapMultiply(DIRECTION_RADIUS));

            graphics.drawLine((int)projectedCenter.getDataRef()[0], (int)projectedCenter.getDataRef()[1], (int)projectDirectionEnd.getDataRef()[0], (int)projectDirectionEnd.getDataRef()[1]);
        }

        graphics.dispose();
    }
}
