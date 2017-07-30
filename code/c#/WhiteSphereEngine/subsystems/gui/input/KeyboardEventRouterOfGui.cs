using System;
using WhiteSphereEngine.input;

namespace WhiteSphereEngine.subsystems.gui.input {
    // routes keyboard events to the currently active gui element
    public class KeyboardEventRouterOfGui {
        public KeyboardEventRouterOfGui(SelectionTracker selectionTracker) {
            this.selectionTracker = selectionTracker;
        }

        public void keyUp(
            int alphanumericKeyCode,
            KeyCodes.EnumKeyType keyType,
            KeyCodes.EnumKeyCode? keyCode
        ) {

            tryPropagateTranslatedEventToSelectedElement(
                KeyboardEvent.EnumStatechange.ON_OFF,
                alphanumericKeyCode,
                keyType,
                keyCode);
        }

        public void keyDown(
            int alphanumericKeyCode,
            KeyCodes.EnumKeyType keyType,
            KeyCodes.EnumKeyCode? keyCode
        ) {

            tryPropagateTranslatedEventToSelectedElement(
                KeyboardEvent.EnumStatechange.OFF_ON,
                alphanumericKeyCode,
                keyType,
                keyCode);
        }

        void tryPropagateTranslatedEventToSelectedElement(
            KeyboardEvent.EnumStatechange statechange,
            int alphanumericKeyCode,
            KeyCodes.EnumKeyType keyType,
            KeyCodes.EnumKeyCode? keyCode
        ) {

            GuiElement selectedGuiElement;
            {
                bool wasRetrived;
                selectedGuiElement = tryRetriveSelectedGuiElement(out wasRetrived);
                if (!wasRetrived) {
                    return;
                }
            }

            if (!(selectedGuiElement is IReactingToKeyboard)) {
                return;
            }
            var receiver = ((IReactingToKeyboard)selectedGuiElement).reactingToKeyboard;
            receiver.propagateKeyboardEvent(translateKeycodesToKeyboardEvent(
                KeyboardEvent.EnumStatechange.OFF_ON,
                alphanumericKeyCode,
                keyType, keyCode));
        }

        private KeyboardEvent translateKeycodesToKeyboardEvent(KeyboardEvent.EnumStatechange stateChange, int alphanumericKeyCode, KeyCodes.EnumKeyType keyType, KeyCodes.EnumKeyCode? keyCode) {
            throw new NotImplementedException();
        }

        private GuiElement tryRetriveSelectedGuiElement(out bool wasRetrived) {
            wasRetrived = false;

            if (!selectionTracker.isAnyGuiElementSelected) {
                return null;
            }

            wasRetrived = true;
            return selectionTracker.selectedElement;
        }

        SelectionTracker selectionTracker;
    }
}
