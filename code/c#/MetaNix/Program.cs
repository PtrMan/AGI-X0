using System;
using System.Collections.Generic;

namespace MetaNix {
    class CellularAutomata {
        public static int calc(int rule, uint bits, int value) {
            int result = 0;

            { // optimization for first bit
                int read2 = (value >> ((int)bits-1)) & 1;
                int read1 = (value >> 0) & 1;
                int read0 = (value >> 1) & 1;

                result = applyCaRule(rule, read2, read1, read0);
            }

            { // optimization for last bit
                int read2 = (value >> 0) & 1;
                int read1 = (value >> ((int)bits)) & 1;
                int read0 = (value >> ((int)bits - 1)) & 1;

                result |= (applyCaRule(rule, read2, read1, read0) << (int)bits);
            }

            for(int i = 1; i < bits-1; i++) {
                int read2 = (value >> (i + 1)) & 1;
                int read1 = (value >> (i   )) & 1;
                int read0 = (value >> (i - 1)) & 1;

                int caResult = applyCaRule(rule, read2, read1, read0);

                result |= (caResult << i);
            }

            return result;
        }

        static int applyCaRule(int rule, int value2, int value1, int value0) {
            int value = value2 * 4 + value1 * 2 + value0 * 1;
            return (rule >> value) & 1;
        }
    }

    // reservoir computing with cellular automata
    class CaReservoir {
        public int rule;
        public uint bits;

        public class Trace {
            public int[] numbers;
        }

        public Trace[] traces;

        public void calcTraces() {
            for(int i = 0; i < traces.Length; i++) {
                calcTrace(traces[i]);
            }
        }

        void calcTrace(Trace trace) {
            for(int i = 0; i < trace.numbers.Length-1; i++) {
                // calculate next ca state
                trace.numbers[i + 1] = CellularAutomata.calc(rule, bits, trace.numbers[i]);
            }
        }
    }


    class Program {
        static void caReservoirTest() {
            CaReservoir caReservoir = new CaReservoir();
            caReservoir.rule = 110;
            caReservoir.bits = 5;

            caReservoir.traces = new CaReservoir.Trace[1];
            caReservoir.traces[0] = new CaReservoir.Trace();
            caReservoir.traces[0].numbers = new int[3];

            caReservoir.calcTraces();

        }

        /* uncommented because we have to overhaul this and unify it with the other functional representation
         * and it must be an unittest
        static void interpreterTest0() {
            PrimitiveInstructionInterpreter interpreter = new PrimitiveInstructionInterpreter();

            PrimitiveInterpretationContext interpreterContext = new PrimitiveInterpretationContext();
            interpreterContext.registers = new Register[3];
            interpreterContext.registers[0] = new Register();
            interpreterContext.registers[1] = new Register();
            interpreterContext.registers[2] = new Register();

            ImmutableNodeReferer rootnode0 = ImmutableNodeReferer.makeBranch();
            rootnode0.children = new ImmutableNodeReferer[4];

            rootnode0.children[0] = ImmutableNodeReferer.makeBranch();
            rootnode0.children[0].children = new ImmutableNodeReferer[4];
            rootnode0.children[0].children[0] = Node.makeInstr(Node.EnumType.INSTR_ADD_INT);
            rootnode0.children[0].children[1] = Node.makeAtomic(Variant.makeInt(0));
            rootnode0.children[0].children[2] = Node.makeAtomic(Variant.makeInt(1));
            rootnode0.children[0].children[3] = Node.makeAtomic(Variant.makeInt(2));


            rootnode0.children[1] = ImmutableNodeReferer.makeBranch();
            rootnode0.children[1].children = new ImmutableNodeReferer[2];
            rootnode0.children[1].children[0] = Node.makeInstr(Node.EnumType.INSTR_GOTO);
            rootnode0.children[1].children[1] = Node.makeAtomic(Variant.makeInt(0));




            interpreter.interpret(interpreterContext, rootnode0.children[0], new List<ImmutableNodeReferer>(), new List<string>());
        }
        **/


