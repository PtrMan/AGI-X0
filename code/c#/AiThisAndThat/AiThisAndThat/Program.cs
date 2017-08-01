using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;



using execution.functional;
using treeTransducer;
using execution.translateFunctionalToLowlevel;
using AiThisAndThat.prototyping;







// paper [1] "Controlling Procedural Modeling Programs with Stochastically-Ordered Sequential Monte Carlo"
//           http://stanford.edu/~dritchie/procmod-smc.pdf

abstract class AbstractParticle {
    public float weight;
}

// particle filter  also known as  sequential monte carlo
abstract class Smc<ParticleType> where ParticleType : AbstractParticle {
    public Distribution<ParticleType> distribution;
    public uint numberOfParticles;

    protected Random rand = new Random();

    // does a simulation of the particles for whatever purpose
    protected abstract void simulate();

    protected abstract void weightParticle(ParticleType particle);

    // rates all particles of the distribution
    protected void weight() { // final
        foreach (ParticleType iterationParticle in distribution.particles) {
            weightParticle(iterationParticle);
        }
    }


    public void singleIteration() { // final
        simulate();
        weight();
        resample();
    }

    // resamples based on choosing the particles based on the relative weight to the other particles
    private void resample() { // final
        float sum = 0.0f;
        foreach(ParticleType iterationParticle in distribution.particles) {
            sum += iterationParticle.weight;
        }

        List<ParticleType> newParticles = new List<ParticleType>();

        for (int i = 0; i < numberOfParticles; i++) {
            float chosenRandomValue = randomInclusive(0.0f, sum); // all inclusive

            float accumulator = 0.0f;
            bool chosen = false;
            foreach (ParticleType iterationParticle in distribution.particles) {
                accumulator += iterationParticle.weight;
                if (accumulator >= chosenRandomValue) { // equal seems important
                    newParticles.Add(iterationParticle);
                    chosen = true;
                    break;
                }
            }
            Debug.Assert(chosen, "Resample logic is wrong"); // should never happen
        }

        distribution.particles = newParticles;
    }

    protected float randomInclusive(float start, float end) {
        return start + (float)rand.NextDouble() * (end - start);
    }
}


abstract class Sosmc: Smc<Particle2> {
    // uses interpreter and select random action
    protected abstract void simulateParticle(Particle2 particle);

    public delegate float ScoreFunctionType(List<DistributionElement> x);
    protected ScoreFunctionType scoreFunction;

    // inherited from Smc
    protected override  void simulate() { // final
        foreach (Particle2 iterationParticle in distribution.particles) {
            simulateParticle(iterationParticle);
        }
    }

    // inherited from Smc
    protected override void weightParticle(Particle2 particle) { // final pure
        List<DistributionElement> traceElementsOfExecutionTrace = new List<DistributionElement>();
        Debug.Assert(false, "TODO");
        particle.distributionElementOfParticle.buildExecutionTrace(traceElementsOfExecutionTrace, /* TODO */ null, 0);
        
        particle.rating = scoreFunction(traceElementsOfExecutionTrace);
    }

}


class TestSosmc : Sosmc {
    public TestSosmc() {
        scoreFunction = specificScoreFunction;
    }

    protected override void simulateParticle(Particle2 particle) {
        // TODO< random choice to advance particle >
        // TODO TODO TODO TODO TODO
        Debug.Assert(false, "TODO");
    }

    protected float specificScoreFunction(List<DistributionElement> x) {
        // TODO TODO TODO TODO TODO
        Debug.Assert(false, "TODO");
        return 0.0f;
    }
}








class Particle2 : AbstractParticle {
    public DistributionElement distributionElementOfParticle;

    public float rating;
}

class Distribution<ParticleType> {
    public DistributionElement entry;

    public List<ParticleType> particles;
}

// new part

// a distribution element can have many childrens, depending on the choices which were made for the branches
class DistributionElement {
    public List<ChoiceWithDistributionElement> childrens;

    public DistributionElement parent = null;

    public void buildExecutionTrace(List<DistributionElement> resultTraceElements, List<uint> proceduralRandomChoice, uint proceduralRandomChoiceIndex) {
        Debug.Assert((int)proceduralRandomChoiceIndex <= proceduralRandomChoice.Count - 1);

        if (proceduralRandomChoiceIndex == proceduralRandomChoice.Count - 1) {
            return;
        }

        uint currentProceduralRandomChoice = proceduralRandomChoice[(int)proceduralRandomChoiceIndex];
        DistributionElement distributionElementToAdd = searchChildrenByProceduralRandomChoice(currentProceduralRandomChoice);
        resultTraceElements.Add(distributionElementToAdd);
    }

