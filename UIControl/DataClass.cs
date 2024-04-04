using System;
using System.Globalization;
using System.Threading.Tasks;
using UIControl.enums;
using WatsonWebsocket;

namespace UIControl
{
    public class DataClass : IDataClass
    {
        readonly WatsonWsClient ws = ConnectionClass.MixConnection;

        public DataClass()
        {
            ws.ServerDisconnected += EventOnClose;
        }
        private void EventOnClose(object Sender, EventArgs e)
        {
            ws.ServerDisconnected -= EventOnClose;
        }
        public void ChangeCue(string Show, string Cue)
        {
            if (ConnectionClass.isOpen)
            {
                ws.SendAsync($"3:::LOADCUE^{ Show }^{ Cue  }");
            }
        }

        public void ChangeMuteGroup(uint mgroup)
        {
            if (ConnectionClass.isOpen)
            {
                ws.SendAsync($"3:::SETD^mgmask^{ mgroup }");
                ConnectionClass.MuteGroupVal = mgroup;
            }
        }

        public void ChangeSnapshot(string Show, string Snapshot)
        {
            if (ConnectionClass.isOpen)
            {
                ws.SendAsync($"3:::LOADSNAPSHOT^{ Show }^{ Snapshot }");
            }
        }

        public void ClearSolo(dataclasses.Channel[][] Channels)
        {
            for (byte o = 0; o < Channels.GetLength(0); o++)
            {
                for (byte i = 0; i < Channels[o].GetLength(0); i++)
                {
                    if (Channels[o][i].Settings.Solo)
                    {
                        ws.SendAsync($"3:::SETD^{ (Channeltype)o }.{ i }.solo^{ 0 }");
                        Channels[o][i].Settings.Solo = false;
                    }
                    if (Channels[o][i].Settings.Stereo && i % 2 == 0)
                    {
                        ws.SendAsync($"3:::SETD^{ (Channeltype)o }.{ i + 1 }.solo^{ 0 }");
                        Channels[o][i + 1].Settings.Solo = false;
                    }

                }
            }
        }

        public void MuteChannel(Channeltype channeltype, int channel, bool on)
        {
            if (ConnectionClass.isOpen)
            {
                byte onoff = 0;
                if (on) { onoff = 1; }
                ws.SendAsync($"3:::SETD^{ channeltype}.{ channel }.mute^{ onoff }");
                if (ConnectionClass.Channels[(byte)channeltype][channel].Settings.Stereo && channel % 2 == 0)
                {
                    Task.Delay(10).Wait();
                    ws.SendAsync($"3:::SETD^{ channeltype}.{ channel + 1 }.mute^{ onoff }");
                }
            }
        }

        public void SoloChannel(Channeltype channeltype, int channel, bool on)
        {
            if (ConnectionClass.isOpen)
            {
                byte onoff = 0;
                if (on) { onoff = 1; }
                ws.SendAsync($"3:::SETD^{ channeltype }.{ channel }.solo^{ onoff }");
                if (ConnectionClass.Channels[(byte)channeltype][channel].Settings.Stereo && channel % 2 == 0)
                {
                    Task.Delay(10).Wait();
                    ws.SendAsync($"3:::SETD^{ channeltype }.{ channel + 1 }.solo^{ onoff }");
                }
            }
        }

        public void ChangeVolume(Channeltype channeltype, int channel, double volume)
        {
            ws.SendAsync($"3:::SETD^{channeltype}.{channel}.mix^{volume.ToString(CultureInfo.InvariantCulture)}");
            ConnectionClass.Channels[(byte)channeltype][channel].Settings.Volume= volume;
            if (ConnectionClass.Channels[(byte)channeltype][channel].Settings.Stereo && channel % 2 == 0)
            {
                ConnectionClass.Channels[(byte)channeltype][channel + 1].Settings.Volume = volume;
                ws.SendAsync($"3:::SETD^{channeltype}.{channel + 1}.mix^{volume.ToString(CultureInfo.InvariantCulture)}");
            }
        }
        public void ChangeVolume(Channeltype channeltype, double volume)
        {
            ws.SendAsync($"3:::SETD^{channeltype}.mix^{volume.ToString(CultureInfo.InvariantCulture)}");
            ConnectionClass.Channels[(byte)channeltype][0].Settings.Volume = volume;
        }
        public void Dim(bool on)
        {
            if (ConnectionClass.isOpen)
            {
                byte onoff = 0;
                if (on) { onoff = 1; }
                ws.SendAsync($"3:::SETD^m.dim^{ onoff }");
            }

        }

        public void SoloPrep(bool on)
        {
            if (on)
            {
                ws.SendAsync($"3:::SETS^hwouthp.0.src^hp.0");
                ws.SendAsync($"3:::SETS^hwouthp.1.src^hp.1");
            } else
            {
                ws.SendAsync($"3:::SETS^hwouthp.0.src^a.8");
                ws.SendAsync($"3:::SETS^hwouthp.1.src^a.9");
            }
        }
    }
}
