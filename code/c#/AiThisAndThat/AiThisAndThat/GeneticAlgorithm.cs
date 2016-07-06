using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace evolutionaryAlgorithms.geneticAlgorithm
{
    class GeneticAlgorithm
    {
        public int genomeBits;
        private Random random = new Random();
        private List<Genome> genomesWithRating = new List<Genome>();

        public int eliteCount = 3; /** \brief configuration, number of elites */
        public int mutateNumberOfBitsFlipped = 3; /** \brief configuration, number of flipped bits of a mutation */

        public interface IRating
        {
            void rating(List<Genome> genomes);
        }

        public GeneticAlgorithm(IRating ratingCallback)
        {
            this.ratingCallback = ratingCallback;
        }

        public void initPool()
        {
            int genomeI;

            for( genomeI = 0; genomeI < 10; genomeI++ )
            {
                Genome genomeWithRating;
                int bitI;

                genomeWithRating = new Genome(genomeBits);

                for( bitI = 0; bitI < genomeBits; bitI++ )
                {
                    genomeWithRating.genome[bitI] = random.Next(1) == 0;
                }

                genomesWithRating.Add(genomeWithRating);
            }
        }

        public Genome getBestGenome(out int bestIndex)
        {
            float bestGenomeRating;
            int i;

            rating();

            // search best

            bestIndex = 0;
            bestGenomeRating = genomesWithRating[0].rating;

            for( i = 1; i < genomesWithRating.Count; i++ )
            {
                if( genomesWithRating[i].rating < bestGenomeRating )
                {
                    bestIndex = i;
                    bestGenomeRating = genomesWithRating[i].rating;
                }
            }

            return genomesWithRating[bestIndex];
        }

        public void run()
        {
            List<Genome> newRound, crossOverGenoms;
            float fitnessSum;
            float currentThreshold;
            int eliteI;
            int genomeI;

            newRound = new List<Genome>();
            crossOverGenoms = new List<Genome>();

            rating();

            // elitism selection algorithm

            // we choose the top 3 genoms and transfer them into the winner

            

            currentThreshold = float.PositiveInfinity;

            for( eliteI = 0; eliteI < eliteCount; eliteI++ )
            {
                int highestIndex;
                bool resultValid;

                highestIndex = getHighestGenomWithRatingBelow(out resultValid, currentThreshold);

                newRound.Add(genomesWithRating[highestIndex]);

                currentThreshold = genomesWithRating[highestIndex].rating;

                // update the highest rating
                if( eliteI == 0 )
                {
                    highestRating = currentThreshold;
                }
            }

            newRound.AddRange(getAllGenomsWithRatingBelow(currentThreshold));

            // selection
            // we use the Roulette Wheel Selection algorithm
          
            fitnessSum = 0.0f;

            foreach( Genome iterationGenome in newRound )
            {
                fitnessSum += iterationGenome.rating;
            }

            for(;;)
            {
                float currentSum;
                int mateI;
                int[] mateIndices; // indices of genoms which need to be combined
                float[] roulettValues;
                bool[] crossoverResult;

                mateIndices = new int[2];
                roulettValues = new float[2];

                roulettValues = GeneticAlgorithm.getRoulettPair(random, fitnessSum, newRound.Count);

                currentSum = 0.0f;
                mateI = 0;

                for( genomeI = 0; genomeI < newRound.Count; genomeI++ )
                {
                    currentSum += newRound[genomeI].rating;

                    if( currentSum > roulettValues[mateI] )
                    {
                        mateIndices[mateI] = genomeI;

                        mateI++;

                        if( mateI == 2 )
                        {
                            break;
                        }
                    }
                }

                // sanity check
                if( mateI == 0 )
                {
                    Debug.Assert(false, "Should not happen!");
                }
                else if( mateI == 1 )
                {
                    // if not all mates have been chosen
                    Debug.Assert(false, "TODO???");
                }

                crossoverResult = crossover(newRound[mateIndices[0]].genome, newRound[mateIndices[1]].genome);

                // create the new crossover genom
                Genome crossOverGenom;

                crossOverGenom = new Genome(genomeBits);
                crossOverGenom.genome = crossoverResult;
                crossOverGenom.rating = float.PositiveInfinity;

                crossOverGenoms.Add(crossOverGenom);

                break;
            }

            System.Diagnostics.Debug.Assert(crossOverGenoms.Count == 1);

            newRound.AddRange(crossOverGenoms);

            genomesWithRating = newRound;

            while( genomesWithRating.Count > 10 )
            {
                // remove the genom of the lowest rating

                int lowestFittnessI;

                lowestFittnessI = GeneticAlgorithm.getIndexOfLowestFittness(genomesWithRating);

                genomesWithRating.RemoveAt(lowestFittnessI);
            }
      
            // mutate
            // we mutate all but the elites not

            for( genomeI = 2; genomeI < genomesWithRating.Count; genomeI++ )
            {
                Genome iterationGenome;

                iterationGenome = genomesWithRating[genomeI];

                mutate(ref iterationGenome.genome);
            }
        }

        static private int getIndexOfLowestFittness(List<Genome> genoms)
        {
            int resultI;
            float lowestFittness;
            int genomeI;

            Debug.Assert(genoms.Count > 0);

            resultI = 0;
            lowestFittness = genoms[0].rating;

            for( genomeI = 1; genomeI < genoms.Count; genomeI++ )
            {
                if( genoms[genomeI].rating < lowestFittness )
                {
                    resultI = genomeI;
                    lowestFittness = genoms[genomeI].rating;
                }
            }

            return resultI;
        }

        static private float[] getRoulettPair(Random randomGenerator, float fitnessSum, int count)
        {
            float[] results;
            int[] intResults;

            Debug.Assert(fitnessSum > 0.0f);
            Debug.Assert(count > 0);
            
            intResults = new int[2];
            results = new float[2];

            for(;;)
            {
                intResults[0] = randomGenerator.Next(count);
                intResults[1] = randomGenerator.Next(count);

                if( intResults[0] == intResults[1] )
                {
                    continue;
                }

                results[0] = ((float)intResults[0]/(float)count) * fitnessSum;
                results[1] = ((float)intResults[1]/(float)count) * fitnessSum;

                break;
            }

            if( results[0] > results[1] )
            {
                float temp;

                temp = results[0];
                results[0] = results[1];
                results[1] = temp;
            }

            Debug.Assert(results[0] < results[1]);

            return results;
        }

        private List<Genome> getAllGenomsWithRatingBelow(float Threshold)
        {
            List<Genome> result;

            result = new List<Genome>();

            foreach (Genome iterationGenome in genomesWithRating)
            {
                if (iterationGenome.rating < Threshold)
                {
                    result.Add(iterationGenome);
                }
            }

            return result;
        }

        
        private int getHighestGenomWithRatingBelow(out bool resultValid, float threshold)
        {
            int highestIndex;
            float highestRating;
            bool found;
            int i;

            resultValid = false;

            found = false;

            // to make compiler happy
            highestRating = float.PositiveInfinity;
            highestIndex = 0;

            for( i = 0; i < genomesWithRating.Count; i++ )
            {
                if( genomesWithRating[i].rating < threshold )
                {
                    highestRating = this.genomesWithRating[i].rating;
                    highestIndex = i;
                    found = true;
                    break;
                }
            }

            if( !found )
            {
                return 0; // didn't found anything
            }

            // search for genom with higher rating
            // TODO< maybe this can be optimized a bit >
            for( i = 0; i < genomesWithRating.Count; i++ )
            {
                if (this.genomesWithRating[i].rating >= highestRating && this.genomesWithRating[i].rating < threshold)
                {
                    highestRating = genomesWithRating[i].rating;
                    highestIndex = i;
                }
            }

            resultValid = true;
            return highestIndex;
        }

        private void mutate(ref bool[] operand)
        {
            int bitflipCounter;

            for( bitflipCounter = 0; bitflipCounter < mutateNumberOfBitsFlipped; bitflipCounter++ )
            {
                int bitflipIndex;

                bitflipIndex = random.Next(genomeBits);

                operand[bitflipIndex] = !operand[bitflipIndex];
            }
        }

        private bool[] crossover(bool[] a, bool[] b)
        {
            // implements single point cross over

            int crossOverPoint0;
            bool[] result;
            int i;

            result = (bool[])a.Clone();
            
            crossOverPoint0 = random.Next(genomeBits);

            for( i = crossOverPoint0; i < genomeBits; i++ )
            {
                result[i] = b[i];
            }

            return result;
        }

        private void rating()
        {
            ratingCallback.rating(genomesWithRating);
        }

        // contains the highest rating of an element
        public float highestRating;

        private IRating ratingCallback;
    }
}