    protected DistributionElement searchChildrenByProceduralRandomChoice(uint proceduralRandomChoice) {
        foreach(ChoiceWithDistributionElement iterationChildrenChoiceWithDistributionElement in childrens) {
            if(
                iterationChildrenChoiceWithDistributionElement.choice.type == Choice.EnumType.PROCEDURAL &&
                iterationChildrenChoiceWithDistributionElement.choice.proceduralRandomChoice == proceduralRandomChoice
            ) {
                return iterationChildrenChoiceWithDistributionElement.element;
            }
        }

        // TODO< throw something >
        Debug.Assert(false);

        return null;
    }
}

class Choice {
    public enum EnumType {
        PROCEDURAL,
        ORDERING
    }

    public EnumType type;

    public uint proceduralRandomChoice; // TODO 24.06.2016 < custom type? >
}

class ChoiceWithDistributionElement {
    public DistributionElement element;
    public Choice choice; // describe as p_n (is just one element)
}













/*
class CompressionTableEntry {
    List<CompressionTableEntry> childrens = new List<CompressionTableEntry>();


}*/


struct Vector2d<Type> {
    public Vector2d(Type x, Type y) {
        this.x = x;
        this.y = y;
    }

    public Type x, y;
}

class Array2d<Type> {
    public Array2d(Vector2d<uint> size) {
        array = new Type[size.x, size.y];
        cachedSize = size;
    }

    public Type read(Vector2d<uint> position) {
        return array[position.x, position.y];
    }

    public void write(Vector2d<uint> position, Type value) {
        array[position.x, position.y] = value;
    }

    public Vector2d<uint> getSize() {
        return cachedSize;
    }

    protected Type[,] array;
    protected Vector2d<uint> cachedSize;
}

class Map2d<Type> {
    public Map2d(Vector2d<uint> size) {
        array2d = new Array2d<Type>(size);
    }

    public Type read(Vector2d<uint> position) {
        return array2d.read(position);
    }

    public void write(Vector2d<uint> position, Type value) {
        array2d.write(position, value);
    }

    public Vector2d<uint> getSize() {
        return array2d.getSize();
    }

    private Array2d<Type> array2d;
}



class ImageDrawer<Type> {
    public ImageDrawer() {
        uint radiusToCache = 20;
        drawAndCacheCircles(radiusToCache);
    }
    

    public void drawHorizontalLine(Map2d<Type> target, Vector2d<uint> start, uint width, Type value) {
        for (uint i = start.x; i < Math.Min(start.x + width, target.getSize().x-1); i++) {
            target.write(new Vector2d<uint>(i, start.y), value);
        }
    }

    public void drawCircle(Map2d<Type> target, Vector2d<uint> position, uint radius, Type value) {
        bool cachedCircleDrawn = drawCircleIfCached(target, position, radius, value);

        if( !cachedCircleDrawn ) {
            // TODO< draw circle using slow algorithm, bresenham >
            Debug.Assert(false, "TODO");
        }
    }

    private bool drawCircleIfCached(Map2d<Type> target, Vector2d<uint> center, uint radius, Type value) {
        if(radius == 0) {
            return false;
        }

        int index = (int)(radius - cachedCirclesFirstRadius);

        if( index >= cachedCircles.Count) {
            return false;
        }
        
        drawCachedCircle(target, index, center, value);
        return true;
    }

    private void drawCachedCircle(Map2d<Type> target, int index, Vector2d<uint> center, Type value) {
        // TODO< speed up with special case checks >

        Map2d<bool> bitmapToDraw = cachedCircles[index];

        for (int bitmapY = 0; bitmapY < bitmapToDraw.getSize().y; bitmapY++ ) {
            for (int bitmapX = 0; bitmapX < bitmapToDraw.getSize().x; bitmapX++) {
                int absoluteX = bitmapX + (int)center.x;
                int absoluteY = bitmapY + (int)center.y;
                
                if( absoluteX < 0 || absoluteX >= target.getSize().x || absoluteY < 0 || absoluteY >= target.getSize().y ) {
                    continue;
                }

                if( !bitmapToDraw.read(new Vector2d<uint>((uint)bitmapX, (uint)bitmapY))) {
                    continue;
                }

                target.write(new Vector2d<uint>((uint)absoluteX, (uint)absoluteY), value);
            }
        }
    }

