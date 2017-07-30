using System;
using System.Collections.Generic;
using System.Linq;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.gui {
    // changes selection of GUI elements based on mouse events
    class SelectionInteraction {
        public SelectionInteraction(SelectionTracker selectionTracker, GuiElements guiElements) {
            this.selectionTracker = selectionTracker;
            this.positionChecker = new GuiElementMousePositionChecker(guiElements);
        }

        // called from outside when the mouse button was released
        public void eventMouseReleased(SpatialVectorDouble mousePosition) {
            IEnumerable<GuiElement> guiElementsUnderMousePositionAsEnumerable = 
                positionChecker.getElementsWhichOverlapTheMousePosition(mousePosition);
            IList<GuiElement> guiElementsUnderMousePosition = new List<GuiElement>(guiElementsUnderMousePositionAsEnumerable);

            if(guiElementsUnderMousePosition.Count > 0) {
                // change the selection

                // we just take first GuiElement in case of multiple
                // NOTE< could be a problem if there are multiple elements under the mouse position, we let it this way for now >
                GuiElement elementUnderMousePosition = guiElementsUnderMousePosition[0];

                // delesection
                trySendDeselectionToElementIfNotSameElement(elementUnderMousePosition);

                // selection
                if(!isSameAsAlreadySelectedElement(elementUnderMousePosition)) {
                    changeSelectionTo(elementUnderMousePosition);
                }
            }
            
        }

        void changeSelectionTo(GuiElement element) {
            selectionTracker.selectedElement = element;
            sendSelectionEventTo(element);
        }

        bool isSameAsAlreadySelectedElement(GuiElement element) {
            // is not the same if no element is selected
            if (!selectionTracker.isAnyGuiElementSelected) {
                return false;
            }
            
            return element == selectionTracker.selectedElement;
        }

        void trySendDeselectionToElementIfNotSameElement(GuiElement element) {
            // if no element is/was selected we can't send it an deselection event
            if( !selectionTracker.isAnyGuiElementSelected ) {
                return;
            }

            // if it is the same gui element sending it a deselection event is useless
            if (isSameAsAlreadySelectedElement(element)) {
                return;
            }

            // all fine, send delesection event
            sendDelesectionEventTo(selectionTracker.selectedElement);
        }

        void sendSelectionEventTo(GuiElement element) {
            throw new NotImplementedException(); // TODO
        }

        void sendDelesectionEventTo(GuiElement element) {
            throw new NotImplementedException(); // TODO
        }

        SelectionTracker selectionTracker;
        GuiElementMousePositionChecker positionChecker;
    }
}
