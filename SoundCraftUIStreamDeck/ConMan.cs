using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UIsocket;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SoundCraftUIStreamDeck
{
    public sealed class ConMan
    {
        const string mixer = "ws://192.168.1.8/socket.io/1/websocket";
        private static ConMan instance = null;
        private static readonly object objLock = new object();
        public static DataClass client;
        private readonly Timer tmrCheckDirty;

        public bool IsConnected { get; private set; }
        ConMan()
        {
            ConnectionClass.InitMixer(mixer);
            ConnectionClass.OpenMixer();
            client = new DataClass();
            IsConnected = true;
            tmrCheckDirty = new Timer();
            tmrCheckDirty.Elapsed += TmrCheckDirty_Elapsed;
            tmrCheckDirty.Interval = 1000;
            tmrCheckDirty.Start();
        }

        private void TmrCheckDirty_Elapsed(object sender, ElapsedEventArgs e)
        {
            // IsConnected = (client.Poll() >= 0);
            ConnectionClass.KeepAlive();
            if ( ConnectionClass.isOpen == false )
            {
                ConnectionClass.OpenMixer();
                IsConnected = true;
            } 
        }

        public static ConMan Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (objLock)
                {
                    if (instance == null)
                    {
                        instance = new ConMan();
                    }
                    return instance;
                }
            }
        }
    }
}
