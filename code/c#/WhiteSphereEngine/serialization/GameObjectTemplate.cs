using System.Collections.Generic;

using Newtonsoft.Json;

using WhiteSphereEngine.game.responsibilities;

namespace WhiteSphereEngine.serialization {
    // json serialization structures for game object templates, as general and extensible as possible

    public class GameObjectTemplate {
        public string mainMassShapeType; // "box"
        public double[] mainMassDimensions; // variable length, based on shapeType
        public double mainMass;

        public string meshPath;

        // physics colliders, are attached to main physics object
        public List<Collider> colliders;

        // solids used for calculating destructed debre
        public List<Solid> solids;

        public List<Effect> effects;

        public List<Thruster> thrusters;

        public List<SpecialAttribute> specialAttributes;

        public AttitudeAndAccelerationControlResponsibility.PidControlConfigurationOfEntity pidControllerConfiguration; // can be null

        public static GameObjectTemplate deserialize(string json) {
            return JsonConvert.DeserializeObject<GameObjectTemplate>(json);
        }

        public static string serialize(GameObjectTemplate data) {
            return JsonConvert.SerializeObject(data);
        }
    }

    public class Collider {
        public double[] size; // variable length, based on shapeType
        public double[] localPosition;
        public double[] localRotation;

        public string shapeType;
    }

    public class Solid {
        public double[] size; // variable length, based on shapeType

        // TODO< multiple fractions >

        public double fractionMass; // in kilogram
        public string fractionIsotopeName;

        //IList<CompositionFraction> solidCompositionFractions = new List<CompositionFraction>() { new CompositionFraction(new Isotope("Fe56"), massOfSolidInKilogram) };

        public string shapeType;
        public double[] localPosition;
        public double[] localRotation;
    }

    // effects are like "cheats" for objects to give objects special behaviours
    // examples
    // * chemicalExplosive
    // * directionalOrientation (physics object rotation is not influenced by impulse and depends on velocity)
    public class Effect {
        public string effectType;

        public double[] localPosition; // can be null
    }

    public class Thruster {
        public double[] locationPosition;
        public double[] direction;

        public double maximalForce;

        public string tag; // can be null
    }

    public class SpecialAttribute {
        public string type;
    }
}
