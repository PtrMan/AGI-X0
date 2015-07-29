package automenta.vivisect;

import com.gs.collections.impl.map.mutable.UnifiedMap;
import com.gs.collections.impl.set.mutable.UnifiedSet;

import java.util.Map;
import java.util.Set;

public class Global {
    /** use this for advanced error checking, at the expense of lower performance.
     it is enabled for unit tests automatically regardless of the value here.    */
    public static boolean DEBUG = false;

    public static final int METRICS_HISTORY_LENGTH = 256;

    public static <K, V> Map<K,V> newHashMap(int capacity) {
        //return new FastMap<>(); //javolution http://javolution.org/apidocs/javolution/util/FastMap.html
        return new UnifiedMap<K,V>(capacity);
        //return new HashMap<>(capacity);
        //return new LinkedHashMap(capacity);
    }

    public static <X> Set<X> newHashSet(int capacity) {
        return new UnifiedSet(capacity);
        //return new SimpleHashSet(capacity);
        //return new HashSet(capacity);
        //return new LinkedHashSet(capacity);
    }
}
