using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UIControl;

namespace SoundCraftUIStreamDeck
{
    [PluginActionId("org.m-a-b.soundcraftuidivbuttons")]
    public class DivButtons : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    DivButtons = string.Empty
                };
                return instance;
            }
            [JsonProperty(PropertyName = "divButtons")]
            public string DivButtons { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public DivButtons(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (settings.DivButtons != string.Empty)
            {
                switch (settings.DivButtons)
                {
                    case "mDim":
                        ConMan.client.Dim(!ConnectionClass.Dim);
                        ConnectionClass.Dim = !ConnectionClass.Dim;
                        break;
                    case "SoloPrep":
                        ConMan.client.SoloPrep(!ConnectionClass.SoloPrep);
                        ConnectionClass.SoloPrep = !ConnectionClass.SoloPrep;
                        break;
                    default:
                        break;
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
            switch (settings.DivButtons)
            {
                case "mDim":
                    if (ConnectionClass.Dim)
                    {
                        await Connection.SetStateAsync(0);
                    }
                    else
                    {
                        await Connection.SetStateAsync(1);
                    }
                    break;
                case "SoloPrep":
                    if (ConnectionClass.SoloPrep)
                    {
                        await Connection.SetStateAsync(0);
                    }
                    else
                    {
                        await Connection.SetStateAsync(1);
                    }
                    break;
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