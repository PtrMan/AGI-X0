using System.Collections.Generic;
using System.Linq;

namespace WhiteSphereEngine.physics.solid {
    public class Isotope {
        public string name;

        public Isotope(string name) {
            this.name = name;
        }
    }

    public class CompositionFraction {
        public Isotope isotope;
        public double mass; // in kilogram

        public CompositionFraction(Isotope isotope, double mass) {
            this.isotope = isotope;
            this.mass = mass;
        }
    }

    public class Composition {
        public Composition(IList<CompositionFraction> fractions) {
            this.fractions = fractions;
        }

        public IList<CompositionFraction> fractions = new List<CompositionFraction>();

        public double mass {
            get {
                return fractions.Select(v => v.mass).Sum();
            }
        }

        public Composition getPartByRatio(double ratio) {
            IList<CompositionFraction> resultFractions = fractions.Select(v => new CompositionFraction(v.isotope, v.mass * ratio)).ToList();
            return new Composition(resultFractions);
        }
    }
}
