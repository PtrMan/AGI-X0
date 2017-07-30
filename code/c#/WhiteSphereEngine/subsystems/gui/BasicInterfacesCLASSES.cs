using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.gui {
    public interface IGuiRenderable {
        void render(IGuiRenderer renderer);
    }
    
    public interface IHasText<TextType> {
        TextType text { set; }
        Color textColor { set; }
        SpatialVectorDouble textScale { set; }
    }
}