        static void parsedTest0() {
            Functional.ParseTreeElement parseTree = Functional.parseRecursive("(bAnd (shl value i) 1)");
            NodeRefererEntry node = TranslateFunctionalParseTree.translateRecursive(parseTree);

            int debugHere = 1;
        }

        
        static void parseInterpretSurrogateTest0() {
            FunctionalInterpreter functionalInterpreter = new FunctionalInterpreter();
            functionalInterpreter.tracer = new NullFunctionalInterpreterTracer();
            FunctionalInterpretationContext functionalInterpretationContext = new FunctionalInterpretationContext();


            dispatch.FunctionalInterpreterSurrogate interpreterSurrogate = new dispatch.FunctionalInterpreterSurrogate(functionalInterpreter, functionalInterpretationContext);

            // dispatcher which dispatches hidden function calls to the interpreter
            dispatch.SurrogateProvider surrogateProvider = new dispatch.SurrogateProvider();

            // dispatcher which can shadow calls (to the surrogate provider)
            dispatch.ShadowableHiddenDispatcher shadowableHiddenDispatcher = new dispatch.ShadowableHiddenDispatcher(surrogateProvider);
            

            // dispatcher which calls another dispatcher and a number of observers,
            // which is in this case our instrumentation observer
            dispatch.InstrumentationHiddenDispatcher instrHiddenDispatcher = new dispatch.InstrumentationHiddenDispatcher(shadowableHiddenDispatcher);
            dispatch.TimingAndCountHiddenDispatchObserver instrObserver = new dispatch.TimingAndCountHiddenDispatchObserver();
            instrObserver.resetCountersAndSetEnableTimingInstrumentation(true);
            instrHiddenDispatcher.dispatchObservers.Add(instrObserver);

            dispatch.PublicDispatcherByArguments publicDispatcherByArguments = new dispatch.PublicDispatcherByArguments(instrHiddenDispatcher);
            // dispatcher which accepts function names
            dispatch.PublicCallDispatcher callDispatcher = new dispatch.PublicCallDispatcher(publicDispatcherByArguments);
            
            Functional.ParseTreeElement parseTree = Functional.parseRecursive("(let [value 4 i 1   read2 (bAnd (shl value (+ i 1)) 1)] read2)");
            NodeRefererEntry rootNode = TranslateFunctionalParseTree.translateRecursive(parseTree);

            { // set descriptor to route all public function id's 0 to hidden function id 0
                dispatch.PublicDispatcherByArguments.FunctionDescriptor fnDescriptor = new dispatch.PublicDispatcherByArguments.FunctionDescriptor();
                fnDescriptor.wildcardHiddenFunctionId = dispatch.HiddenFunctionId.make(0);
                publicDispatcherByArguments.setFunctionDescriptor(dispatch.PublicFunctionId.make(0), fnDescriptor);
            }

            surrogateProvider.updateSurrogateByFunctionId(dispatch.HiddenFunctionId.make(0), interpreterSurrogate);
            interpreterSurrogate.updateFunctionBody(dispatch.HiddenFunctionId.make(0), rootNode.entry);
            interpreterSurrogate.updateParameterNames(dispatch.HiddenFunctionId.make(0), new List<string>());

            callDispatcher.setFunctionId("test", dispatch.PublicFunctionId.make(0));

            for(int i=0;i<100;i++) {
                callDispatcher.dispatchCallByFunctionName("test", new List<Variant>());
            }

            System.Console.WriteLine(instrObserver.getInstrumentation(dispatch.HiddenFunctionId.make(0)).calltimeMaxInNs);
            System.Console.WriteLine(instrObserver.getInstrumentation(dispatch.HiddenFunctionId.make(0)).calltimeMinInNs);
            System.Console.WriteLine(instrObserver.getInstrumentation(dispatch.HiddenFunctionId.make(0)).calltimeSumInNs);

            int debugMe = 0;
        }



