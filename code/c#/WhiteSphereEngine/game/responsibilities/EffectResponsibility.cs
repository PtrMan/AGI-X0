using System.Collections.Generic;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.game.responsibilities {
    // central class to manage attached effects for physics objects
    // effects are special attributes or 
    // examples for effects
    // * explosion
    // * alignment(rotation) of object to controlled direction
    // etc
    public class EffectResponsibility {
        public IDictionary<ulong, IList<Effect>> physicsObjectIdToEffects = new Dictionary<ulong, IList<Effect>>();
    }

    public class Effect {
        private Effect(EnumType type) {
            this.type = type;
        }

        public EnumType type;

        public enum EnumType {
            EXPLOSION,
            // ALIGNMENT < TODO >
        }

        public SpatialVectorDouble localPosition; // object local position of effect, can be null

        public static Effect makeExplosion(SpatialVectorDouble localPosition) {
            Effect created = new Effect(EnumType.EXPLOSION);
            created.localPosition = localPosition;
            return created;
        }
    }
}