    private void drawAndCacheCircles(uint radiusToCache) {
        for (uint i = 0; i < radiusToCache; i++) {
            cachedCircles.Add(drawCircleSlow(cachedCirclesFirstRadius + i));
        }
    }

    // slow and naive implementation
    private static Map2d<bool> drawCircleSlow(uint radius) {
        Map2d<bool> result = new Map2d<bool>(new Vector2d<uint>(radius * 2 + 1, radius * 2 + 1));

        Vector2d<float> center = new Vector2d<float>((float)radius, (float)radius);

        for( uint y = 0; y < radius*2+1; y++ ) {
            for( uint x = 0; x < radius*2+1; x++ ) {
                if(distance(new Vector2d<float>((float)x, (float)y), center ) <= (float)radius ) {
                    result.write(new Vector2d<uint>(x,y), true);
                }
            }
        }

        return result;
    }

    private static float distance(Vector2d<float> a, Vector2d<float> b) {
        float xDiff = a.x - b.x, yDiff = a.y - b.y;
        return (float)Math.Sqrt(xDiff*xDiff + yDiff*yDiff);
    }

    private List<Map2d<bool>> cachedCircles = new List<Map2d<bool>>();

    private static uint cachedCirclesFirstRadius = 1;
}

// (an encoder is a function which takes an image and produces an compact (lossy) description)

// <count how many bits are required for encoding the image with the old encoder>
// <count how many bits are required for encoding the image with the new encoder>
// <rate the lowering of the bits against the error difference between the image and its reconstruction>
// <replace the better encoder witht the new one if it fares better>

class TestEncoder {
    public struct Candidate {
        public enum EnumType {
            CIRCLE,
            LINE_HORIZONTAL,
        }

        public EnumType type;

        public Vector2d<uint> startPosition;
        public uint attribute; // radius, width
        public float distance;
    }

    // TODO< refactor the code so that the api doesn't leak out Candidate >
    public Candidate encode(Map2d<float> image)
    {
        List<Candidate> candidates = new List<Candidate>();

        uint numberOfCandidatesToGenerate = 5000;


        // do this over and over again
        for (uint iterationCandidate = 0; iterationCandidate < numberOfCandidatesToGenerate; iterationCandidate++) {
            Candidate currentCandidate;

            Map2d<float> compressedCompare = new Map2d<float>(image.getSize());


            int typeFromRandom = rand.Next(2);
            if( typeFromRandom == 0 ) {
                currentCandidate.type = Candidate.EnumType.LINE_HORIZONTAL;
            }
            else {
                currentCandidate.type = Candidate.EnumType.CIRCLE;
            }



            // draw candidate

            currentCandidate.startPosition = new Vector2d<uint>((uint)rand.Next((int)image.getSize().x), (uint)rand.Next((int)image.getSize().y));
            currentCandidate.attribute = (uint)rand.Next(10);

            if (currentCandidate.type == Candidate.EnumType.LINE_HORIZONTAL ) {
                
                drawer.drawHorizontalLine(compressedCompare, currentCandidate.startPosition, currentCandidate.attribute, 1.0f);
            }
            else { // CIRLCE
                drawer.drawCircle(compressedCompare, currentCandidate.startPosition, currentCandidate.attribute, 1.0f);
            }


            currentCandidate.distance = calcDistance(image, compressedCompare);

            candidates.Add(currentCandidate);
        }

        // search the best candidate
        {
            Candidate bestCandidate = candidates[0];

            foreach(Candidate iterationCandidate in candidates) {
                if (iterationCandidate.distance < bestCandidate.distance) {
                    bestCandidate = iterationCandidate;
                }
            }

            return bestCandidate;
        }

    }

    private float calcDistance(Map2d<float> image, Map2d<float> compressedCompare) {
        float distance = 0.0f;

        for (uint y = 0; y < image.getSize().y; y++) {
            for (uint x = 0; x < image.getSize().x; x++) {
                Vector2d<uint> position;
                position.x = x;
                position.y = y;

                float valueImage = image.read(position);
                float valueCompressedCompare = compressedCompare.read(position);

                distance += ((valueImage - valueCompressedCompare) * (valueImage - valueCompressedCompare));
            }
        }

        return distance;
    }

