using Microsoft.Extensions.Configuration;
using System.Timers;
using UIControl;
using Timer = System.Timers.Timer;

namespace SoundCraftUIStreamDeck
{
    public sealed class ConMan
    {
        readonly string mixer = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("appSettings")["mixerurl"];
        // readonly string mixer = ConfigurationManager.AppSettings.Get("mixerurl");
        private static ConMan instance = null;
        private static readonly object objLock = new object();
        public static DataClass client;
        private readonly Timer tmrCheckDirty;

        public static int ActiveClients { get; private set; } = 0;



        public bool IsConnected { get; private set; } = false;
        ConMan()
        {
            ConnectionClass.ConnectionOpen += IsOpenEvent;
            ConnectionClass.ConnectionClose += IsCloseEvent;
            ConnectionClass.InitMixer(mixer);
            ConnectionClass.OpenMixer();
            client = new DataClass();
            tmrCheckDirty = new Timer();
            tmrCheckDirty.Elapsed += TmrCheckDirty_Elapsed;
            tmrCheckDirty.Interval = 1000;
            tmrCheckDirty.Start();
        }

        private void TmrCheckDirty_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ActiveClients > 0)
            {
                ConnectionClass.KeepAlive();
                if (instance.IsConnected == false)
                {
                    KillInstance();
                }
            }
        }
        private void KillInstance()
        {
            ConnectionClass.CloseMixer();
            instance = null;

        }

        public static ConMan Instance
        {
            get
            {
                if (instance is not null)
                {
                    return instance;
                }

                lock (objLock)
                {
                    if (instance is null)
                    {
                        instance = new ConMan();
                    }
                    return instance;
                }
            }
        }
        public static void ClientActive(bool state)
        {
            switch (state)
            {
                case true:
                    ActiveClients++;
                    break;
                case false:
                    if (ActiveClients > 0) { ActiveClients--; } else { ActiveClients = 0; }
                    break;
            }

        }

        private void IsOpenEvent()
        {
            IsConnected = true;
        }
        private void IsCloseEvent()
        {
            IsConnected = false;
        }

    }
}
