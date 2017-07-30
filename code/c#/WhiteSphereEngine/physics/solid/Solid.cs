//using System;
//using System.Collections.Generic;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.physics.solid {
    // platonic solid made out of a composition of Isotopes
    public class Solid {
        private Solid(EnumShapeType shapeType, Composition composition, SpatialVectorDouble size) {
            this.privateShapeType = shapeType;
            this.size = size;
            this.composition = composition;
        }

        public enum EnumShapeType {
            BOX
        }

        EnumShapeType privateShapeType;

        public EnumShapeType shapeType {
            get {
                return privateShapeType;
            }
        }

        public Composition composition; // what is the solid made out of

        public SpatialVectorDouble size;

        public static Solid makeBox(Composition composition, SpatialVectorDouble size) {
            return new Solid(EnumShapeType.BOX, composition, size);
        } 
    }
}
