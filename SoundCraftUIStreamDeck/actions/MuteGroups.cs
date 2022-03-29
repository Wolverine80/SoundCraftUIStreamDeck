using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using UIControl;

namespace SoundCraftUIStreamDeck
{
    [PluginActionId("org.m-a-b.soundcraftuimutegroup")]
    public class MuteGroups : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    MuteGroup = string.Empty,
                    PTT = false
                };
                return instance;
            }
            [JsonProperty(PropertyName = "muteGroup")]
            public string MuteGroup { get; set; }
            [JsonProperty(PropertyName = "ptt")]
            public bool PTT { get; set; }
        }

        #region Private Members

        private PluginSettings settings;
        private Enums.MuteFLags GSettingsMute;

        #endregion
        public MuteGroups(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            ConnectionClass.MuteEvent += OnMuteEvent;
            Enum.TryParse(settings.MuteGroup, out GSettingsMute);
        }


        public override void Dispose()
        {
            ConnectionClass.MuteEvent -= OnMuteEvent;
            ConMan.ClientActive(false);
        }

        public async override void KeyPressed(KeyPayload payload)
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            if (settings.MuteGroup != string.Empty)
            {
                if (ConvertMuteFlags() == true)
                {
                    ConMan.client.ChangeMuteGroup(ConnectionClass.MuteGroupVal - (uint)GSettingsMute);
                }
                else
                {   if (settings.PTT == false)
                    {
                        ConMan.client.ChangeMuteGroup(ConnectionClass.MuteGroupVal + (uint)GSettingsMute);
                    }
                }
            }
        }

        public async override void KeyReleased(KeyPayload payload) 
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            if (settings.MuteGroup != string.Empty)
            {
                if ( settings.PTT == true)
                {
                    ConMan.client.ChangeMuteGroup(ConnectionClass.MuteGroupVal + (uint)GSettingsMute);
                }
            }
        }
        public async override void OnTick()
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            if (ConvertMuteFlags())
            {
                await Connection.SetStateAsync(1);
            }
            else
            {
                await Connection.SetStateAsync(0);
            }
        }
        private async void OnMuteEvent()
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            if (ConvertMuteFlags())
            {
                await Connection.SetStateAsync(1);
            }
            else
            {
                await Connection.SetStateAsync(0);
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

        private bool ConvertMuteFlags()
        {
            Enums.MuteFLags MuteGroupFlags = (Enums.MuteFLags)ConnectionClass.MuteGroupVal;
            if (MuteGroupFlags.HasFlag(GSettingsMute))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }

}