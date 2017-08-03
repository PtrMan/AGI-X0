using ChatSharp;
using System.Linq;
using MetaNix.framework.logging;
using System;

namespace MetaNixExperimentalPrivate {
    class IrcLogger : ILogger {
        LogDelegateType logDelegate;

        public delegate void LogDelegateType(string message);

        public IrcLogger(LogDelegateType logDelegate) {
            this.logDelegate = logDelegate;
        }

        public void write(Logged logged) {
            if( logged.notifyConsole != Logged.EnumNotifyConsole.YES )   return;

            logDelegate(logged.message);
        }
    }

    public class IrcEntry {
        IrcClient client;


        string channelToJoin = "#netention";

        void logToIrc(string message) {
            client.SendMessage(message, channelToJoin);
        }

        public void entry2(MultiSinkLogger log) {
            log.sinks.Add(new IrcLogger(logToIrc));

            // from irc in #nars
            // oh big number zealand or math in the united states and canada . nickname : BNZ or BNZea
            string nick = "BNZea";
            string user = nick;
            

            client = new IrcClient("irc.freenode.net", new IrcUser(nick, user));

            client.ConnectionComplete += (s, e) => client.JoinChannel(channelToJoin);

            client.ChannelMessageRecieved += (s, e) =>
            {
                var channel = client.Channels[e.PrivateMessage.Source];

                if (e.PrivateMessage.Message == ".list")
                    channel.SendMessage(string.Join(", ", channel.Users.Select(u => u.Nick)));

                string message = e.PrivateMessage.Message;

                client.SendMessage("ECHO", channelToJoin);
            };

            bool isConnected = false;

            client.ConnectAsync();
            client.ConnectionComplete += (s, e) => {
                isConnected = true;
            };

            // wait till connected
            for(;;) {
                if( isConnected ) {
                    break;
                }
            }
        }
    }
}
