using System;
using System.Diagnostics;

namespace WhiteSphereEngine.geometry {
    // contains vertices with attributes and a index array to the vertices, which can be used for describing polygons
    public class VerticesWithAttributesAndIndex {
        public VerticesWithAttributesAndIndex(AbstractMeshAttribute[] meshComponents, AbstractMeshAttribute indexMeshAttribute, int positionComponentIndex) {
            verticesAndAttributes = new VerticesWithAttributes(meshComponents, positionComponentIndex);

            this.protectedIndexMeshComponent = indexMeshAttribute;
            
            Debug.Assert(indexMeshAttribute.dataType == AbstractMeshAttribute.EnumDataType.UINT32);
            checkIndexComponentIndicesInRange(indexMeshAttribute, (int)numberOfVertices);
        }

        protected static void checkIndexComponentIndicesInRange(AbstractMeshAttribute indexMeshComponent, int numberOfVertices) {
            AbstractMeshAttribute.Uint32Accessor accessor = indexMeshComponent.getUint32Accessor();
            for (int i = 0; i < indexMeshComponent.length; i++) {
                // TODO< should throw exception on missmatch >
                Debug.Assert(accessor[i].Length < numberOfVertices);
            }
        }

        public AbstractMeshAttribute.Uint32Accessor getUint32AccessorForIndexBuffer() {
            return protectedIndexMeshComponent.getUint32Accessor();
        }
        
        public AbstractMeshAttribute indexBufferMeshComponent {
            get {
                return protectedIndexMeshComponent;
            }
        }



        public uint numberOfComponents {
            get {
                return verticesAndAttributes.numberOfVertices;
            }
        }

        public AbstractMeshAttribute.EnumDataType getDatatypeOfComponent(uint index) {
            return verticesAndAttributes.getDatatypeOfComponent(index);
        }

        public AbstractMeshAttribute.Float4Accessor getFloat4AccessorByComponentIndex(int index) {
            return verticesAndAttributes.getFloat4AccessorByComponentIndex(index);
        }

        public AbstractMeshAttribute.Float2Accessor getFloat2AccessorByComponentIndex(int index) {
            return verticesAndAttributes.getFloat2AccessorByComponentIndex(index);
        }

        public AbstractMeshAttribute.Double4Accessor getDouble4AccessorByComponentIndex(int index) {
            return verticesAndAttributes.getDouble4AccessorByComponentIndex(index);
        }

        public AbstractMeshAttribute.Double4Accessor double4PositionAccessor {
            get {
                return verticesAndAttributes.double4PositionAccessor;
            }
        }

        public AbstractMeshAttribute.Float4Accessor float4PositionAccessor {
            get {
                return verticesAndAttributes.float4PositionAccessor;
            }
        }

        public uint numberOfVertices {
            get {
                return verticesAndAttributes.numberOfVertices;
            }
        }


        // members are not changable from the outside after setting them
        protected AbstractMeshAttribute protectedIndexMeshComponent;

        VerticesWithAttributes verticesAndAttributes;
    }

    // contains vertices with attributes
    public class VerticesWithAttributes {
        public VerticesWithAttributes(AbstractMeshAttribute[] meshComponents, int positionComponentIndex) {
            Trace.Assert(positionComponentIndex >= 0 && positionComponentIndex < meshComponents.Length);
            checkMeshComponentsHaveEqualSize(meshComponents);
            
            this.protectedMeshComponents = meshComponents;
            this.positionComponentIndex = positionComponentIndex;
            
            cacheMeshComponentAccessors();
        }

        protected static void checkMeshComponentsHaveEqualSize(AbstractMeshAttribute[] meshComponents) {
            Debug.Assert(meshComponents.Length > 0);

            int lengthOfFirstComponent = (int)meshComponents[0].length;

            foreach (AbstractMeshAttribute iterationComponent in meshComponents ) {
                // TODO< should be an exception >
                Debug.Assert(iterationComponent.length == lengthOfFirstComponent);
            }
        }
        
        protected void cacheMeshComponentAccessors() {
            cachedFloat4Accessors = new AbstractMeshAttribute.Float4Accessor[numberOfComponents];
            cachedFloat2Accessors = new AbstractMeshAttribute.Float2Accessor[numberOfComponents];
            cachedDouble4Accessors =  new AbstractMeshAttribute.Double4Accessor[numberOfComponents];

            int i = 0;
            foreach (AbstractMeshAttribute iterationComponent in protectedMeshComponents ) {
                switch (iterationComponent.dataType) {
				case AbstractMeshAttribute.EnumDataType.FLOAT4:
                    cachedFloat4Accessors[i] = iterationComponent.getFloat4Accessor();
                    break;

				case AbstractMeshAttribute.EnumDataType.FLOAT2:
                    cachedFloat2Accessors[i] = iterationComponent.getFloat2Accessor();
                    break;
				
				case AbstractMeshAttribute.EnumDataType.DOUBLE4:
                    cachedDouble4Accessors[i] = iterationComponent.getDouble4Accessor();
                    break;
				
				case AbstractMeshAttribute.EnumDataType.UINT32:
                    Debug.Assert(false, "Not jet supported cache of accessors for UINT32 component");
                    break;
                }
            }
        }

        public int numberOfComponents {
            get {
                return protectedMeshComponents.Length;
            }
        }

        public AbstractMeshAttribute.EnumDataType getDatatypeOfComponent(uint index) {
		    return protectedMeshComponents[index].dataType;
	    }
        
        public AbstractMeshAttribute.Float4Accessor getFloat4AccessorByComponentIndex(int index) {
            Debug.Assert(cachedFloat4Accessors[index] != null);
            return cachedFloat4Accessors[index];
        }

        public AbstractMeshAttribute.Float2Accessor getFloat2AccessorByComponentIndex(int index) {
            Debug.Assert(cachedFloat2Accessors[index] != null);
            return cachedFloat2Accessors[index];
        }

        public AbstractMeshAttribute.Double4Accessor getDouble4AccessorByComponentIndex(int index) {
            Debug.Assert(cachedDouble4Accessors[index] != null);
            return cachedDouble4Accessors[index];
        }

        public AbstractMeshAttribute.Double4Accessor double4PositionAccessor {
            get {
                return getDouble4AccessorByComponentIndex(positionComponentIndex);
            }
        }

        public AbstractMeshAttribute.Float4Accessor float4PositionAccessor {
            get {
                return getFloat4AccessorByComponentIndex(positionComponentIndex);
            }
        }

        public uint numberOfVertices {
            get {
                if (cachedFloat4Accessors[positionComponentIndex] != null ) {
                    return protectedMeshComponents[positionComponentIndex].length;
                }
        		else if (cachedDouble4Accessors[positionComponentIndex] != null ) {
                    return protectedMeshComponents[positionComponentIndex].length;
                }

                throw new Exception("should be unreachable");
            }
        }
        
        // members are not changable from the outside after setting them
        protected AbstractMeshAttribute[] protectedMeshComponents;
        protected AbstractMeshAttribute.Float4Accessor[] cachedFloat4Accessors; // elements are null for non-float4
        protected AbstractMeshAttribute.Float2Accessor[] cachedFloat2Accessors; // elements are null for non-float2
        protected AbstractMeshAttribute.Double4Accessor[] cachedDouble4Accessors; // elements are null for non-float4

        protected int positionComponentIndex;
    }
}
