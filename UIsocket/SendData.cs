using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace UIsocket
{
    public class DataClass
    {
        readonly WebSocket ws = ConnectionClass.MixConnection;
        public string Snapshot { get; set; } = "";
        public string Show { get; set; } = "Manu";
        public string Cue { get; set; } = "";

        public DataClass()
        {
                ws.OnError += EventOnError;
                ws.OnClose += EventOnClose;
                ws.OnMessage += EventAllOnMessage;
        }

        private void EventOnMessage(object Sender, MessageEventArgs e)
        {
            if (e.Data == "3:::MSG^$SNAPLOAD^Phone")
            {
          //      Console.WriteLine("Mixer says:" + e.Data);
            }
        }

        private void EventOnError(object Sender, ErrorEventArgs e)
        {
            //       Console.WriteLine("Mixer Error:" + e.Message);
            ConnectionClass.CloseMixer();
        }

        private void EventOnClose(object Sender, CloseEventArgs e)
        {
         //   Console.WriteLine("Mixer Closed:" + e.Reason);
            ws.OnError -= EventOnError;
            ws.OnClose -= EventOnClose;
            ws.OnMessage -= EventAllOnMessage;
            //   Console.WriteLine(counter.ToString());
        }

        public void ChangeSnapshot(string Show, string Snapshot)
        {
            if (ConnectionClass.isOpen)
            {
                ws.SendAsync($"3:::LOADSNAPSHOT^{ Show }^{ Snapshot }", completed);
            }
        }

        private void completed(bool obj)
        {
            if (obj)
            {
                Console.WriteLine("Messeage sent.");
            }
        }

        public void GetMessages()
        {
            ws.SendAsync("3:::LISTSNAPSHOTS", completed);
        }

        public void ChangeCue(string Show, string Cue)
        {
            if (ConnectionClass.isOpen)
            {
                ws.SendAsync($"3:::LOADCUE^{ Show }^{ Cue  }", completed);
            }
        }

        private void EventAllOnMessage(object Sender, MessageEventArgs e)
        {
            // Console.WriteLine("Mixer says:" + e.Data);
            switch (e.Data)
            {
                case "3:::MSG^$SNAPLOAD^Phone":
                    Snapshot = "Phone";
                    break;
                case "3:::MSG^$SNAPLOAD^Boxen":
                    Snapshot = "Boxen";
                    break;
                default:
                    break;
            }
        }

        public bool CheckState(string showName, string snapshotName, string cueName)
        {
            // ws.SendAsync("3:::LISTSNAPSHOTS", completed);
            if (showName == Show && snapshotName == Snapshot && cueName == Cue) return true;
            else return false;
            
        }
    }
}
