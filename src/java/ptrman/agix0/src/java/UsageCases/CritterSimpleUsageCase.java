package ptrman.agix0.src.java.UsageCases;


import org.apache.commons.math3.linear.ArrayRealVector;
import org.jbox2d.common.Vec2;
import ptrman.agix0.src.java.Common.Evironment.Environment;
import ptrman.agix0.src.java.Common.Scripting.EnvironmentScriptingAccessor;

/**
 *
 */
public class CritterSimpleUsageCase implements IUsageCase {
    private final EnvironmentScriptingAccessor environmentScriptingAccessor;

    public CritterSimpleUsageCase(EnvironmentScriptingAccessor environmentScriptingAccessor) {
        this.environmentScriptingAccessor = environmentScriptingAccessor;
    }

    @Override
    public int getNumberOfNeuralSimulationSteps() {
        return 120;
    }

    @Override
    public boolean[] beforeNeuroidSimationStepGetNeuroidInputForNextStep(Environment environment, final int stepCounter) {
        final boolean stimulation;

        if( (stepCounter % 5) == 0 ) {
            stimulation = true;
        }
        else {
            stimulation = false;
        }

        boolean[] raySense = new boolean[1];

        final Vec2 entityPositionVec2 = environment.entities.get(0).body.body.getPosition();
        final ArrayRealVector entityPosition = new ArrayRealVector(new double[]{entityPositionVec2.x, entityPositionVec2.y});
        final ArrayRealVector entityDirection = environment.entities.get(0).getDirection();
        final ArrayRealVector rawStart = entityPosition.add(entityDirection.mapMultiply(2.1));

        final float RAYCAST_LENGTH = 5.0f;

        raySense[0] = environmentScriptingAccessor.physics2dNearestRaycast(rawStart, entityDirection, RAYCAST_LENGTH) != null;

        if( raySense[0] ) {
            int debugHere = 0;
        }

        return new boolean[]{stimulation, raySense[0]};
    }

    @Override
    public void afterNeuroidSimulationStep(Environment environment, boolean[] hiddenNeuronActivation) {
        final boolean moveForwardNeuralSignal = hiddenNeuronActivation[hiddenNeuronActivation.length-1];
        final boolean moveLeftNeuralSignal = hiddenNeuronActivation[hiddenNeuronActivation.length-2];
        final boolean moveRightNeuralSignal = hiddenNeuronActivation[hiddenNeuronActivation.length-3];

        float rotationDelta = 0.0f;

        if( moveLeftNeuralSignal ) {
            rotationDelta += 1.0f;
        }
        if( moveRightNeuralSignal ) {
            rotationDelta -= 1.0f;
        }

        rotationDelta *= 0.05f;

        environment.entities.get(0).angle2d += rotationDelta;


        if( moveForwardNeuralSignal ) {
            environment.entities.get(0).speed = 1.0f;
        }
        else {
            environment.entities.get(0).speed = 0.0f;
        }
    }
}
