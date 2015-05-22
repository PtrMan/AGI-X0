using System;
using System.Collections.Generic;


namespace Misc
{
    class ListTools
    {
        public static bool isListTheSameInt(List<int> a, List<int> b)
        {
            int i;
            
            if( a.Count != b.Count )
            {
                return false;
            }

            for( i = 0; i < a.Count; i++ )
            {
                if( a[i] != b[i] )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
