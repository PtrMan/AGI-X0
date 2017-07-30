using System;
using System.Diagnostics;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.renderer.common.font {
    public interface IHersheyCommandDriver {
        void setPositions(float left, float right);
        void raise();
        void command(HersheyInterpreter.EnumCommandType command, float x, float y);
    }

    public sealed class HersheyInterpreter {
        public enum EnumCommandType {
            MOVE,
            DRAW,
        }


        // interpreter for hershey fonts
        // see http://paulbourke.net/dataformats/hershey/
        // sends commands to an driver which can do the drawing
        public void interpret(string commands, IHersheyCommandDriver driver) {
            // translate a char to a value it encodes

            Func<char, float> translate = (char p) => {
                return (float)(p - 'R');
            };

            Func<uint, float> readAndTranslate = (uint index) => {
                return translate(commands[(int)index]);
            };

            Func<uint, bool> isRaiseCommand = (uint index) => {
                return commands[(int)index] == ' ' && commands[(int)index + 1] == 'R';
            };
            
            float leftHandPosition = readAndTranslate(8);
            float rightHandPosition = readAndTranslate(9);

            driver.setPositions(leftHandPosition, rightHandPosition);

            EnumCommandType commandType = EnumCommandType.MOVE;

            for (uint i = 0; ; i++) {
                uint i2 = 10 + i * 2;


                Debug.Assert(i2 <= commands.Length);
                if (i2 == commands.Length) {
                    break;
                }

                if (isRaiseCommand(i2)) {
                    driver.raise();
                    commandType = EnumCommandType.MOVE;
                }
                else {
                    float x, y;

                    // C# implementation of readAndTranslateCoordinate(i2, /*out*/ x, /*out*/ y);
                    {
                        x = readAndTranslate(i2);
                        y = readAndTranslate(i2 + 1);
                    }

                    driver.command(commandType, x, y);
                    commandType = EnumCommandType.DRAW;
                }
            }
        }
    }
    

    // implementation just has to draw the commands
    public interface ILineCommandInterpreter {
        void drawLine(float x1, float y1, float x2, float y2);
    }

    // interprets Hershey commands and translates them to line commands
    public class LineRendererDriver : IHersheyCommandDriver {
        public float positionLeft, positionRight;
        SpatialVectorDouble lastPosition;

        public SpatialVectorDouble center = new SpatialVectorDouble(new double[]{0,0 }), scale;

        public ILineCommandInterpreter lineCommandInterpreter;

        public LineRendererDriver(ILineCommandInterpreter lineCommandInterpreter) {
            this.lineCommandInterpreter = lineCommandInterpreter;
        }
        

        public void setPositions(float left, float right) {
            positionLeft = left;
            positionRight = right;
        }

        public void raise() {
            // we just ignore it
        }

        public void command(HersheyInterpreter.EnumCommandType command, float x, float y) {
            SpatialVectorDouble currentPosition = new SpatialVectorDouble(new double[]{x, y }).componentMultiplication(scale) + center;

            if (command == HersheyInterpreter.EnumCommandType.MOVE) {
                lastPosition = currentPosition;
                return;
            }

            lineCommandInterpreter.drawLine((float)lastPosition.x, (float)lastPosition.y, (float)currentPosition.x, (float)currentPosition.y);
            lastPosition = currentPosition;
        }
    }
}
