using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using WhiteSphereEngine.subsystems.gui;
using WhiteSphereEngine.math;
using WhiteSphereEngine.subsystems.renderer.common.font;

using Color = WhiteSphereEngine.subsystems.gui.Color;

namespace WhiteSphereEngine.subsystems.renderer.prototypeFormRenderer.gui {
    public class SoftwareGuiRenderer : IGuiRenderer {
        public SoftwareGuiRenderer(SoftwareGuiTextRenderer softwareTextRenderer, GuiElementDrawCommandCollection guiElementDrawCommands) {
            this.softwareTextRenderer = softwareTextRenderer;
            this.guiElementDrawCommands = guiElementDrawCommands;
        }

        public IGuiTextRenderer textRenderer => softwareTextRenderer;

        public GuiElementDrawCommandHandle createFillClosedLines(ClosedLoop outline, Color color, float Transparency, uint StartIndex) {
            ulong id;
            GuiElementDrawCommand guiElementDrawCommand = guiElementDrawCommands.createNewGuiElementDrawCommandAndIncrementId(GuiElementDrawCommand.EnumType.CLOSEDLOOPGEOMETRY, out id);
            guiElementDrawCommand.closedLoopGeometry = outline.points;

            return new GuiElementDrawCommandHandle(id);
        }

        public void draw(GuiElementDrawCommandHandle handle, SpatialVectorDouble position) {
            if( graphics == null ) {
                return;
            }

            if ( handle.id == 0xdeadf00d) { // pseudo special handle to ignore rendering
                return;
            }

            GuiElementDrawCommand drawCommand = guiElementDrawCommands.drawCommandsById[handle.id];

            if( drawCommand.type == GuiElementDrawCommand.EnumType.CLOSEDLOOPGEOMETRY )
            {
                Point[] points = new Point[drawCommand.closedLoopGeometry.Count + 1];
                for(int i = 0; i < drawCommand.closedLoopGeometry.Count; i++) {
                    // TODO< pull size of screen from context >
                    points[i] = new Point((int)((drawCommand.closedLoopGeometry[i].x + position.x) * 150.0f), (int)((drawCommand.closedLoopGeometry[i].y + position.y) * 150.0f));
                }
                points[points.Length-1] = new Point((int)((drawCommand.closedLoopGeometry[0].x + position.x) * 150.0f), (int)((drawCommand.closedLoopGeometry[0].y + position.y) * 150.0f));

                graphics.DrawLines(new Pen(Brushes.Black), points);
            }
            else if( drawCommand.type == GuiElementDrawCommand.EnumType.LINES ) {
                foreach( var iLine in drawCommand.lines) {
                    graphics.DrawLine(
                        new Pen(Brushes.Black),
                        (int)((iLine.a.x + position.x) * 150.0f), (int)((iLine.a.y + position.y) * 150.0f),
                        (int)((iLine.b.x + position.x) * 150.0f), (int)((iLine.b.y + position.y) * 150.0f)
                    );
                }
            }
            
        }

        public void releaseGuiElementHandle(GuiElementDrawCommandHandle handle) {
            Trace.Assert(guiElementDrawCommands.drawCommandsById.ContainsKey(handle.id)); // detect double free
            guiElementDrawCommands.drawCommandsById.Remove(handle.id);
        }
        
        GuiElementDrawCommandCollection guiElementDrawCommands;
        private SoftwareGuiTextRenderer softwareTextRenderer;
        public Graphics graphics;
    }
    

    // keeps track of the geometry of the stored GuiElementDrawCommandHandle elements by id  and it keeps track if the id
    public class GuiElementDrawCommandCollection {
        public IDictionary<ulong, GuiElementDrawCommand> drawCommandsById = new Dictionary<ulong, GuiElementDrawCommand>();

        public GuiElementDrawCommand createNewGuiElementDrawCommandAndIncrementId(GuiElementDrawCommand.EnumType type, out ulong id) {
            GuiElementDrawCommand created = new GuiElementDrawCommand(type);
            drawCommandsById[idCounter] = created;
            id = idCounter;
            idCounter++;
            return created;
        }

        ulong idCounter = 0;
    }

    public class GuiElementDrawCommand {
        public class Line {
            public Line(SpatialVectorDouble a, SpatialVectorDouble b) {
                this.a = a;
                this.b = b;
            }

            public readonly SpatialVectorDouble a, b;
        }

        public enum EnumType {
            CLOSEDLOOPGEOMETRY,
            LINES,
        }

        public readonly EnumType type;
        

        public static GuiElementDrawCommand makeClosedLoopGeometry(IList<SpatialVectorDouble> closedLoopGeometry) {
            GuiElementDrawCommand created = new GuiElementDrawCommand(EnumType.CLOSEDLOOPGEOMETRY);
            created.closedLoopGeometry = closedLoopGeometry;
            return created;
        }

        public static GuiElementDrawCommand makeLines(IList<Line> lines) {
            GuiElementDrawCommand created = new GuiElementDrawCommand(EnumType.LINES);
            created.lines = lines;
            return created;
        }


        public GuiElementDrawCommand(EnumType type) {
            this.type = type;
        }
        
        // for now just implemented for the closedLoop
        public IList<SpatialVectorDouble> closedLoopGeometry;

        public IList<Line> lines;
    }



    public class SoftwareGuiTextRenderer : IGuiTextRenderer {
        public SoftwareGuiTextRenderer(GuiElementDrawCommandCollection guiElementDrawCommands) {
            this.guiElementDrawCommands = guiElementDrawCommands;
        }

