using System.Collections.Generic;
using System.Linq;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.gui {
    // keeps track of the displayed elements
    public class GuiElements {
        public IList<GuiElement> elements = new List<GuiElement>();
    }

    // renders gui elements
    internal class GuiElementRenderer {
        public GuiElementRenderer(GuiElements guiElements, IGuiRenderer guiRenderer) {
            this.guiElements = guiElements;
            this.guiRenderer = guiRenderer;
        }

        public void render() {
            foreach(GuiElement iElement in guiElements.elements) {
                iElement.render(guiRenderer);
            }
        }

        GuiElements guiElements;
        IGuiRenderer guiRenderer;
    }

    // manages the mouse interaction
    internal class GuiElementMouseInteraction {
        public GuiElementMouseInteraction(
            GuiElementMousePositionChecker mousePositionChecker,
            SelectionInteraction selectionInteraction
        ) {

            this.mousePositionChecker = mousePositionChecker;
            this.selectionInteraction = selectionInteraction;
        }

        public void tick(
            SpatialVectorDouble mousePosition,
            MouseStateTracker.EnumMouseButtonState mouseButtonState
        ) {

            IEnumerable<GuiElement> elementsWhichOverlapMousePosition = 
                mousePositionChecker.getElementsWhichOverlapTheMousePosition(mousePosition);

            if ( mouseButtonState == MouseStateTracker.EnumMouseButtonState.WASDOWN ) {
                // update selection
                selectionInteraction.eventMouseReleased(mousePosition);

                // mousebutton was released, send message to all elements below the current mouse position

                foreach ( GuiElement iElement in elementsWhichOverlapMousePosition ) {
                    if(iElement is IReactingToMouse) {
                        ((IReactingToMouse)iElement).reactingToMouse.wasClicked(mousePosition);
                    }
                }
            }
        }

        GuiElementMousePositionChecker mousePositionChecker;
        SelectionInteraction selectionInteraction;
    }

    // finds which elements overlap currently the mouse position
    internal class GuiElementMousePositionChecker {
        private GuiElements guiElements;

        public GuiElementMousePositionChecker(GuiElements guiElements) {
            this.guiElements = guiElements;
        }

        public IEnumerable<GuiElement> getElementsWhichOverlapTheMousePosition(SpatialVectorDouble mousePosition) {
            return guiElements.elements.Where(v => v.isOverlappingMousePosition(mousePosition));
        }
    }
    
    // keeps track of the mousestate
    public class MouseStateTracker {
        public enum EnumMouseButtonState {
            ISUP, // not pressed
            WASDOWN, // down -> up
            WASUP, // up -> down
            ISDOWN, // down -> down
        }

        EnumMouseButtonState? cachedTransition;
        public bool isMouseDown;

        public EnumMouseButtonState tick() {
            if( cachedTransition != null ) {
                var result = cachedTransition.Value;
                cachedTransition = null;

                return result;
            }
            else {
                return isMouseDown ? EnumMouseButtonState.ISDOWN : EnumMouseButtonState.ISUP;
            }
        } 

        public void transitionWasDown() {
            setTransition(EnumMouseButtonState.WASDOWN);
            isMouseDown = false;
        }

        public void transitionWasUp() {
            setTransition(EnumMouseButtonState.WASUP);
            isMouseDown = true;
        }

        void setTransition(EnumMouseButtonState transition) {
            cachedTransition = transition;
        }
    }

    // keeps track of the mouse state, renders the elements and sends information to the elemnts for mouse interaction
    public class GuiContext {
        public SpatialVectorDouble mousePosition;

        public MouseStateTracker mouseStateTracker = new MouseStateTracker();

        // used to keep track of which element is in focus and receives keyboard input
        public SelectionTracker selectionTracker = new SelectionTracker();
        
        public GuiContext(IGuiRenderer guiRenderer) {
            {
                GuiElementMousePositionChecker guiElementMousePositionChecker = new GuiElementMousePositionChecker(guiElements);

                SelectionInteraction selectionInteraction = new SelectionInteraction(selectionTracker, guiElements);
                mouseInteraction = new GuiElementMouseInteraction(guiElementMousePositionChecker, selectionInteraction);
            }

            elementRenderer = new GuiElementRenderer(guiElements, guiRenderer);
        }

        public void addGuiElement(GuiElement element) {
            guiElements.elements.Add(element);
        }

        public void removeAndDisposeGuiElement(GuiElement element) {
            guiElements.elements.Remove(element);
            
            // TODO< dispose >
        }

        public void tick() {
            MouseStateTracker.EnumMouseButtonState mouseButtonState = mouseStateTracker.tick();
            mouseInteraction.tick(mousePosition, mouseButtonState);
        }

        public void render() {
            elementRenderer.render();
        }

        GuiElements guiElements = new GuiElements();
        GuiElementMouseInteraction mouseInteraction;
        GuiElementRenderer elementRenderer;
    }
}
