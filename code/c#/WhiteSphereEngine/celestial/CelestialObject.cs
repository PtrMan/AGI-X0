namespace WhiteSphereEngine.celestial {
    // informations about a celestial object without spatial informations
    // see (my old unreal engine 4 space game prototype)
    public class CelestialObject {
        public enum EnumType {
            NOTSET,

            STAR,
            // Blackhole
  
            GENERICCELESTIALBODY,
            // Planet
            // Asteroid
            // RoundMoon
            // Planetoid
  
            // TODO< system for procedurally blending betwen moon, asteroid and planet?, because they are all the same >
  
            //Blackhole UMETA(DisplayName="Blackhole"),
            //NeutronStar UMETA(DisplayName="NeutronStar"),
        }

        public CelestialObject(EnumType type) {
            this.type = type;
        }


        // TODO< age >
        // TODO< maxRadius (radius but same for irregular shapes >

        // is ignored/not valid if it is a GenericCelestialBody
        public float luminosityLogarithmic; // base 10, relative to sun
        
        public float surfaceTemperatureInKelvin = 0;

        // is ignored/not valid if it is a Star/Blackhole
        // TODO< add more properties about the atmosphere >
        public bool hasAtmosphere = false;

        public double mass = 1.0; // set to one kilo to avoid numeric problems

        public EnumType type;
    }
}
