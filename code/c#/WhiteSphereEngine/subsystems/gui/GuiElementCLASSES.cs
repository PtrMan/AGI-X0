using System;
using System.Diagnostics;
using WhiteSphereEngine.math;

using WhiteSphereEngine.input;

using DirtyableDrawCommandHandle = 
    WhiteSphereEngine.subsystems.gui.DirtyableWithPayload<
        WhiteSphereEngine.subsystems.gui.GuiElementDrawCommandHandle
    >;

namespace WhiteSphereEngine.subsystems.gui {
    public abstract class GuiElement : IGuiRenderable {
        public SpatialVectorDouble position {
            set {
                protectedPosition = value;
                positionWasChanged();
            }
        }

        public SpatialVectorDouble size {
            set {
                protectedSize = value;
                sizeWasChanged();
            }
        }

        public abstract void render(IGuiRenderer renderer);

        protected abstract void positionWasChanged();
        protected abstract void sizeWasChanged();

        public abstract bool isOverlappingMousePosition(SpatialVectorDouble mousePosition);

        protected SpatialVectorDouble protectedSize, protectedPosition;
        
    }
    
    // A editable singleline GUI text field
    public class EditField : GuiElement, IHasText<string>, IReactingToKeyboard, IReactingToMouse {
        public EditField() {
            initReactingToKeyboard();
        }

        public ReactingToMouse reactingToMouse => protectedReactingToMouse;
        public ReactingToKeyboard reactingToKeyboard => protectedReactingToKeyboard;

        public float backgroundTransparency { set => privateBackgroundTransparency = value; }
        public Color backgroundColor { set => privateBackgroundColor = value; }
        public Color textColor { set => privateTextColor = value; }

        public SpatialVectorDouble textScale {
            set {
                privateTextScale = value;
                dirtyableTextHandle.markDirty();
            }
        }

        public string text {
            set {
                privateText = value;

                // update cursor
                if(cursorPosition != null) {
                    cursorPosition = System.Math.Max(0, System.Math.Min(cursorPosition.Value, value.Length));
                }

                dirtyableTextHandle.markDirty();
            }
        }

        public override void render(IGuiRenderer renderer) {
            checkIfOutlineIsDirtyAndReaquire(renderer);
            checkIfTextIsDirtyAndReaquire(renderer);

            renderer.draw(dirtyableOutlineHandle.payload, protectedPosition);
            renderer.draw(dirtyableTextHandle.payload, protectedPosition);
        }

        protected override void positionWasChanged() {
        }

        protected override void sizeWasChanged() {
            // when the size was changed we have to rebuild the outline
            dirtyableOutlineHandle.markDirty();
        }
        
        public override bool isOverlappingMousePosition(SpatialVectorDouble mousePosition) {
            return GuiMiscHelper.checkInBox(mousePosition, protectedPosition, protectedSize);
        }
        
        void checkIfOutlineIsDirtyAndReaquire(IGuiRenderer renderer) {
            ClosedLoop outline = calcOutline();
            GuiMiscHelper.checkIfResourceIsDirtyAndReaquire(
                () => renderer.createFillClosedLines(outline, privateBackgroundColor, privateBackgroundTransparency, 0),
                renderer, dirtyableOutlineHandle
            );
        }

        void checkIfTextIsDirtyAndReaquire(IGuiRenderer renderer) {
            Debug.Assert(privateTextScale != null);
            GuiMiscHelper.checkIfResourceIsDirtyAndReaquire(
                () => renderer.textRenderer.createDrawText(privateText, privateTextScale.Value, privateTextColor),
                renderer, dirtyableTextHandle
            );
        }


        ClosedLoop calcOutline() {
            float insetWidth = 0.05f;
            float insetHeight = 0.02f;
            return GuiStyleHelper.generateStandardOutline(insetWidth, insetHeight, protectedSize);
        }
        

        // sets up the keyboard handling
        void initReactingToKeyboard() {
            protectedReactingToKeyboard.addHandler(keyboardHandler);
        }

