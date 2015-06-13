package ptrman.agix0.src.java.Common.Evironment;

import java.util.ArrayList;
import java.util.List;

/**
 * A "field" where the world is simulated
 */
public class Environment {
    public List<Entity> entities = new ArrayList<>();

    public org.jbox2d.dynamics.World physicsWorld2d;

    public void timestep() {
        //for( Entity iterationEntity : entities ) {
        //    iterationEntity.position = iterationEntity.position.add(iterationEntity.direction.mapMultiply(iterationEntity.speed));
        //}

        if( physicsWorld2d != null ) {
            final float timeStep = 1.0f/60.0f; // we assume just 60 FPS
            final int velocityIterations = 6;
            final int positionIterations = 2;

            physicsWorld2d.step(timeStep, velocityIterations, positionIterations);
        }
    }

    public void reset() {
        physicsWorld2d = null;
        entities = new ArrayList<>();
    }
}
