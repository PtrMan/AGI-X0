using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.gui {
    // provides functionality for rendering GUI graphics
    public interface IGuiRenderer {
        IGuiTextRenderer textRenderer {
            get;
        }

        /** \brief creates an draw command to draw a filled closed loop
         *
         * \param Loop the Shape that should be drawn
         * \param FillColor ...
         * \param Transparency ...
         * \param StartIndex the index of the points where the Polygon-fan begins (IGNORED)
         */
        GuiElementDrawCommandHandle createFillClosedLines(
            ClosedLoop outline,
            Color color,
            float Transparency,
            uint StartIndex);

        void draw(GuiElementDrawCommandHandle handle, SpatialVectorDouble position);

        void releaseGuiElementHandle(GuiElementDrawCommandHandle handle);
    }

    public interface IGuiTextRenderer {
        /** \brief Draws Text
         *
         * \brief Text ...
         * \brief SignScale Scale of one Sign
         * \brief color ...
         */
        GuiElementDrawCommandHandle createDrawText(
            string Text,
            SpatialVectorDouble SignScale,
            Color color);
    }

    public class GuiElementDrawCommandHandle {
        internal ulong id;

        internal GuiElementDrawCommandHandle(ulong id) {
            this.id = id;
        }
    }
}
