using System;
using System.Collections.Generic;

using SuboptimalProcedureLearner.SuboptimalProcedureLearner;

namespace SuboptimalProcedureLearner
{
    class Program
    {
        static void Main(string[] args)
        {

            SuboptimalProcedureLearner.Unittests.UnittestOperatorBlueprint.unittest();
            /*
            Executive.Fiber executiveFiber;

            executiveFiber = new Executive.Fiber();
            executiveFiber.operatorPlan = new OperatorBlueprint();
            executiveFiber.operatorStack.Add(new Executive.Fiber.StackElement());
            executiveFiber.operatorStack[0].cachedPath = new List<int> { 0 };
            executiveFiber.operatorStack[0].operatorInstance = new OperatorInstance(new SuboptimalProcedureLearner.Operators.OperatorAdd());

            executiveFiber.configure(1000);

            for (;;)
            {
                if( executiveFiber.isExecutionFinished() )
                {
                    break;
                }

                if( executiveFiber.isExecutionStepCounterOverLimit() )
                {
                    break;
                }

                executiveFiber.step();
            }
            */
        }
    }
}