        static void Main(string[] args) {
            parseInterpretSurrogateTest0();

            for (;;) {
                //double n = addX(5.0, 2.0, 3.0);

                //int x = (int)n;
            }
        }

        /*

        // http://www.wildml.com/2015/10/recurrent-neural-networks-tutorial-part-3-backpropagation-through-time-and-vanishing-gradients/
        void bptt(float[] y) {
            int T = y.Length;


            // results from forward propagation
            // TODO< actual forward propagation >
            Matrix s, o;



            Matrix dLdV;
            Matrix dLdW;

            Matrix[] delta_o = o.copy();
            // TODO subtract 1 from delta_o

            // for each output backwards
            for( int t = T-1; t >= 0; t-- ) {
                dLdV = dLdV.add(Matrix.outer(delta_o[t], s[t].transpose()));

                // initial delta calculation: dL/dz
                Matrix delta_t = this.V.transposed().dot(delta_o[t]) * (1.0f - (s[t].square()));

                // backpropagation through time (for at most this.bptt_truncate steps)
                for ( int bptt_step = t+1; bptt_step >= Math.Max(0, t-this.bptt_truncate); bptt_step-- ) {
                    // print "Backpropagation step t=%d bptt step=%d " % (t, bptt_step)

                    //Add to gradients at each previous step
                    dLdW = dLdW.add(Matrix.outer(delta_t, s[bptt_step - 1]));

                    /// i think i translated this correctly from python, but im not so sure
                    // python dLdU[:, x[bptt_step]] += delta_t
                    for( int ix = 0; ix < bptt_step; ix++ ) {
                        for( int iy = 0; iy; iy++ ) {
                            dLdU[ix, iy] += delta_t[ix, 0];
                        }
                    }

                    // Update delta for next step dL/dz at t-1
                    delta_t = this.W.transpose().dot(delta_t) * (1.0f - s[bptt_step - 1].square());
                }
                
            }
            
        }

        */

        int bptt_truncate; // how many steps are used for truncating the bptt
    }

    class Matrix {
        float[,] values;

        public Matrix(int sizeX, int sizeY) {
            values = new float[sizeX, sizeY];
        }

        public Matrix add(Matrix other) {
            if( values.GetLength(0) != other.values.GetLength(0) || values.GetLength(1) != other.values.GetLength(1) ) {
                throw new Exception();
            }

            Matrix result = new Matrix(values.GetLength(0), values.GetLength(1));
            for(int ix=0; ix < values.GetLength(0); ix++) {
                for (int iy = 0; iy < values.GetLength(1); iy++) {
                    result.values[ix, iy] = values[ix, iy] + other.values[ix, iy];
                }
            }
            
            return result;
        }

        // calculates the square of each number
        public Matrix square() {
            Matrix result = new Matrix(values.GetLength(0), values.GetLength(1));
            for(int ix=0;ix < values.GetLength(0);ix++) {
                for(int iy=0;iy < values.GetLength(1);iy++) {
                    result.values[ix, iy] = values[ix, iy] * values[ix, iy];
                }
            }
            return result;
        }

        // outer product
        public static Matrix outer(Matrix a, Matrix b) {
            if( a.values.GetLength(0) != b.values.GetLength(0) || a.values.GetLength(1) != 1 || b.values.GetLength(1) != 1 ) {
                throw new Exception();
            }

            int size = a.values.GetLength(0);

            Matrix result = new Matrix(size, size);

            for(int x = 0; x < size; x++) {
                for(int y = 0; y < size;y++ ) {
                    result.values[x, y] = a[y, 0] * b[x, 0];
                }
            }

            return result;
        }

        public float this[int x, int y] {
            get {
                return values[x, y];
            }
            set {
                values[x, y] = value;
            }
        }

        // slice by x
        public Matrix this[int x] {
            get {
                Matrix result = new Matrix(1, values.GetLength(1));
                for (int iy = 0; iy < values.GetLength(0); iy++ ) {
                    result.values[0, iy] = values[x, iy];
                }

                return result;
            }
            
            // set not implemented
        }
    }
}
