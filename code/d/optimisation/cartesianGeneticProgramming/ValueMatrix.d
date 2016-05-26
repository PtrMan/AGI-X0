module optimisation.cartesianGeneticProgramming.ValueMatrix;

template ValueMatrix(Type) {
    class ValueMatrix {
    	public final this(uint width, uint height) {
    		protectedWidth = width;
    		protectedHeight = height;
    		
    		data.length = width*height;
    	}
    	
		public final Type opIndexAssign(Type value, size_t row, size_t column) {
			return data[row*protectedWidth + column] = value;
		}

		public final Type opIndex(size_t row, size_t column) {
			return data[row*protectedWidth + column];
		}
    	
        public final @property uint width() {
        	return protectedWidth;
        }
        
        public final @property uint height() {
        	return protectedHeight;
        }
    	
        protected Type[] data;
        
        protected uint protectedWidth, protectedHeight;
    }
}
