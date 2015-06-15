package ptrman.agix0.src.java.Common.Evironment;

import org.apache.commons.math3.linear.ArrayRealVector;
import ptrman.agix0.src.java.Common.Scripting.EnvironmentScriptingAccessor;

import java.util.ArrayList;
import java.util.List;

/**
 * A "field" where the world is simulated
 */
public class Environment {
    public EnvironmentScriptingAccessor environmentScriptingAccessor;

    public List<Entity> entities = new ArrayList<>();

    public org.jbox2d.dynamics.World physicsWorld2d;

    public Environment() {
    }

    public void timestep() {
        for( Entity iterationEntity : entities ) {
            if( iterationEntity.body != null ) {
                iterationEntity.body.body.setTransform(iterationEntity.body.body.getPosition(), iterationEntity.angle2d);
            }
        }

        if( physicsWorld2d != null ) {
            physics2dApplyForcesToMatchSpeed();

            final float timeStep = 1.0f/60.0f; // we assume just 60 FPS
            final int velocityIterations = 6;
            final int positionIterations = 2;

            physicsWorld2d.step(timeStep, velocityIterations, positionIterations);
        }
    }

    private void physics2dApplyForcesToMatchSpeed() {
        // TODO< get speed and calculate delta force (quadratic with maximum), apply force >

        for( Entity iterationEntity : entities ) {
            environmentScriptingAccessor.physics2dApplyForce(iterationEntity.body, new ArrayRealVector(iterationEntity.getDirection().mapMultiply(iterationEntity.speed*30.0)));
        }
    }

    public void reset() {
        physicsWorld2d = null;
        entities = new ArrayList<>();
    }
}
