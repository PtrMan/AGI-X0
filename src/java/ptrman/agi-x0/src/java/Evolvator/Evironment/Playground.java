package Evolvator.Evironment;

import java.util.ArrayList;
import java.util.List;

/**
 * A "field" where the world is simulated
 */
public class Playground {
    public List<Entity> entities = new ArrayList<>();

    public void timestep() {
        for( Entity iterationEntity : entities ) {
            iterationEntity.position = iterationEntity.position.add(iterationEntity.direction.mapMultiply(iterationEntity.speed));
        }
    }
}
