using WhiteSphereEngine.geometry;

namespace WhiteSphereEngine.subsystems.renderer {
    // used to abstract away the mesh decorations from the different renderers for the different APIs
    public abstract class AbstractDecoratedMesh {
        public VerticesWithAttributesAndIndex decoratedMesh;

        public AbstractDecoratedMesh(VerticesWithAttributesAndIndex mesh) {
		    this.decoratedMesh = mesh;
        }
    }

    // mesh with decoration for the specific API implementation
    public class DecoratedMesh<DecorationType> : AbstractDecoratedMesh {
	    public DecorationType decoration;

        public DecoratedMesh(VerticesWithAttributesAndIndex mesh) : base(mesh) {
        }
    }
}
