using System.Collections.Generic;

using WhiteSphereEngine.input;

namespace WhiteSphereEngine.subsystems.gui {
    // implementations can react to the keyboard
    public interface IReactingToKeyboard {
        ReactingToKeyboard reactingToKeyboard {
            get;
        }
    }
 

    // used to decouple the details of the mous handling without requiring a class
    // the interface has an member hlding this which simplifies the design
    public class ReactingToKeyboard {
        public void addHandler(ReactingToKeyboardTypes.KeyboardEventDelegateType handler) {
            onKeyboardEventDelegates.Add(handler);
        }

        public void removeHandler(ReactingToKeyboardTypes.KeyboardEventDelegateType handler) {
            onKeyboardEventDelegates.Remove(handler);
        }

        internal void propagateKeyboardEvent(KeyboardEvent @event) {
            // propagate click event to handler delegates
            foreach (var iHandler in onKeyboardEventDelegates) {
                iHandler(@event);
            }
        }

        internal IList<ReactingToKeyboardTypes.KeyboardEventDelegateType> onKeyboardEventDelegates = new List<ReactingToKeyboardTypes.KeyboardEventDelegateType>();
    }

    public class ReactingToKeyboardTypes {
        public delegate void KeyboardEventDelegateType(KeyboardEvent @event);
    }

    // keyboard event
    public class KeyboardEvent {
        public bool isAlphaNumerical;
        public char alphaNumerical;

        public bool isControl;
        public KeyCodes.EnumKeyCode control;

        public enum EnumStatechange {
            OFF_ON, // button down
            ON_OFF, // button up
        }

    }
}
