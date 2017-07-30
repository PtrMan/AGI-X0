using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.gui {
    /** \brief Color
     *
     */
    public class Color {
        private SpatialVectorDouble vector = new SpatialVectorDouble(new double[3]);

	    /** \brief set color as RGB
	     *
	     * \param R ...
	     * \param G ...
	     * \param B ...
	     */
	    public void setRgb(float r, float g, float b) {
            vector[0] = r;
            vector[1] = g;
            vector[2] = b;
        }

        /** \brief return color as RGB
         *
         * \param R ...
         * \param G ...
         * \param B ...
         */
        public void getRgb(out float r, out float g, out float b) {
            r = (float)vector[0];
            g = (float)vector[1];
            b = (float)vector[2];
        }
    }
}
