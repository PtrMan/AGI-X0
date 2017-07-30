using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

using MetaNix.misc;
using MetaNix.nars.config;
using MetaNix.nars.entity;

namespace MetaNix.nars.memory {
    /**
     * Original Bag implementation which distributes items into
     * discrete levels (queues) according to priority
     */
    public class LevelBag<E, K> : Bag<E, K> where E : Item<K> {

        private readonly uint levels; // priority levels
        
        public int fireCompleteLevelThreshold; // firing threshold

        short[] DISTRIBUTOR; // shared DISTRIBUTOR that produce the probability distribution
        
        public IDictionary<K, E> nameTable; // mapping from key to item

        public Level<E, K>[] level; // array of lists of items, for items on different level

        public readonly uint capacity; // defined in different bags

        private float mass; // current sum of occupied level

        int levelIndex; // index to get next level, kept in individual objects

        uint currentLevel; // current take out level
        uint currentCounter; // maximum number of items to be taken out at current level

        private bool[] levelEmpty;
    
        
        public LevelBag(uint levels, uint capacity) : this(levels, capacity, (int)(Parameters.BAG_THRESHOLD * levels)) {
        }

        /**
         * /param thresholdLevel = 0 disables "fire level completely" threshold effect
         */
        public LevelBag(uint levels, uint capacity, int thresholdLevel) {
            this.levels = levels;
            this.fireCompleteLevelThreshold = thresholdLevel;
            this.capacity = capacity;
            nameTable = new Dictionary<K, E>((int)capacity);
            level = new Level<E, K>[this.levels];
            levelEmpty = new bool[this.levels];
            ArrayUtilities.fill(ref levelEmpty, true);
            DISTRIBUTOR = Distributor.get(this.levels).order; 
            clear();
        }

        public class Level<E, K> : IEnumerable<E> where E : Item<K> {
            private uint thisLevel;
        
            //Deque<E> items;
            HashSet<E> items;

            LevelBag<E, K> parent;


            public Level(uint level, uint numElements, LevelBag<E, K> levelBag) : base() {
                items = new HashSet<E>();
                this.thisLevel = level;
                this.parent = levelBag;
            }
            
            public uint size() {
                return (uint)items.Count;
            }
            
            void levelIsEmpty(bool e) {
                parent.levelEmpty[thisLevel] = e;
            }
        
            public void clear() {
                items.Clear();
                levelIsEmpty(true);
            }

           public bool add(E e) {
               if (e == null)
                   throw new Exception("Bag requires non-null items");
           
                if (items.Add(e)) {
                    levelIsEmpty(false);
                    return true;
                }
                return false;
            }

            public bool remove(E o) {
                if (items.Remove(o)) {
                    levelIsEmpty(items.isEmpty());
                    return true;
                }
                return false;
            }

            public E removeFirst() {
                E e = items.GetEnumerator().Current;
                items.Remove(e);
                if (e!=null) {
                    levelIsEmpty(items.isEmpty());
                }
                return e;
            }

            public E peekFirst() {
                return items.GetEnumerator().Current;
            }
            
