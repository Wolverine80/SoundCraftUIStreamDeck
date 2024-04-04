//using System.Threading;
using UIControl.dataclasses;
using UIControl.enums;

namespace UIControl
{
    public interface IDataClass
    {
        void ChangeCue(string Show, string Cue);
        void ChangeMuteGroup(uint mgroup);
        void ChangeSnapshot(string Show, string Snapshot);
        void ClearSolo(Channel[][] Channels);
        void ChangeVolume(Channeltype channeltype, int channel, double volume);
        void MuteChannel(Channeltype channeltype, int channel, bool on);
        void SoloChannel(Channeltype channeltype, int channel, bool on);
        void SoloPrep(bool on);
        void Dim(bool on);
    }
}