    private Random rand = new Random();
    private ImageDrawer<float> drawer = new ImageDrawer<float>();
}


class TestInverseGraphicsCompressor {
    public static void testImageCompressor() {
        Vector2d<uint> imageSize = new Vector2d<uint>(5, 7);

        Map2d<float> stimulus = new Map2d<float>(imageSize);

        ImageDrawer<float> drawer = new ImageDrawer<float>();

        // draw candidate
        drawer.drawHorizontalLine(stimulus, new Vector2d<uint>(1, 1), 3, 1.0f);

        TestEncoder testEncoder = new TestEncoder();

        TestEncoder.Candidate bestCandidate = testEncoder.encode(stimulus);


        int debug = 1;
    }
}



class Program {
    static void testCompactIntegerVector() {
        CompactIntegerVector compactIntegerVector = new CompactIntegerVector(4, 5);

        compactIntegerVector.setElement(0, 5);

        uint element = compactIntegerVector.getElement(0);


    }

    static void testBitvectorInduction() {
        SlowBitvector testBitvector = new SlowBitvector();
        //testBitvector.vector = new bool[]{false, true, false, false};
        testBitvector.vector = new List<bool>() { false, true, false, true,  };

        induction.BitvectorInduction.predictNextBit(testBitvector, 1);
    }

    static void testBitvectorInductionSequence1() {
        SlowBitvector testBitvector = new SlowBitvector();

        /*
        testBitvector.vector = new List<bool>() {
            true, true, false, false,
            };
        */

        
        testBitvector.vector = new List<bool>() {
            false, false, false,
            false, false, true, 
            false, true, false,
            false, true, true, 

            true, false, false,
            true, false, true, 
            true, true, false,
            };
        
        bool predictedBit;
        induction.BitvectorInduction.Prediction prediction;

        for (int i = 0; i < 1; i++) {
            prediction = induction.BitvectorInduction.predictNextBit(testBitvector, 1);

            predictedBit = prediction.trueCounter > prediction.falseCounter;
            Console.WriteLine("predicted {0}", predictedBit);

            testBitvector.vector.Add(predictedBit);
        }

        int breakHere = 1;
    }

    static void testInductionSymbolEnumerationContext() {
        uint countOfSymbolsInAlphabet = 2;
        uint maxNumberOfSymbols = 3;
        induction.BitvectorInduction.SymbolEnumerationContext symbolicEnumerationContext = induction.BitvectorInduction.SymbolEnumerationContext.make(countOfSymbolsInAlphabet, maxNumberOfSymbols);

        uint numberOfDecodedSymbols;
        bool finished;

        for (int repetition = 0; repetition < 12; repetition++) {
            symbolicEnumerationContext.increment(out numberOfDecodedSymbols, out finished);

            Console.WriteLine("numberOfDecodedSymbols={0}", numberOfDecodedSymbols);
            Console.WriteLine("finished={0}", finished);

            if (numberOfDecodedSymbols == 1) {
                Console.WriteLine("{0}", symbolicEnumerationContext.decodedSymbols[0]);
            }
            else if (numberOfDecodedSymbols == 2) {
                Console.WriteLine("{0} {1}", symbolicEnumerationContext.decodedSymbols[1], symbolicEnumerationContext.decodedSymbols[0]);
            }
            else if (numberOfDecodedSymbols == 3) {
                Console.WriteLine("{0} {1} {2}", symbolicEnumerationContext.decodedSymbols[2], symbolicEnumerationContext.decodedSymbols[1], symbolicEnumerationContext.decodedSymbols[0]);

            }


            
            Console.WriteLine("");
        }
        
        int breakHere = 0;
    }

    static private void testTreeTransducer() {
        FunctionalProgramTransducerFacade transducerFacade = new FunctionalProgramTransducerFacade();

        List<Rule<FunctionalProgramElement>> rules = new List<Rule<FunctionalProgramElement>>();
        
        {
            FunctionalProgramElement matching = new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.BRACE));

