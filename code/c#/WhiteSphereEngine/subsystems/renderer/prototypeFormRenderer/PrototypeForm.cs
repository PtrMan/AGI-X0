using System;
using System.Drawing;

using System.Windows.Forms;

using WhiteSphereEngine.math;
using WhiteSphereEngine.input;
using WhiteSphereEngine.subsystems.renderer.prototypeFormRenderer.gui;
using WhiteSphereEngine.subsystems.gui;

using Color = System.Drawing.Color;

namespace WhiteSphereEngine.subsystems.renderer.prototypeFormRenderer {
    public partial class PrototypeForm : Form {
        public PrototypeForm() {
            InitializeComponent();

            // reduce annoying flickering
            DoubleBuffered = true;

            {
                GuiElementDrawCommandCollection drawCommandCollection = new GuiElementDrawCommandCollection();

                SoftwareGuiTextRenderer softwareGuiTextRenderer = new SoftwareGuiTextRenderer(drawCommandCollection);
                softwareGuiRenderer = new SoftwareGuiRenderer(softwareGuiTextRenderer, drawCommandCollection);
            }
            
            this.KeyPress +=
                new KeyPressEventHandler(keyPress);
        }

        private void keyPress(object sender, KeyPressEventArgs e) {
        }

        public SoftwareGuiRenderer softwareGuiRenderer;
        public SoftwareRenderer softwareRenderer;
        public KeyboardInputRouter keyboardInputRouter;
        public GuiContext guiContext;

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            softwareGuiRenderer.graphics = e.Graphics;

            softwareRenderer.graphics = e.Graphics;
            softwareRenderer.pen = new Pen(Color.FromArgb(0, 0, 0));
            softwareRenderer.viewSize = 300;

            Matrix projectionMatrix = SoftwareRendererUtilities.createProjectionMatrix(0.0, 0.0, -softwareRenderer.viewSize);
            softwareRenderer.modelViewProjection = projectionMatrix; // TODO< calculate model view projection

            softwareRenderer.render();

            guiContext.render();
        }

        private void PrototypeForm_KeyDown(object sender, KeyEventArgs e) {
            KeyCodes.EnumKeyType keyType;
            KeyCodes.EnumKeyCode? keyCode;
            translateToEngineKeycode(e.KeyCode, out keyCode, out keyType);
            keyboardInputRouter.routeKeyDown(e.KeyValue, keyType, keyCode);
        }

        private void PrototypeForm_KeyUp(object sender, KeyEventArgs e) {
            KeyCodes.EnumKeyType keyType;
            KeyCodes.EnumKeyCode? keyCode;
            translateToEngineKeycode(e.KeyCode, out keyCode, out keyType);
            keyboardInputRouter.routeKeyUp(e.KeyValue, keyType, keyCode);
        }

        private static void translateToEngineKeycode(Keys windowsKeycode, out KeyCodes.EnumKeyCode? keyCode, out KeyCodes.EnumKeyType keyType) {
            keyCode = null;
            keyType = KeyCodes.EnumKeyType.ALPHANUMERIC;

            switch(windowsKeycode) {
                case Keys.Left:
                keyCode = KeyCodes.EnumKeyCode.DIRECTION_LEFT;
                goto isControl;

                case Keys.Right:
                keyCode = KeyCodes.EnumKeyCode.DIRECTION_RIGHT;
                goto isControl;

                case Keys.Up:
                keyCode = KeyCodes.EnumKeyCode.DIRECTION_UP;
                goto isControl;

                case Keys.Down:
                keyCode = KeyCodes.EnumKeyCode.DIRECTION_DOWN;
                goto isControl;

                case Keys.OemBackslash:
                keyCode = KeyCodes.EnumKeyCode.BACKSLASH;
                goto isControl;

                isControl:
                keyType = KeyCodes.EnumKeyType.CONTROL;
                return;

                default:
                keyType = KeyCodes.EnumKeyType.ALPHANUMERIC;
                return;
            }
        }
        
    }
}
