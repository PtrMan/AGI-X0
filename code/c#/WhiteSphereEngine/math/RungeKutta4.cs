namespace WhiteSphereEngine.math {
    // translation of https://github.com/PtrMan/WhiteSphereEngine/blob/master/src/d/math/RungeKutta4.d

    public struct RungeKutta4State {
        public SpatialVectorDouble x;      // position
        public SpatialVectorDouble v;      // velocity
    }
    
    // see http://gafferongames.com/game-physics/integration-basics/
    public class RungeKutta4 {
        // needs to be implemented by the physics simulation which uses the RungeKutta4 solver
        public interface IAcceleration {
            SpatialVectorDouble calculateAcceleration(ref RungeKutta4State state, float time);
        }

        public struct Derivative {
            public SpatialVectorDouble dx;      // dx/dt = velocity
            public SpatialVectorDouble dv;      // dv/dt = acceleration
        }

        Derivative evaluate(ref RungeKutta4State initial, float t, float dt, ref Derivative d) {
            RungeKutta4State state;
            state.x = initial.x + d.dx.scale(dt);
            state.v = initial.v + d.dv.scale(dt);

            Derivative output;
            output.dx = state.v;
            output.dv = acceleration.calculateAcceleration(ref state, t + dt);
            return output;
        }

        public void integrate(ref RungeKutta4State state, float t, float dt) {
            Derivative a, b, c, d, dummy;
            dummy.dx = new SpatialVectorDouble(new double[]{0.0, 0.0, 0.0 });
            dummy.dv = new SpatialVectorDouble(new double[] { 0.0, 0.0, 0.0 });

            a = evaluate(ref state, t, 0.0f, ref dummy);
            b = evaluate(ref state, t, dt * 0.5f, ref a);
            c = evaluate(ref state, t, dt * 0.5f, ref b);
            d = evaluate(ref state, t, dt, ref c);

            SpatialVectorDouble dxdt = (a.dx + (b.dx + c.dx).scale(2.0f) + d.dx).scale(1.0f / 6.0f);
            SpatialVectorDouble dvdt = (a.dv + (b.dv + c.dv).scale(2.0f) + d.dv).scale(1.0f / 6.0f);

            state.x = state.x + dxdt.scale(dt);
            state.v = state.v + dvdt.scale(dt);
        }

        public IAcceleration acceleration;
    }
}
