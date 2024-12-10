using System.Runtime.InteropServices;

namespace NetSDRClientApp.Model
{
    public enum ChannelId
    {
        Channel1 = 0x00,
        Channel2 = 0x02
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SetFrequencyParameters
    {
        public byte ChannelId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] Frequency;
    }
}
