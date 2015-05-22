using System;
using System.Collections.Generic;

namespace Misc
{
    class Deepcopy
    {
        // TODO< too slow, speed it up with C# magic >
        public static List<int> deepCopyListInt(List<int> input)
        {
            List<int> result;

            result = new List<int>();

            foreach( int iteration in input )
            {
                result.Add(iteration);
            }

            return result;
        }
    }
}
