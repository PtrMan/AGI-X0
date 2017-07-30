using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteSphereEngine.entity {
    public class EntityManager {
        public void addEntity(Entity entity) {
            privateEntities.Add(entity);
            if(entity.doesAnyComponentRequireUpdate()) {
                entitiesWithUpdate.Add(entity);
            }
        }

        public void removeEntity(Entity entity) {
            privateEntities.Remove(entity);
            entitiesWithUpdate.Remove(entity);
        }

        public void updateAllEntities() {
            foreach( Entity iEntity in entitiesWithUpdate ) {
                iEntity.update();
            }
        }

        public void addComponentToEntity(Entity entity, IComponent component) {
            entity._components.Add(component);
            
            // recalc if it needs updates
            entity.invalideRequiresUpdateCache();

            updateEntitiesWithUpdateByEntity(entity);
            
        }


        // adds or removes the entity from the list of entities for which the update method has to be called
        void updateEntitiesWithUpdateByEntity(Entity entity) {
            bool doesNeedUpdate = entity.doesAnyComponentRequireUpdate();

            if (doesNeedUpdate) {
                if (!entitiesWithUpdate.Contains(entity)) {
                    entitiesWithUpdate.Add(entity);
                }
            }
            else {
                entitiesWithUpdate.Remove(entity);
            }
        }

        public IEnumerable<Entity> entities {
            get {
                return privateEntities;
            }
        }

        IList<Entity> privateEntities = new List<Entity>();
        IList<Entity> entitiesWithUpdate = new List<Entity>(); // list of entities which have an enabled update
    }
}
