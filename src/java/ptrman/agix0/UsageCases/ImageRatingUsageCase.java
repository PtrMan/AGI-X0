package ptrman.agix0.UsageCases;

import ptrman.agix0.Common.Evironment.Environment;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

/**
 * used to classify between binary images
 */
public class ImageRatingUsageCase implements IUsageCase {
    private final Random random;

    public int shownImages = 0;
    public List<Image> images = new ArrayList<>();

    public List<Integer> chosenImageIndices;

    private List<Image.Line> linesRemaining = new ArrayList<>();

    private int lineCounter;

    public static class Image {
        public static class Line {
            public boolean[] row;
        }

        public List<Line> lines = new ArrayList<>();
    }

    public ImageRatingUsageCase(Random random) {
        this.random = random;
    }

    public void setupNextRun() {
        lineCounter = 0;

        // chose random images

        List<Integer> remainingImages = new ArrayList<>();
        chosenImageIndices = new ArrayList<>();

        for( int imageI = 0; imageI < shownImages; imageI++ ) {
            int chosenImage = random.nextInt(images.size());
            remainingImages.add(chosenImage);
            chosenImageIndices.add(chosenImage);
        }

        // translate it to the lines it should perceive
        linesRemaining = new ArrayList<>();
        for( int chosenImageIndex : chosenImageIndices ) {
            Image image = images.get(chosenImageIndex);

            for( Image.Line iterationLine : image.lines ) {
                linesRemaining.add(iterationLine);
            }
        }

    }

    public boolean getCurrentImageIndex() {
        return
    }

    @Override
    public int getNumberOfNeuralSimulationSteps() {
        return shownImages*images.size();
    }

    @Override
    public boolean[] beforeNeuroidSimationStepGetNeuroidInputForNextStep(Environment environment, int stepCounter) {
        Image.Line resultLine = linesRemaining.get(0);

        linesRemaining.remove(0);

        return resultLine.row;
    }

    @Override
    public void afterNeuroidSimulationStep(Environment environment, boolean[] hiddenNeuronActivation) {

    }
}
