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
                    MuteGroup = string.Empty
                };
                return instance;
            }
            [JsonProperty(PropertyName = "muteGroup")]
            public string MuteGroup { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

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
                Enum.TryParse(settings.MuteGroup, out Enums.MuteFLags SettingsMute);

                if (ConvertMuteFlags())
                {
                    ConMan.client.ChangeMuteGroup(ConnectionClass.MuteGroupVal - (uint)SettingsMute);
                }
                else
                {
                    ConMan.client.ChangeMuteGroup(ConnectionClass.MuteGroupVal + (uint)SettingsMute);
                }
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
            Enum.TryParse(settings.MuteGroup, out Enums.MuteFLags SettingsMute);
            Enums.MuteFLags MuteGroupFlags = (Enums.MuteFLags)ConnectionClass.MuteGroupVal;
            if (MuteGroupFlags.HasFlag(SettingsMute))
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