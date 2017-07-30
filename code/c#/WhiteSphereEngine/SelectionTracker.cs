using System;
using System.Diagnostics;

namespace WhiteSphereEngine.gui {
    // keeps track of which GUI element is currently selected
    public class SelectionTracker {
        public bool isAnyGuiElementSelected {
            get {
                return privateSelectedElement != null;
            }
        }

        public GuiElement selectedElement {
            get {
                if(privateSelectedElement == null) {
                    throw new Exception("Result not defined for no selected GUI Element!");
                }
                return privateSelectedElement;
            }

            set {
                Trace.Assert(value != null);
                privateSelectedElement = value;
            }
        }
        
        public void resetSelection() {
            privateSelectedElement = null;
        }

        GuiElement privateSelectedElement;
    }
}