        // keyboard handler
        void keyboardHandler(KeyboardEvent @event) {
            if( @event.isControl ) {
                // ignore control if no text is selected
                if ( cursorPosition == null ) {
                    return;
                }

                switch(@event.control) {
                    case KeyCodes.EnumKeyCode.DIRECTION_LEFT:
                    cursorPosition = System.Math.Max(cursorPosition.Value - 1, 0);
                    break;

                    case KeyCodes.EnumKeyCode.DIRECTION_RIGHT:
                    cursorPosition = System.Math.Min(cursorPosition.Value + 1, privateText.Length);
                    break;

                }
            }
            else if( @event.isAlphaNumerical ) {
                if( cursorPosition == null ) {
                    cursorPosition = privateText.Length;
                }

                privateText.Insert(cursorPosition.Value, @event.alphaNumerical.ToString());
                dirtyableTextHandle.markDirty();

                cursorPosition++;
            }
        }

        SpatialVectorDouble? privateTextScale;

        float privateBackgroundTransparency;
        Color privateBackgroundColor, privateTextColor;
        string privateText;
        int? cursorPosition; // nullable because this element MAY not be active or the position is just not tracked 

        
        DirtyableDrawCommandHandle
            dirtyableTextHandle = new DirtyableDrawCommandHandle(),
            dirtyableOutlineHandle = new DirtyableDrawCommandHandle();

        protected ReactingToKeyboard protectedReactingToKeyboard = new ReactingToKeyboard();
        protected ReactingToMouse protectedReactingToMouse = new ReactingToMouse();
    }


    // A editable singleline GUI text field
    public class UneditableText : GuiElement, IHasText<string> {
        public UneditableText() {
        }

        public float backgroundTransparency { set => privateBackgroundTransparency = value; }
        public Color backgroundColor { set => privateBackgroundColor = value; }
        public Color textColor { set => privateTextColor = value; }

        public SpatialVectorDouble textScale {
            set {
                privateTextScale = value;
                dirtyableTextHandle.markDirty();
            }
        }

        public string text {
            set {
                privateText = value;
                dirtyableTextHandle.markDirty();
            }
        }

        public override void render(IGuiRenderer renderer) {
            checkIfTextIsDirtyAndReaquire(renderer);

            renderer.draw(dirtyableTextHandle.payload, protectedPosition);
        }

        protected override void positionWasChanged() {}
        protected override void sizeWasChanged() {}

        public override bool isOverlappingMousePosition(SpatialVectorDouble mousePosition) {
            return GuiMiscHelper.checkInBox(mousePosition, protectedPosition, protectedSize);
        }
        
        void checkIfTextIsDirtyAndReaquire(IGuiRenderer renderer) {
            Debug.Assert(privateTextScale != null);
            GuiMiscHelper.checkIfResourceIsDirtyAndReaquire(
                () => renderer.textRenderer.createDrawText(privateText, privateTextScale.Value, privateTextColor),
                renderer, dirtyableTextHandle
            );
        }

        
        SpatialVectorDouble? privateTextScale;

        float privateBackgroundTransparency;
        Color privateBackgroundColor, privateTextColor;
        string privateText;

        DirtyableDrawCommandHandle
            dirtyableTextHandle = new DirtyableDrawCommandHandle();
        
    }


    /** \brief Interface for a GUI Button
     *
     */
    public abstract class AbstractButton : GuiElement, IHasText<string>, IReactingToMouse {
        public abstract float backgroundTransparency { set; }
        public abstract Color backgroundColor { set; }
        public abstract string text { set; }
        public abstract Color textColor { set; }
        public abstract SpatialVectorDouble textScale { set; }

        public ReactingToMouse reactingToMouse => protectedReactingToMouse;

        protected ReactingToMouse protectedReactingToMouse = new ReactingToMouse();
    }
    
    public class Button : AbstractButton {
        public override string text {
            set {
                privateText = value;
                dirtyableTextHandle.markDirty();
            }
        }

        public override SpatialVectorDouble textScale {
            set {
                privateTextScale = value;
                dirtyableTextHandle.markDirty();
            }
        }

        public override float backgroundTransparency { set => privateBackgroundTransparency = value; }
        public override Color backgroundColor { set => privateBackgroundColor = value; }
        public override Color textColor { set => privateTextColor = value; }

        public override void render(IGuiRenderer renderer) {
            checkIfOutlineIsDirtyAndReaquire(renderer);
            checkIfTextIsDirtyAndReaquire(renderer);

            renderer.draw(dirtyableOutlineHandle.payload, protectedPosition);
            renderer.draw(dirtyableTextHandle.payload, protectedPosition);
        }

