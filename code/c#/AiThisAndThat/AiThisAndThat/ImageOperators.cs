
using System;

public interface IOp2 {
	void run2(Map2d<float> a, Map2d<float> b, Map2d<float> result);
}

public interface IOp1 {
	void run2(Map2d<float> a, Map2d<float> result);
}

public class OpAdd : IOp2 {
	public void run2(Map2d<float> a, Map2d<float> b, Map2d<float> result) {
		for( uint y = 0; y < result.getSize().y; y++ )
			for( uint x = 0; x < result.getSize().x; x++ ) {
				Vector2d<uint> pos = new Vector2d<uint>(x, y);
				float
					aValue = a.read(pos),
					bValue = b.read(pos),
					temp = aValue + bValue;
				result.write(pos, temp);
			}
	}
}

public class OpSub : IOp2 {
	public void run2(Map2d<float> a, Map2d<float> b, Map2d<float> result) {
		for( uint y = 0; y < result.getSize().y; y++ )
			for( uint x = 0; x < result.getSize().x; x++ ) {
				Vector2d<uint> pos = new Vector2d<uint>(x, y);
				float
					aValue = a.read(pos),
					bValue = b.read(pos),
					temp = aValue - bValue;
				result.write(pos, temp);
			}
	}
}

public class OpMul : IOp2 {
	public void run2(Map2d<float> a, Map2d<float> b, Map2d<float> result) {
		for( uint y = 0; y < result.getSize().y; y++ )
			for( uint x = 0; x < result.getSize().x; x++ ) {
				Vector2d<uint> pos = new Vector2d<uint>(x, y);
				float
					aValue = a.read(pos),
					bValue = b.read(pos),
					temp = aValue * bValue;
				result.write(pos, temp);
			}
	}
}

public class OpDiv : IOp2 {
	public void run2(Map2d<float> a, Map2d<float> b, Map2d<float> result) {
		for( uint y = 0; y < result.getSize().y; y++ )
			for( uint x = 0; x < result.getSize().x; x++ ) {
				Vector2d<uint> pos = new Vector2d<uint>(x, y);
				float
					aValue = a.read(pos),
					bValue = b.read(pos),
					temp = aValue / bValue;
				result.write(pos, temp);
			}
	}
}

public class OpMax : IOp2 {
	public void run2(Map2d<float> a, Map2d<float> b, Map2d<float> result) {
		for( uint y = 0; y < result.getSize().y; y++ )
			for( uint x = 0; x < result.getSize().x; x++ ) {
				Vector2d<uint> pos = new Vector2d<uint>(x, y);
				float
					aValue = a.read(pos),
					bValue = b.read(pos),
					temp = Math.Max(aValue, bValue);
				result.write(pos, temp);
			}
	}
}

public class OpMin : IOp2 {
	public void run2(Map2d<float> a, Map2d<float> b, Map2d<float> result) {
		for( uint y = 0; y < result.getSize().y; y++ )
			for( uint x = 0; x < result.getSize().x; x++ ) {
				Vector2d<uint> pos = new Vector2d<uint>(x, y);
				float
					aValue = a.read(pos),
					bValue = b.read(pos),
					temp = Math.Min(aValue, bValue);
				result.write(pos, temp);
			}
	}
}



public class OpSqrt : IOp1 {
	public void run2(Map2d<float> a, Map2d<float> result) {
		for( uint y = 0; y < result.getSize().y; y++ )
			for( uint x = 0; x < result.getSize().x; x++ ) {
				Vector2d<uint> pos = new Vector2d<uint>(x, y);
				float
					aValue = a.read(pos),
					temp = (float)Math.Sqrt(aValue);
				result.write(pos, temp);
			}
	}
}

