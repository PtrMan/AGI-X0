package ptrman.agix0.src.java.Common.Scripting;

import org.apache.commons.math3.linear.ArrayRealVector;
import org.jbox2d.collision.shapes.PolygonShape;
import org.jbox2d.common.Vec2;
import org.jbox2d.dynamics.*;
import ptrman.agix0.src.java.Common.Evironment.Environment;
import ptrman.agix0.src.java.Common.Evironment.Physics2dBody;

/**
 *
 */
public class EnvironmentScriptingAccessor {

    private final Environment environment;

    public enum EnumShapeType {
        BOX
    }

    public EnvironmentScriptingAccessor(Environment entry) {
        this.environment = entry;
    }

    // must be called before using the 2d physics
    public void physics2dCreateWorld() {
        environment.physicsWorld2d = new World(new Vec2(0.0f, 0.0f));
    }

    public Physics2dBody physics2dCreateBody(boolean fixed, String shapeType, ArrayRealVector position, ArrayRealVector size, float radius, float density, float friction) {
        BodyDef bodyDefinition = new BodyDef();
        bodyDefinition.position.set(new Vec2((float) position.getDataRef()[0], (float) position.getDataRef()[1]));

        if( fixed ) {
            bodyDefinition.type = BodyType.STATIC;
        }
        else {
            bodyDefinition.type = BodyType.DYNAMIC;
        }

        Body body = environment.physicsWorld2d.createBody(bodyDefinition);

        PolygonShape polygonShape = new PolygonShape();

        if( shapeType.equals("BOX") ) {
            polygonShape.setAsBox((float)size.getDataRef()[0] * 0.5f, (float)size.getDataRef()[1] * 0.5f);
        }
        else {
            throw new InternalError();
        }

        if( fixed ) {
            body.createFixture(polygonShape, 0.0f);
        }
        else {
            FixtureDef fixture = new FixtureDef();
            fixture.shape = polygonShape;
            fixture.density = density;
            fixture.friction = friction;
            body.createFixture(fixture);
        }

        return new Physics2dBody(body);
    }

    public void physics2dSetLinearDamping(Physics2dBody body, float damping) {
        body.body.setLinearDamping(damping);
    }

}
