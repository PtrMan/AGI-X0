module NearestNeighbors;

float distSquared(float[3] a, float[3] b) {
	float dx = a[0] - b[0];
  float dy = a[1] - b[1];
  float dz = a[2] - b[2];
  return dx*dx + dy*dy + dz*dz;
}

void nNearestNeighbors(uint n)(float[3][] points, float[3] center,  out float[3][n] resultPositions, out float[n] resultDistances) {
  if( points.length == 0 ) {
    return;
  }
  
  // setup
  foreach( i; 0..n ) {
    resultPositions[i] = points[0];
    resultDistances[i] = distSquared(points[0], center);
  }
  
  foreach( iterationPoint; points ) {
    float distance = distSquared(iterationPoint, center);
    findInsert(distance, iterationPoint,  /*ref*/ resultDistances, /*ref*/ resultPositions);
  }
}

// points are ordered from low to high
private void findInsert(uint n)(float toInsertDistance, float[3] toInsertPosition,  ref float[n] distances, ref float[3][n] resultPositions) {
  foreach( i; 0..distances.length ) {
    if( toInsertDistance < distances[i] ) {
      insertAndMove(distances, toInsertDistance, i);
      insertAndMove(resultPositions, toInsertPosition, i);
      return;
    }
  }
  
  // else the point falls out of the array, we can safely drop it
}

private void insertAndMove(uint n, Type)(ref Type[n] arr, Type insertValue, size_t insertionIndex) {
  foreach( i; insertionIndex..n-1) {
    arr[i+1] = arr[i];
  }
  arr[insertionIndex] = insertValue;
}
