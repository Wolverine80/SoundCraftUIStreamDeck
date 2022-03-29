using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UIControl.dataclasses;
using UIControl.enums;
using WatsonWebsocket;

namespace UIControl
{
    public delegate void ConnectionOpenEventHandler();
    public delegate void ConnectionCloseEventHandler();
    public delegate void MuteEventHandler();
    public static class ConnectionClass
    {
        public static event ConnectionOpenEventHandler ConnectionOpen;
        public static event ConnectionCloseEventHandler ConnectionClose;
        public static event MuteEventHandler MuteEvent;


        public static dataclasses.Channel[][] Channels { get; private set; }
        public static bool isOpen;
        public static string Snapshot { get; private set; } = "";
        public static string Show { get; private set; } = "";
        public static string Cue { get; private set; } = "";
        public static uint MuteGroupVal { get; set; }
        public static bool Dim { get; set; }
        public static bool SoloPrep { get; set; }

        public static WatsonWsClient MixConnection { get; set; }

        public static void InitMixer(string URL)
        {
            Uri MixerURI = new Uri(URL);
            Channels = new dataclasses.Channel[8][];
            Channels[0] = new Channel[24];
            Channels[1] = new Channel[2];
            Channels[2] = new Channel[2];
            Channels[3] = new Channel[4];
            Channels[4] = new Channel[6];
            Channels[5] = new Channel[10];
            Channels[6] = new Channel[6];
            Channels[7] = new Channel[2];

            for (byte o = 0; o < Channels.GetLength(0); o++)
            {
                for (byte i = 0; i < Channels[o].GetLength(0); i++)
                {

                    Channels[o][i] = new Channel(i, (Channeltype)o);
                }
            }
            MixConnection = new WatsonWsClient(MixerURI);
            MixConnection.ServerConnected += EventOnOpen;
            MixConnection.ServerDisconnected += EventOnClose;
            MixConnection.MessageReceived += EventOnMessage;
        }

        private static void EventOnMessage(object sender, MessageReceivedEventArgs e)
        {
            foreach (string Daten in Encoding.UTF8.GetString(e.Data).Split(Environment.NewLine.ToCharArray()))
            {

                if (Daten.StartsWith("2::")) { continue; }
                if (Daten.StartsWith("3:::RTA^") || Daten.StartsWith("3:::VU2^") || Daten.StartsWith("3:::VUA^")) { continue; }
                if (Daten.StartsWith("RTA^") || Daten.StartsWith("VU2^") || Daten.StartsWith("VUA^")) { continue; }

                if (Daten.Contains("3:::SNAPSHOTLIST^"))
                {
                    Show = MatchMessage(Encoding.UTF8.GetString(e.Data), 3);
                }
                if (Daten.Contains("SETS^var.currentSnapshot^"))
                {
                    Snapshot = MatchMessage(Daten, 1);
                }
                if (Daten.Contains("SETS^hwouthp.0.src^hp.0"))
                {
                    SoloPrep = true;
                } else
                if (Daten.Contains("SETS^hwouthp.0.src^a.8"))
                {
                    SoloPrep = false;
                }
                if (Daten.Contains("SETS^var.currentShow^"))
                {
                    Show = MatchMessage(Daten, 2);
                }
                if (Daten.Contains("SETS^var.currentCue^"))
                {
                    Cue = MatchMessage(Daten, 4);
                }
                if (Daten.Contains("SETD^m.dim^"))
                {
                    if (Regex.Match(Daten, @".*SETD\^m.dim\^(.*)").Groups[1].Value == "1")
                    {
                        Dim = true;
                    }
                    else
                    {
                        Dim = false;
                    }
                }
                if (Daten.Contains("SETD^mgmask"))
                {
                    if (uint.TryParse(MatchMessage(Daten, 5), out uint j))
                    {
                        MuteGroupVal = j;
                        MuteEvent?.Invoke();
                    }
                }
                var regMute = Regex.Match(Daten, @"SETD\^([ilpfsavm])\.([\d]{1,2})\.mute\^([0|1])");
                if (regMute.Success)
                {
                    byte channel = byte.Parse(regMute.Groups[2].Value);
                    Enum.TryParse(regMute.Groups[1].Value, out Channeltype channeltypeM);
                    bool onoff = byte.Parse(regMute.Groups[3].Value) == 1 ? true : false;
                    Channels[(int)channeltypeM][channel].Settings.Mute = onoff;
                }
                var regSolo = Regex.Match(Daten, @"SETD\^([ilpfsavm])\.([\d]{1,2})\.solo\^([0|1])");
                if (regSolo.Success)
                {
                    byte channel = byte.Parse(regSolo.Groups[2].Value);
                    Enum.TryParse(regSolo.Groups[1].Value, out Channeltype channeltypeS);
                    bool onoff = byte.Parse(regSolo.Groups[3].Value) == 1 ? true : false;
                    Channels[(int)channeltypeS][channel].Settings.Solo = onoff;
                }
                var regStereo = Regex.Match(Daten, @"SETD\^([ilpfsavm])\.([\d]{1,2})\.stereoIndex\^([-|0|1])");
                if (regStereo.Success)
                {
                    int channel = int.Parse(regStereo.Groups[2].Value);
                    Enum.TryParse(regStereo.Groups[1].Value, out Channeltype channeltypeStereo);
                    bool onoff;
                    if (regStereo.Groups[3].Value == "-")
                    {
                        onoff = false;
                    }
                    else
                    {
                        onoff = true;
                    }
                    Channels[(int)channeltypeStereo][channel].Settings.Stereo = onoff;
                }
            }
        }

        private static string MatchMessage(string Data, int art)
        {
            string m;
            switch (art)
            {
                case 1:
                    m = Regex.Match(Data, @"(.*SETS\^var\.currentSnapshot\^)(.*)").Groups[2].Value;
                    break;
                case 2:
                    m = Regex.Match(Data, @"(.*SETS\^var\.currentShow\^)(.*)").Groups[2].Value;
                    break;
                case 3:
                    m = Regex.Match(Data, @"(.*SNAPSHOTLIST\^)(.+?)\^.*").Groups[2].Value;
                    break;
                case 4:
                    m = Regex.Match(Data, @"(.*SETS\^var\.currentCue\^)(.*)").Groups[2].Value;
                    break;
                case 5:
                    m = Regex.Match(Data, @"(.*SETD\^mgmask\^)(.*)").Groups[2].Value;
                    break;
                default:
                    return "";
            }

            return m;

        }

        public static void OpenMixer()
        {
            if (null != MixConnection && MixConnection?.Connected == false)
            {
                MixConnection.StartWithTimeoutAsync(5);
                Task.Delay(100).Wait();
            }

        }

        public static void KeepAlive()
        {
            MixConnection.SendAsync($"3:::ALIVE");
        }

        public static void CloseMixer()
        {
            if (isOpen)
            {
                MixConnection.Stop();
                isOpen = false;
                MixConnection.Dispose();
            }
        }

        private static void EventOnOpen(object Sender, EventArgs e)
        {
            if (MixConnection.Connected == true)
            {
                isOpen = true;
                ConnectionOpen?.Invoke();
            }
        }

        private static void EventOnClose(object Sender, EventArgs e)
        {
            isOpen = false;
            ConnectionClose?.Invoke();
        }
    }
}
