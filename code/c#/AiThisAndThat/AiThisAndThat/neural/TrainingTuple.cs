using System.Collections.Generic;

namespace AiThisAndThat.neural {
    public class TrainingTuple<Type> {
        public Type[] input;
        public Type[] output;

        public TrainingTuple(Type[] input, Type[] output) {
            this.input = input;
            this.output = output;
        }
    }
}
