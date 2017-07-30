using System;
using System.Reflection;

namespace MetaNix.framework.misc {
    public static class PathHelper {
        // used to load and store files in and from the directory of the executed code
        // from http://stackoverflow.com/a/283917
        public static UriBuilder AssemblyDirectory {
            get {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                return uri;
            }
        }
    }
}