        public GuiElementDrawCommandHandle createDrawText(string text, SpatialVectorDouble signScale, Color color) {
            ulong id;
            GuiElementDrawCommand guiElementDrawCommand = guiElementDrawCommands.createNewGuiElementDrawCommandAndIncrementId(GuiElementDrawCommand.EnumType.LINES, out id);

            // create lineCommandInterpreter which fills the lines into the GuiElementDrawCommand
            //        lineRendererDriver which receives Hershey commands and translates it into line commands
            //        hersheyInterpreter which interprets hershey commands of the signs
            var lineCommandInterpreter = new SoftwareGuiTextRendererLineCommandInterpreter();
            LineRendererDriver lineRendererDriver = new LineRendererDriver(lineCommandInterpreter);

            // OPTIMIZATION EXTREMELY LOW< we could cache this object >
            HersheyTextRenderer textRenderer = new HersheyTextRenderer();
            textRenderer.lineRendererDriver = lineRendererDriver;
            

            lineCommandInterpreter.reset();

            lineRendererDriver.scale = signScale;
            
            textRenderer.loadHersheyCommands(@"D:\win10host\files\github\WhiteSphereEngine\resources\engine\graphics\font\hershey");

            textRenderer.renderString(text, signScale, new SpatialVectorDouble(new double[] { 0, 0 }));

            guiElementDrawCommand.lines = lineCommandInterpreter.lines; // transfer the line command 

            return new GuiElementDrawCommandHandle(id);
        }

        GuiElementDrawCommandCollection guiElementDrawCommands;
    }

    // renders strings with the hershey renderer
    public class HersheyTextRenderer {
        HersheyInterpreter hesheyInterpreter = new HersheyInterpreter();
        public LineRendererDriver lineRendererDriver;

        IDictionary<char, int> signToHersheyCommandIndex; // maps char's to file line indices, not the name of the commands!
        IList<string> hersheyCommandsOfLetters;

        public HersheyTextRenderer() {
            initSignLookupTable();
        }

        void initSignLookupTable() {
            signToHersheyCommandIndex = new Dictionary<char, int>();

            // fill lookup table

            for( char c = 'A'; c <= 'Z'; c++ ) {
                signToHersheyCommandIndex[c] = (60 + 30 - 1) + (int)(c - 'A');
            }

            for (char c = 'a'; c <= 'z'; c++) {
                signToHersheyCommandIndex[c] = (60 + 30 - 1) + 3 * 30 + (int)(c - 'a');
            }

            for (char c = '0'; c <= '9'; c++) {
                signToHersheyCommandIndex[c] = (60 + 30 - 1) + 3 * 30 + 90 - 3 + (int)(c - '0');
            }

            int i = 0;
            foreach( char c in ".,:;!?'\"°$/()|-+=i*iiii#&i") {
                if( c != 'i') { // do we not ignore it?
                    signToHersheyCommandIndex[c] = (60 + 30 - 1) + 3 * 30 + 90 - 3 + 10 + i;
                }

                i++;
            }

            // from another remote symbol table
            // means that the size maybe doesn't match up 100%, but this is fine
            signToHersheyCommandIndex['<'] = 695 + 12;
            signToHersheyCommandIndex['>'] = 695 + 13;
            signToHersheyCommandIndex['['] = 695 + 12 - 20;
            signToHersheyCommandIndex[']'] = 695 + 12 - 20 + 1;
        }

        public void loadHersheyCommands(string path) {
            hersheyCommandsOfLetters = new List<string>(File.ReadLines(path));
        }

        public void renderString(string @string, SpatialVectorDouble signScale, SpatialVectorDouble position) {
            Debug.Assert(lineRendererDriver != null);

            SpatialVectorDouble currentPosition = position.deepClone();



            // TODO< find optimal scale >
            const float rescalingFactor = 1.0f / 16.0f;
            lineRendererDriver.scale = signScale.scale(rescalingFactor); // normalize scale with scale of a typical sign

            int i = 0;

            foreach(char @char in @string) {
                lineRendererDriver.center = currentPosition;

                // continue with next sign if we can't look it up
                if ( !signToHersheyCommandIndex.ContainsKey(@char) ) {
                    continue;
                }

                int commandIndex = signToHersheyCommandIndex[@char];// (60+30-1) + 3*30; // signToHersheyCommandIndex[@char];
                string hersheyCommands = hersheyCommandsOfLetters[commandIndex];

                hesheyInterpreter.interpret(hersheyCommands, lineRendererDriver);

                float widthBeforeRescaling = lineRendererDriver.positionRight - lineRendererDriver.positionLeft; // width is the difference
                float width = (widthBeforeRescaling * rescalingFactor) * (float)signScale.x;

                currentPosition.x += width;

                i++;// for testing
            }
        }
    }

    // 
    public class SoftwareGuiTextRendererLineCommandInterpreter : ILineCommandInterpreter {
        public IList<GuiElementDrawCommand.Line> lines;
        
        public void reset() {
            lines = new List<GuiElementDrawCommand.Line>();
        }

        public void drawLine(float x1, float y1, float x2, float y2) {
            lines.Add(new GuiElementDrawCommand.Line(
                new SpatialVectorDouble(new double[]{x1, y1}),
                new SpatialVectorDouble(new double[]{x2, y2})
            ));
        }
    }
}
