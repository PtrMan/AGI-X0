using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class MyMath {
    public static uint divRoundUp(uint a, uint div) {
        uint remainder = a % div;
        return (a / div) + ((remainder == 0) ? 0u : 1u); 
    }
}