            public IEnumerator<E> GetEnumerator() {
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
    
        private Level<E, K> newLevel(uint l) {
            return new Level<E, K>(l, 1 + capacity / levels, this);
        }
    
        public override void clear() {
            for (int i = 0; i < levels; i++) {
                if( level[i] != null ) {
                    level[i].clear();
                }
            }
            nameTable.Clear();

            Debug.Assert(levels >= 1);
            currentLevel = levels - 1;

            levelIndex = (int)capacity % (int)levels; // so that different bags start at different point
            mass = 0;
            currentCounter = 0;
        }

        public override ulong size { get {
            int @in = nameTable.Count;
            if (Parameters.DEBUG_BAG && (Parameters.DEBUG)) {
                uint @is = sizeItems();
                if (Math.Abs(@is-@in) > 1 ) {                
                    throw new Exception(/*this.getClass() + */String.Format(" inconsistent index: items={0}, names={1}, capacity={2}", @is, @in, capacity));
                }
            }
            return (ulong)@in;
        } }
        
        /** this should always equal size(), but it's here for testing purposes */
        protected uint sizeItems() {
            uint t = 0;
            foreach (Level<E, K> l in level) {
                if (l!=null) {
                    t += l.size();
                }
            }
            return t;
        }
        

        /**
         * Get the average priority of Items
         *
         * @return The average priority of Items in the bag
         */
        // commented because not defined in Bag
        // @Override
        //public float getAveragePriority() {
        //    if (size() == 0) {
        //        return 0.01f;
        //    }
        //    float f = (float) mass / (size());
        //    if (f > 1) {
        //        return 1.0f;
        //    }
        //    return f;
        //}

        /**
         * Get an Item by key
         *
         * /param key The key of the Item
         * /return The Item with the given key
         */
        override public E referenceByKey(K key) {
            return nameTable[key];
        }
    
    
        /** look for a non-empty level */
        protected void nextNonEmptyLevel() {
            uint cl = currentLevel;
            do {} while (levelEmpty[cl = (uint)DISTRIBUTOR[(levelIndex++) % DISTRIBUTOR.Length]]);
            currentLevel = cl;  
            if (currentLevel < fireCompleteLevelThreshold) { // for dormant levels, take one item
                currentCounter = 1;
            } else {                  // for active levels, take all current items
                currentCounter = getNonEmptyLevelSize(currentLevel);
            }
        }

        public override E reference() {
            if (size == 0) {// empty bag
                return null;             
            }
            E e = takeNext();
            putIn(e);
            return e;        
        }
        
        public E peekNextWithoutAffectingBagOrder() {    
            if (size == 0) return null; // empty bag                
            if (levelEmpty[currentLevel] || (currentCounter == 0)) { // done with the current level
                nextNonEmptyLevel();
            }
            return level[currentLevel].peekFirst();        
        }
        
        public override E takeNext() {
            if (size == 0) {
                return null; // empty bag                
            }
            if (levelEmpty[currentLevel] || (currentCounter == 0)) { // done with the current level
                nextNonEmptyLevel();
            }
            if (levelEmpty[currentLevel]) {
                throw new Exception("Empty level selected for takeNext");
            }
            E selected = takeOutFirst(currentLevel); // take out the first item in the level
            currentCounter--;        
            return selected;
        }

        public uint getNonEmptyLevelSize(uint level) {
            return this.level[level].size();
        }

        public uint getLevelSize(uint level) {
            return (levelEmpty[level]) ? 0 : this.level[level].size();
        }

        public override E take(K name) {
            E oldItem;
            bool isPresent = nameTable.TryGetValue(name, out oldItem);
            if( !isPresent ) {
                return null;
            }
            nameTable.Remove(name);

            uint expectedLevel = getLevel(oldItem);
            // TODO scan up/down iteratively, it is likely to be near where it was
            if (!levelEmpty[expectedLevel]) {
                if (level[expectedLevel].remove(oldItem)) {                
                    removeMass(oldItem);
                    return oldItem;
                }            
            }
            for (int l = 0; l < levels; l++) {
                if ((!levelEmpty[l]) && (l!=expectedLevel)) {
                    if (level[l].remove(oldItem)) {
                        removeMass(oldItem);
                        return oldItem;
                    }
                }
            }
            //If it wasn't found, it probably was removed already.  So this check is probably not necessary
                //search other levels for this item because it's not where we thought it was according to getLevel()
            if (Parameters.DEBUG) {
                uint ns = (uint)nameTable.Count;
                uint @is = sizeItems();
                if (ns == @is)
                    return null;
                throw new Exception(String.Format("LevelBag inconsistency: {0}|{1}  Can not remove missing element: size inconsistency {2}", nameTable.Count ,sizeItems(), oldItem));
            }
            return oldItem;
        }

        /**
         * Decide the put-in level according to priority
         *
         * /param item The Item to put in
         * /return The put-in level
         */
        private uint getLevel(E item) {
            float fl = item.budget.priority * levels;
            int level = (int) Math.Ceiling(fl) - 1;
            if (level < 0) return 0;
            if (level >= levels) return levels-1;
            return (uint)level;
        }
        
        protected override E addItem(E newItem) {
            E oldItem = null;
            uint inLevel = getLevel(newItem);
            if (size >= capacity) {      // the bag will be full after the next 
                uint outLevel = 0;
                while (levelEmpty[outLevel]) {
                    outLevel++;
                }
                if (outLevel > inLevel) {           // ignore the item and exit
                    return newItem;
                } else {                            // remove an old item in the lowest non-empty level
                    oldItem = takeOutFirst(outLevel);
                }
            }
            ensureLevelExists(inLevel);
            level[inLevel].add(newItem);        // FIFO
            nameTable[newItem.name] = newItem;        
            addMass(newItem);
            return oldItem;
        }

        protected void ensureLevelExists(uint level) {
            if (this.level[level] == null) {
                this.level[level] = newLevel(level);
            }
        }

        /**
         * Take out the first or last E in a level from the itemTable
         *
         * @param level The current level
         * @return The first Item
         */
        private E takeOutFirst(uint level) {
            E selected = this.level[level].removeFirst();
            if (selected!=null) {
                nameTable.Remove(selected.name);
                removeMass(selected);
            }
            else {
                throw new Exception("Attempt to remove item from empty level: " + level);
            }
            return selected;
        }

        protected void removeMass(E item) {
            mass -= item.budget.priority;
        }
        protected void addMass(E item) {
            mass += item.budget.priority;
        }
         
        // commented because not in Bag defined
        //@Override
        //public float getMass() {
        //    return mass;
        //}

        public float getAverageItemsPerLevel() {
            return capacity / levels;
        }

        public float getMaxItemsPerLevel() {
            uint max = getLevelSize(0);
            for (uint i = 1; i < levels; i++) {
                uint s = getLevelSize(i);
                if (s > max) {
                    max = s;
                }
            }
            return max;
        }

        public float getMinItemsPerLevel() {
            uint min = getLevelSize(0);
            for (uint i = 1; i < levels; i++) {
                uint s = getLevelSize(i);
                if (s < min) {
                    min = s;
                }
            }
            return min;
        }

        public override void setMaxSize(ulong size) {
            throw new NotImplementedException();
        }

        public class Enumerator : IEnumerator<E> {
            private LevelBag<E, K> parent;

            int l;
            private IEnumerator<E> levelIterator;
            private E next;
            int size;
            int count = 0;

            public E Current { get {
                return levelIterator.Current;
            } }

            object IEnumerator.Current => Current;

            public Enumerator(LevelBag<E, K> parent) {
                this.parent = parent;

                l = parent.level.Length - 1;
                size = (int)parent.size;
            }

            public void Dispose() {
            }

            public bool MoveNext() {
                bool hasNext = levelIterator.MoveNext();
                if( !hasNext ) {
                    levelIterator = null;
                }

                if (l >= 0 && levelIterator == null) {
                    while (parent.levelEmpty[l]) {
                        if (--l == -1)
                            return false; //end of the levels
                    }
                    levelIterator = parent.level[l].GetEnumerator();
                }

                if (levelIterator == null) {
                    return false;
                }

                count++;
                if (hasNext) {
                    return true;
                }
                else {
                    levelIterator = null;
                    l--;
                    return count <= size;
                }
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public override IEnumerator<E> GetEnumerator() {
            return new Enumerator(this);
        }

        // commented because not in Bag defined
        //@Override
        //public int getCapacity() {
        //    return capacity;
        //}

        // commented because from OpenNARS
        //public Iterable<E> getLevel(final int i) {
        //    if (level[i] == null) {
        //        return Collections.EMPTY_LIST;
        //    }
        //    return level[i];
        //}

        
        public uint numberOfEmptyLevels {
            get {
                uint empty = 0;
                for (int i = 0; i < level.Length; i++) {
                    if( levelEmpty[i] ) {
                        empty++;
                    }
                }
                return empty;
        } }
    }
}