        void checkIfOutlineIsDirtyAndReaquire(IGuiRenderer renderer) {
            ClosedLoop outline = calcOutline();
            GuiMiscHelper.checkIfResourceIsDirtyAndReaquire(
                () => renderer.createFillClosedLines(outline, privateBackgroundColor, privateBackgroundTransparency, 0),
                renderer, dirtyableOutlineHandle
            );
        }

        void checkIfTextIsDirtyAndReaquire(IGuiRenderer renderer) {
            Debug.Assert(privateTextScale != null);
            GuiMiscHelper.checkIfResourceIsDirtyAndReaquire(
                () => renderer.textRenderer.createDrawText(privateText, privateTextScale.Value, privateTextColor),
                renderer, dirtyableTextHandle
            );
        }

        ClosedLoop calcOutline() {
            float insetWidth = 0.05f;
            float insetHeight = 0.02f;
            return GuiStyleHelper.generateStandardOutline(insetWidth, insetHeight, protectedSize);
        }

        public override bool isOverlappingMousePosition(SpatialVectorDouble mousePosition) {
            return GuiMiscHelper.checkInBox(mousePosition, protectedPosition, protectedSize);
        }

        protected override void positionWasChanged() {
        }

        protected override void sizeWasChanged() {
            // when the size was changed we have to rebuild the outline
            dirtyableOutlineHandle.markDirty();
        }

        SpatialVectorDouble? privateTextScale;

        Color privateBackgroundColor, privateTextColor;
        float privateBackgroundTransparency = 0.0f;
        string privateText;

        DirtyableDrawCommandHandle
            dirtyableTextHandle = new DirtyableDrawCommandHandle(),
            dirtyableOutlineHandle = new DirtyableDrawCommandHandle();
    }

    // helper methods for the style
    public class GuiStyleHelper {
        public static ClosedLoop generateStandardOutline(
            float insetWidth,
            float insetHeight,
            SpatialVectorDouble size
        ) {

            ClosedLoop outline = new ClosedLoop();
            outline.points.Add(new SpatialVectorDouble(new double[] { 0.0f, 0.0f }));
            outline.points.Add(new SpatialVectorDouble(new double[] { size.x, 0.0f }));
            outline.points.Add(new SpatialVectorDouble(new double[] { size.x, size.y - insetHeight }));
            outline.points.Add(new SpatialVectorDouble(new double[] { size.x - insetWidth, size.y }));
            outline.points.Add(new SpatialVectorDouble(new double[] { 0.0f, size.y }));
            return outline;
        }
    }


    // all not so related functionality to simplify the GUI code
    public class GuiMiscHelper {
        // checks if a resource is dirty, free's it if necessary and reaquires it
        // calls reaquireHandler if it is dirty and has to be recreated
        public static void checkIfResourceIsDirtyAndReaquire(
            ReaquireDrawCommandHandleDelegateType reaquireHandler,
            IGuiRenderer renderer,
            DirtyableDrawCommandHandle dirtyableDrawCommandHandle
        ) {

            if (!dirtyableDrawCommandHandle.isDirty) {
                return;
            }

            if (dirtyableDrawCommandHandle.payload != null) {
                renderer.releaseGuiElementHandle(dirtyableDrawCommandHandle.payload);
            }

            dirtyableDrawCommandHandle.payload = reaquireHandler();
            dirtyableDrawCommandHandle.undirtyfy();
        }
        
        public static bool checkInBox(
            SpatialVectorDouble checkedPosition,
            SpatialVectorDouble startPosition,
            SpatialVectorDouble size
        ) {

            return
                checkedPosition.x > startPosition.x && checkedPosition.x < startPosition.x + size.x &&
                checkedPosition.y > startPosition.y && checkedPosition.y < startPosition.y + size.y;
        }

        public delegate GuiElementDrawCommandHandle ReaquireDrawCommandHandleDelegateType();
    }
    
    // payload can be marked as dirty
    public class DirtyableWithPayload<PayloadType> {
        public bool isDirty {
            get {
                return privateIsDirty;
            }
        }

        public void markDirty() {
            privateIsDirty = true;
        }

        public void undirtyfy() {
            privateIsDirty = false;
        }

        public PayloadType payload;

        bool privateIsDirty = true; // set to true for forcing to rebake
    }
}
