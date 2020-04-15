using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WebSocketSharp;

namespace UIsocket
{
    public static class ConnectionClass
    {
        public static WebSocket MixConnection { get; set; }
        public static bool isOpen;
        public static void InitMixer(string URL)
        {
            MixConnection = new WebSocket(URL);
            MixConnection.OnOpen += EventOnOpen;
            MixConnection.OnClose += EventOnClose;
        }

        public static void OpenMixer()
        {
            if ( null != MixConnection)
            {
                MixConnection.ConnectAsync();
                Thread.Sleep(100);
            }

        }

        public static void KeepAlive()
        {
            MixConnection.Send($"3:::ALIVE");
        }
         
        public static void CloseMixer()
        {
            if ( isOpen )
            {
                MixConnection.CloseAsync();
                isOpen = false;
            }
        }

        private static void EventOnOpen(object Sender, EventArgs e)
        {
            isOpen = true;
        }

        private static void EventOnClose(object Sender, EventArgs e)
        {
            isOpen = false;
        }

    }
}
