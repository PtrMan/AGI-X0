// seth's quadtree

class Quad {
	// 0 1
	// 2 3
	Quad[4] childrens;
	bool value;

	bool isLeaf;

	static Quad makeLeaf(bool value) {
		Quad result = new Quad;
		result.isLeaf = true;
		result.value = value;
		return result;
	}
}



class QuadTransform : Quad {
	size_t[] destinationPath;

	static QuadTransform makeByPath(size_t[] path) {
		QuadTransform result = new QuadTransform;
		result.destinationPath = path;
		result.isLeaf = true;
		return result;
	}

}


private void unleafifyByPathRecursive(Quad destination, size_t[] path) {
	if( destination.isLeaf ) {
		destination.isLeaf = false;
		destination.childrens = [Quad.makeLeaf(false), Quad.makeLeaf(false), Quad.makeLeaf(false), Quad.makeLeaf(false)];
	}

	// recurse down
	if( path.length > 0 ) {
		size_t currentPathElement = path[0];
		unleafifyByPathRecursive(destination.childrens[currentPathElement], path[1..$]);
	}
}

private void setByPathRecursive(Quad destination, size_t[] path, bool value) {
	// recurse down
	if( path.length > 0 ) {
		assert(!destination.isLeaf);
		size_t currentPathElement = path[0];
		setByPathRecursive(destination.childrens[currentPathElement], path[1..$], value);
	}
	else {
		assert(destination.isLeaf);
		destination.value = value;
	}
}

// transforms root with transform quadtrees
Quad transformRecursive(Quad destination, Quad root, QuadTransform transform, size_t pathOffset) {
	if( root.isLeaf ) {
		assert(transform.isLeaf, "if root is a leaf then the transform has to be a leaf too"); // because we have to have a target for this leaf value
		
		// eventually create nodes so we can fill them with our values
		unleafifyByPathRecursive(destination, transform.destinationPath[pathOffset..$]);

		setByPathRecursive(destination, transform.destinationPath[pathOffset..$], root.value);
	}
	else {
		// recurse down

		assert(!transform.isLeaf);

		foreach( i; 0..4 ) {
			transformRecursive(destination, root.childrens[0], transform.childrens[0], pathOffset);
		}
	}
}

{
	Quad dest;
	dest.isLeaf = true;

	Quad transform = new Quad;
	transform.childrens = [QuadTransform.makeByPath(1), QuadTransform.makeByPath(0), QuadTransform.makeByPath(3), QuadTransform.makeByPath(2)];
	
	Quad toTransform = new Quad;
	toTransform.childrens = [Quad.makeLeaf(false), Quad.makeLeaf(true), Quad.makeLeaf(true), Quad.makeLeaf(false)];

	transform(dest, toTransform, transform, 0);
}