            matching.children = new List<FunctionalProgramElement>();
            matching.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            matching.children[0].expression = "+";
            matching.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.REWRITE_VARIABLE)));
            matching.children[1].expression = "a";
            matching.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.REWRITE_VARIABLE)));
            matching.children[2].expression = "b";

            FunctionalProgramElement target = new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.BRACE));

            target.children = new List<FunctionalProgramElement>();
            target.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[0].expression = "NODE";

            target.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.BRACE)));
            target.children[1].children = new List<FunctionalProgramElement>();
            target.children[1].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[1].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[1].children[0].expression = "emit";
            target.children[1].children[1].expression = "newinstruction";
            
            target.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.BRACE)));
            target.children[2].children = new List<FunctionalProgramElement>();
            target.children[2].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[2].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.REWRITE_VARIABLE)));
            target.children[2].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[2].children[0].expression = "calc";
            target.children[2].children[1].expression = "a";
            target.children[2].children[2].expression = "aResult";

            target.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.BRACE)));
            target.children[3].children = new List<FunctionalProgramElement>();
            target.children[3].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[3].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.REWRITE_VARIABLE)));
            target.children[3].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[3].children[0].expression = "calc";
            target.children[3].children[1].expression = "b";
            target.children[3].children[2].expression = "bResult";

            target.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.BRACE)));
            target.children[4].children = new List<FunctionalProgramElement>();
            target.children[4].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[4].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[4].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[4].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[4].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[4].children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
            target.children[4].children[0].expression = "emit";
            target.children[4].children[1].expression = "slot1";
            target.children[4].children[2].expression = "+";
            target.children[4].children[2].expression = "aResult";
            target.children[4].children[2].expression = "bResult";
            target.children[4].children[2].expression = "RESULT";


            rules.Add(new Rule<FunctionalProgramElement>(matching, target));


            // TODO
        }


        FunctionalProgramElement apply = new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.BRACE));
        apply.children = new List<FunctionalProgramElement>();
        apply.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
        apply.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
        apply.children.Add(new FunctionalProgramElement(new FunctionalProgramElementType(FunctionalProgramElementType.EnumType.VARIABLE)));
        apply.children[0].expression = "+";
        apply.children[1].expression = "myA";
        apply.children[2].expression = "myB";



        FunctionalProgramElement translated = TreeTransducer<FunctionalProgramElement>.tryToApplyRulesRecursivly(rules, apply, transducerFacade);

        /*
        rules.Add(new Rule<TreePayload>());
        rules[0].matching = new TreeElement<TreePayload>();
        rules[0].matching.type = TreeElement<TreePayload>.EnumType.VALUE;
        rules[0].matching.value = n


        rules[0].matching.type = TreeElement<TreePayload>.EnumType.VARIABLE;
        rules[0].matching.variable = "a";

        rules[0].rewriteTarget = new TreeElement<TreePayload>();
        rules[0].rewriteTarget.type = TreeElement<TreePayload>.EnumType.VALUE;
        rules[0].rewriteTarget.value = new TreePayload();
        rules[0].rewriteTarget.value.value = 5;


        TreeElement<TreePayload> toTranslate = new TreeElement<TreePayload>();
        toTranslate.value = new TreePayload();
        toTranslate.value.value = 42;


        TreeElement<TreePayload> translated = TreeTransducer<TreePayload>.tryToApplyRulesRecursivly(rules, toTranslate);
        */
        int debugHere = 1;
    }

    
    /*static void Main(string[] args) {
        // TestInverseGraphicsCompressor.testImageCompressor();

        //LevinTest1.entry();

        // TODO< levinTest2 >

        //testCompactIntegerVector();

        //testInductionSymbolEnumerationContext();

        // induction tests
        //testBitvectorInduction(); // TODO< build unittest from this >
        //testBitvectorInductionSequence1();


        /
        testTreeTransducer();

        execution.lowLevel.LowLevelCodegenTest.test();

        neural.ConvolutionalTriggerNetwork.test();
        *


        // EnumTest.SuperOptimizer1;
        //EnumTest test = EnumTest.ReadFunctionalAndParseAndExecute1;
        
        UtilityFunctionPROTO instance = new UtilityFunctionPROTO();
        instance.test();
    }*/
    

    enum EnumTest {
        SuperOptimizer1,
        Native1,
        Matching1,
        PatternInterpreter1,
        PatternInterpreterExec1,
        FunctionalParserTest1,
        ReadFunctionalAndParseAndExecute1,
    }
}

