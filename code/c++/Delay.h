#include <vector>
using namespace std;

template<typename Type>
struct Delay {
	vector<Type> array;

	Type getLast() const {
		return array[array.size()-1];
	}

	void propagate(Type value) {
      
      for( size_t i = 0; i < array.size()-1; i++ ) {
			array[i+1] = array[i];
		}

		array[0] = value;
	}

};

void cTest(Delay<unsigned> &delay, bool value) {
	delay.propagate(value);
}
