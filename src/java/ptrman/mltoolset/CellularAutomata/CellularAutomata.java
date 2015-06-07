package ptrman.mltoolset.CellularAutomata;

/**
 *
 */
public class CellularAutomata {
    public static class Rule {
        public Rule(final boolean result) {
            this.result = result;
        }

        public final boolean result;
    }

    public static class Cell {
        public static Cell createFromRules(final  Rule[] rules) {
            return new Cell(rules);
        }

        public static Cell createFromRulenumber(final int rulenumber) {
            Rule[] rules = new Rule[8];

            for( int index = 0; index < 8; index++ ) {
                rules[index] = new Rule((rulenumber & (1 << index)) != 0);
            }

            return createFromRules(rules);
        }

        private Cell(final Rule[] rules) {
            this.rules = rules;
        }

        public void calculateActiviationFor(final boolean b0, final boolean b1, final boolean b2) {
            int ruleAddress = 0;
            ruleAddress |= (b2 ? 1 : 0);
            ruleAddress |= (b1 ? 2 : 0);
            ruleAddress |= (b0 ? 4 : 0);

            activiation = rules[ruleAddress].result;
        }

        public boolean activiation;

        public final Rule[] rules; // all (8) rules for the input bits 000 001 010 011 100 101 110 111
    }

    public void update() {
        if( wrapAround ) {
            cells[0].calculateActiviationFor(inputVector[inputVector.length-1], inputVector[0], inputVector[1]);
            cells[cells.length-1].calculateActiviationFor(inputVector[inputVector.length - 2], inputVector[inputVector.length - 1], inputVector[0]);
        }
        else {
            cells[0].calculateActiviationFor(false, inputVector[0], inputVector[1]);
            cells[cells.length-1].calculateActiviationFor(inputVector[inputVector.length-2], inputVector[inputVector.length-1], false);
        }

        for( int cellI = 1; cellI < cells.length-1; cellI++ ) {
            cells[cellI].calculateActiviationFor(inputVector[cellI-1], inputVector[cellI], inputVector[cellI+1]);
        }
    }

    public void storeResultVector() {
        for( int i = 0; i < resultVector.length; i++ ) {
            resultVector[i] = cells[i].activiation;
        }
    }

    public void outputToInput() {
        for( int i = 0; i < inputVector.length; i++ ) {
            inputVector[i] = resultVector[i];
        }
    }

    public boolean[] inputVector;
    public Cell[] cells;
    public boolean[] resultVector;
    public boolean wrapAround;
}
