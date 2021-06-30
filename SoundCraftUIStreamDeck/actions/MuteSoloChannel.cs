using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UIControl;

namespace SoundCraftUIStreamDeck
{
    [PluginActionId("org.m-a-b.soundcraftuimutesolochannel")]
    public class MuteSoloChannel : PluginBase
    {
        private bool MuteOn { get; set; } = false;
        private bool SoloOn { get; set; } = false;
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Channeltype = UIControl.enums.Channeltype.i,
                    Channel = 0,
                    MuteSolo = "mute"
                };
                return instance;
            }
            [JsonProperty(PropertyName = "mutesolo")]
            public string MuteSolo { get; set; }
            [JsonProperty(PropertyName = "channeltype")]
            public UIControl.enums.Channeltype Channeltype { get; set; }
            [JsonProperty(PropertyName = "channel")]
            public byte Channel { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public MuteSoloChannel(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

        public async override void KeyPressed(KeyPayload payload)
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }


            if (settings.MuteSolo == "mute")
            {
                ConMan.client.MuteChannel(settings.Channeltype, settings.Channel, !MuteOn);
                ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Mute = !MuteOn;
                if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Stereo && settings.Channel % 2 == 0)
                {
                    ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel + 1].Settings.Mute = !MuteOn;
                }
                //Logger.Instance.LogMessage(TracingLevel.ERROR, "Mute pressed");
            }
            else
            {
                ConMan.client.ClearSolo(ConnectionClass.Channels);
                ConMan.client.SoloChannel(settings.Channeltype, settings.Channel, !SoloOn);
                ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Solo = !SoloOn;
                if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Stereo && settings.Channel % 2 == 0)
                {
                    ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel + 1].Settings.Solo = !SoloOn;
                }
                //Logger.Instance.LogMessage(TracingLevel.ERROR, "Solo pressed");
            }

        }

        public override void KeyReleased(KeyPayload payload) { }
        public async override void OnTick()
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            //Logger.Instance.LogMessage(TracingLevel.ERROR, "Type: " + settings.Channeltype + " Channel: " + settings.Channel + "Setting: " + ConnectionClass.Channels[(int)settings.Channeltype, settings.Channel].Settings.Mute + " Setting: " + settings.MuteSolo + " MuteOn: " + MuteOn);
            if (settings.MuteSolo == "mute")
            {
                if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Mute)
                {
                    await Connection.SetStateAsync(0);
                    MuteOn = true;
                }
                else
                {
                    await Connection.SetStateAsync(1);
                    MuteOn = false;
                }
            }
            else
            {
                if (ConnectionClass.Channels[(int)settings.Channeltype][settings.Channel].Settings.Solo)
                {
                    await Connection.SetStateAsync(0);
                    SoloOn = true;
                }
                else
                {
                    await Connection.SetStateAsync(1);
                    SoloOn = false;
                }
            }
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

        #endregion
    }

}