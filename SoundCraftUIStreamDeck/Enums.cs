using System;

namespace SoundCraftUIStreamDeck
{
    class Enums
    {

        [Flags]
        public enum MuteFLags
        {
            MuteGroup1 = 1,
            MuteGroup2 = 2,
            MuteGroup3 = 4,
            MuteGroup4 = 8,
            MuteGroup5 = 16,
            MuteGroup6 = 32,
            MuteTemp = 2097152,
            MuteFX = 4194304,
            MuteAll = 8388608,
        }
    }
}
