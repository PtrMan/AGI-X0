package ptrman.agix0.src.java.SpinglassExplorer.Gui;

import org.apache.commons.math3.linear.ArrayRealVector;
import org.jbox2d.common.Vec2;
import ptrman.agix0.src.java.Common.Evironment.Entity;
import ptrman.agix0.src.java.Common.Evironment.Environment;

import java.awt.*;

// TODO< draw so the center is realy the center >
/**
 *
 */
public class Environment2dCanvas extends Canvas {
    private final Environment environment;

    public ArrayRealVector cameraCenter = new ArrayRealVector(new double[]{0.0, 0.0});

    public Environment2dCanvas(Environment environment) {
        this.environment = environment;
    }

    public void paint(Graphics graphics) {
        final double DIRECTION_RADIUS = 30.0;

        if( environment == null ) {
            return;
        }

        // just draw a line for each entity for now


        for( Entity iterationEntity : environment.entities ) {
            final Vec2 physicsPosition = iterationEntity.body.body.getPosition();
            final ArrayRealVector position = new ArrayRealVector(new double[]{physicsPosition.x, physicsPosition.y});

            ArrayRealVector projectedCenter = position.subtract(cameraCenter);

            ArrayRealVector projectDirectionEnd = projectedCenter.add(iterationEntity.getDirection().mapMultiply(DIRECTION_RADIUS));

            graphics.setColor(Color.RED);
            graphics.drawLine((int)projectedCenter.getDataRef()[0], (int)projectedCenter.getDataRef()[1], (int)projectDirectionEnd.getDataRef()[0], (int)projectDirectionEnd.getDataRef()[1]);
        }


        graphics.dispose();
    }
}
