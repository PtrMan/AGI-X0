using System.Collections.Generic;

namespace MetaNix {
    public interface IObserver {
        void notify(params object[] arguments);
    }

    public class Observable {
        public void register(IObserver observer) {
            observers.Add(observer);
        }

        public void unregister(IObserver observer) {
            observers.Remove(observer);
        }

        public void notify(params object[] arguments) {
            foreach( IObserver iterationObserver in observers ) {
                iterationObserver.notify(arguments);
            }
        }

        IList<IObserver> observers = new List<IObserver>();
    }

}
