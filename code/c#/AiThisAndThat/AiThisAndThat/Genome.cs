using System;
using System.Collections.Generic;

namespace evolutionaryAlgorithms.geneticAlgorithm
{
    class Genome
    {
        public Genome(int genomeWidth)
        {
            genome = new bool[genomeWidth];
        }

        public bool[] genome;
        public float rating = 0.0f;
    }

}
