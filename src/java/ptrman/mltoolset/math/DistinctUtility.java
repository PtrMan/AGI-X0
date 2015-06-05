package mltoolset.math;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class DistinctUtility {
    public static List<Integer> getTwoDisjunctNumbers(Random random, int max) {
        int randomNumber;
        List<Integer> temporaryList;
        List<Integer> additionalNumbers;

        randomNumber = random.nextInt(max);
        temporaryList = new ArrayList<>();
        temporaryList.add(randomNumber);

        additionalNumbers = getDisjuctNumbersTo(random, temporaryList, 1, max);
        temporaryList.addAll(additionalNumbers);
        return temporaryList;
    }

    public static List<Integer> getDisjuctNumbersTo(Random random, List<Integer> numbers, int count, int max) {
        int counter;
        List<Integer> disjunctNumbers;

        disjunctNumbers = new ArrayList<>();

        for( counter = 0; counter < count; counter++ )
        {
            for(;;)
            {
                int randomNumber;

                randomNumber = random.nextInt(max);
                if( !numbers.contains(randomNumber) && !disjunctNumbers.contains(randomNumber) )
                {
                    disjunctNumbers.add(randomNumber);
                    break;
                }
            }

        }

        return disjunctNumbers;
    }
}