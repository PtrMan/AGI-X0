using System.Collections.Generic;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.gui {
    // implementations can react to the mouse (clicking, mouseover, dragging)
    public interface IReactingToMouse {
        ReactingToMouse reactingToMouse {
            get;
        }

        // checks if the mouse is "over" the element
        bool isOverlappingMousePosition(SpatialVectorDouble mousePosition);
    }

    // used to decouple the details of the mous handling without requiring a class
    // the interface has an member hlding this which simplifies the design
    public class ReactingToMouse {
        public void addHandler(ClickableTypes.MouseEventDelegateType handler) {
            onClickEventDelegates.Add(handler);
        }

        public void removeHandler(ClickableTypes.MouseEventDelegateType handler) {
            onClickEventDelegates.Remove(handler);
        }
        
        protected void propagateClickEvent(SpatialVectorDouble mousePosition) {
            // propagate click event to handler delegates
            foreach (var iHandler in onClickEventDelegates) {
                iHandler(mousePosition);
            }
        }

        // only called by the GUI if the element was clicked
        internal virtual void wasClicked(SpatialVectorDouble mousePosition) {
            // by default we want to propagate the click to the handlers
            propagateClickEvent(mousePosition);
        }
        

        /* uncommented because TODO< integrte mouse hovering handling and effect >
        
            TODO< methods to add and remove handlers for mouse hovering >

        protected abstract void updateMouseHovering(SpatialVectorDouble mousePosition);

        protected IList<ClickableTypes.MouseEventDelegateType> hoveringEnterEventDelegates = new List<ClickableTypes.MouseEventDelegateType>();
        protected IList<ClickableTypes.MouseEventDelegateType> hoveringExitEventDelegates = new List<ClickableTypes.MouseEventDelegateType>();
             
        internal bool isMoveHovering; // true if the mouse is above this element and if the hover enter event was already triggered
         */

        internal IList<ClickableTypes.MouseEventDelegateType> onClickEventDelegates = new List<ClickableTypes.MouseEventDelegateType>();
    }

    // helper class to declare types
    public class ClickableTypes {
        public delegate void MouseEventDelegateType(SpatialVectorDouble mousePosition);
    }
}
