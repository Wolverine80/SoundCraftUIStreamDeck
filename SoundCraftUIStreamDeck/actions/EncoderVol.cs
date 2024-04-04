using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UIControl;
using UIControl.enums;

namespace SoundCraftUIStreamDeck
{
    [PluginActionId("org.m-a-b.soundcraftuiencodervol")]
    public class EncoderVol : EncoderBase
    {
        private bool MuteOn { get; set; } = false;
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Channeltype = UIControl.enums.Channeltype.i,
                    Channel = 0,
                    MuteSolo = "mute",
                    Title = "Volume",
                    StepSize = 1
                };
                return instance;
            }
            [JsonProperty(PropertyName = "mutesolo")]
            public string MuteSolo { get; set; }
            [JsonProperty(PropertyName = "channeltype")]
            public UIControl.enums.Channeltype Channeltype { get; set; }
            [JsonProperty(PropertyName = "channel")]
            public byte Channel { get; set; }
            public string Title { get; set; }
            [JsonProperty(PropertyName = "stepSize")]
            public int StepSize { get; set; }
        }

        #region Private Members

        private PluginSettings settings;
        private const double MIN_DB_VALUE = -103.0;
        private const double MAX_DB_VALUE = 10.0;


        #endregion
        public EncoderVol(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings is null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            ConMan.ClientActive(true);
        }


        public override void Dispose()
        {
            ConMan.ClientActive(false);
        }
        public override void TouchPress(TouchpadPressPayload payload)
        {
            double outputVolume = DBLinearConversion.LookupLinearValue(0);
            switch (settings.Channeltype)
            {
                case Channeltype.m:
                    ConMan.client.ChangeVolume(settings.Channeltype, outputVolume);
                    break;
                default:
                    ConMan.client.ChangeVolume(settings.Channeltype, settings.Channel, outputVolume);
                    if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Stereo && settings.Channel % 2 == 0)
                    {
                        ConMan.client.ChangeVolume(settings.Channeltype, settings.Channel + 1, outputVolume);
                    }
                    break;
            }
            UpdateScreen(outputVolume);
        }

        public async override void OnTick()
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            //Logger.Instance.LogMessage(TracingLevel.ERROR, "Type: " + settings.Channeltype + " Channel: " + settings.Channel + "Setting: " + ConnectionClass.Channels[(int)settings.Channeltype, settings.Channel].Settings.Mute + " Setting: " + settings.MuteSolo + " MuteOn: " + MuteOn);
            double volume = (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Volume);

            UpdateScreen(volume);

        }

        public async void UpdateScreen(double volume)
        {
            double DBvolume = DBLinearConversion.LookupDBValue(volume);
            Dictionary<string, string> dkv = new Dictionary<string, string>();
            dkv["title"] = String.IsNullOrEmpty(settings.Title) ? "Volume" : settings.Title;
            dkv["value"] = DBLinearConversion.VtoDB(volume);
            dkv["indicator"] = Tools.RangeToPercentage((int)DBvolume, (int)MIN_DB_VALUE, (int)MAX_DB_VALUE).ToString();

            if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Mute)
            {
                MuteOn = true;
                dkv["value"] = "Muted";
            }
            else
            {
                MuteOn = false;
            }
            await Connection.SetFeedbackAsync(dkv);

        }

        public async override void DialRotate(DialRotatePayload payload)
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            double volume = (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Volume);
            double DBvolume = DBLinearConversion.LookupDBValue(volume);
            double increment = payload.Ticks;
            if (payload.Ticks > 1 || payload.Ticks < -1 ) {
                increment *= settings.StepSize;
            }
            if (DBvolume > -20.0)
            {
                increment /= 10;
            }
            double newvolume = DBvolume + increment;
            newvolume = Math.Max(MIN_DB_VALUE, newvolume);
            newvolume = Math.Min(MAX_DB_VALUE, newvolume);
            double outputVolume = DBLinearConversion.LookupLinearValue(newvolume);

            switch (settings.Channeltype)
            {
                case Channeltype.m:
                    ConMan.client.ChangeVolume(settings.Channeltype, outputVolume);
                    break;
                default:
                    ConMan.client.ChangeVolume(settings.Channeltype, settings.Channel, outputVolume);
                    if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Stereo && settings.Channel % 2 == 0)
                    {
                        ConMan.client.ChangeVolume(settings.Channeltype, settings.Channel + 1, outputVolume);
                    }
                    break;
            }
            UpdateScreen(outputVolume);
              //  Logger.Instance.LogMessage(TracingLevel.ERROR, "Sound level changed:" + outputVolume);
          

        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        public override void DialDown(DialPayload payload)
        {
         
        }

        public async override void DialUp(DialPayload payload)
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }

            ConMan.client.MuteChannel(settings.Channeltype, settings.Channel, !MuteOn);
            ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Mute = !MuteOn;
            if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Stereo && settings.Channel % 2 == 0)
            {
                ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel + 1].Settings.Mute = !MuteOn;
            }
            //Logger.Instance.LogMessage(TracingLevel.ERROR, "Mute pressed");
        }
        #endregion
    }

}