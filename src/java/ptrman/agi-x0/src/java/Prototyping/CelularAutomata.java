package Prototyping;

import Evolvator.NetworkGlobalSettings;
import ptrman.mltoolset.CellularAutomata.CellularAutomata;

import java.util.Random;

public class CelularAutomata {
    NetworkGlobalSettings networkGlobalSettings = new NetworkGlobalSettings();


    public static void main(String[] arguments) {
        int numberOfCells = 20;

        Random random = new Random();

        int[] rules;

        rules = new int[numberOfCells];

        for( int cellI = 0; cellI < numberOfCells; cellI++ ) {
            rules[cellI] = NetworkGlobalSettings.celularAutomataRules.sample();
        }


        ptrman.mltoolset.CellularAutomata.CellularAutomata cellularAutomata = new ptrman.mltoolset.CellularAutomata.CellularAutomata();

        cellularAutomata.wrapAround = true;

        cellularAutomata.cells = new CellularAutomata.Cell[numberOfCells];
        for( int cellI = 0; cellI < numberOfCells; cellI++ ) {
            cellularAutomata.cells[cellI] = CellularAutomata.Cell.createFromRulenumber(rules[cellI]);
        }

        cellularAutomata.inputVector = new boolean[numberOfCells];
        cellularAutomata.resultVector = new boolean[numberOfCells];

        // init input vector to random
        for( int cellI = 0; cellI < numberOfCells; cellI++ ) {
            cellularAutomata.inputVector[cellI] = random.nextInt(2) == 1;
        }


        for( int round = 0; round < 50; round++ ) {
            cellularAutomata.inputVector[1] = (round % 3) == 0 ^ (round %4)==0;

            System.out.println(booleanVectorToConsole(cellularAutomata.resultVector));

            cellularAutomata.update();
            cellularAutomata.storeResultVector();
            cellularAutomata.outputToInput();
        }

        //networkGlobalSettings.

    }

    private static String booleanToConsole(final boolean value) {
        if( value ) {
            return "x";
        }
        return ".";
    }

    private static String booleanVectorToConsole(final boolean[] vector) {
        String result = "";

        for( int i = 0; i < vector.length; i++ ) {
            result += booleanToConsole(vector[i]);
        }

        return result;
    }

}