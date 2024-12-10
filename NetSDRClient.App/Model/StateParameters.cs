using System.Runtime.InteropServices;

namespace NetSDRClientApp.Model
{
    public enum ControlItemType
    {
        SetState = 0x0018,
        SetFrequency = 0x0020
    }

    public enum DataType
    {
        Real = 0,
        Complex = 1
    }

    public enum StateType
    {
        Run = 0x02,
        Idle = 0x01
    }

    public enum CaptureMode
    {
        Continuos16bit = 0x00,
        Continuos24bit = 0x80,
        FIFO16bit = 0x01,
        Pulse24bit = 0x83,
        Pulse16bit = 0x03
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StateParameters
    {
        public byte ChannelType;
        public byte StateType;
        public byte CaptureType;
        public byte NumberSamples;
    }
}
