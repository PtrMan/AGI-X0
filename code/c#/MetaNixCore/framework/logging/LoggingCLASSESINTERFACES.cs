using System;
using System.Collections.Generic;
using System.IO;

namespace MetaNix.framework.logging {
    
    public class Logged {
        public enum EnumServerity {
            INFO,
            WARNING,
            RECOVERABLEERROR,
            FATAL, // leads to immediate program termination
        }

        // should the console receive and display the message
        public enum EnumNotifyConsole {
            YES,
            NO,
        }

        public EnumNotifyConsole notifyConsole;
        public EnumServerity serverity;
        public string[] origin; // treaded like a path
        public string message;
    }
    
    public interface ILogger {
        void write(Logged logged);
    }

    public class MultiSinkLogger : ILogger {
        public IList<ILogger> sinks = new List<ILogger>();

        public void write(Logged logged) {
            foreach( var iSink in sinks )   iSink.write(logged);
        }
    }

    public class FileLogger : ILogger {
        public void open(string filename) {
            stream = new StreamWriter(filename, append: true);
        }

        public void close() {
            flush();
            stream.Close();
            stream = null;
        }
        
        void flush() {
            stream.Flush();
        }

        public void write(Logged logged) {
            string formated = logged.serverity.ToString() + "-" + String.Join(".", logged.origin) + ": " + logged.message;

            if( logged.notifyConsole == Logged.EnumNotifyConsole.YES )   Console.WriteLine(formated);

            stream.WriteLine(formated);
            flush();
        }

        StreamWriter stream;
    }


}
