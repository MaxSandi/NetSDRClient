using NetSDRClient.App.Model.MessageType;
using NetSDRClientApp.Model;
using System.Buffers;
using System.Runtime.InteropServices;

namespace NetSDRClientApp.Helpers
{
    public static class NetSDRHelper
    {
        public static (byte type, ushort length) ParseMessageHeader(Span<byte> header)
        {
            if (header.Length != 2)
                throw new Exception("Header length must be 2 byte!");

            var lengthLsb = header[0];
            var lengthMsb = (header[1] >> 3) & 0b11111;

            return ((byte)((header[1] >> 5) & 0x07), (ushort)((lengthMsb << 8) | lengthLsb));
        }

        public static byte[] StructToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] bytes = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, false);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return bytes;
        }

        #region Commands
        public static Memory<byte> PrepareSetFrequencyMessage(IMemoryOwner<byte> memoryOwner, ushort messageLength, ChannelId channelId, ulong frequency)
        {
            var messageMemory = memoryOwner.Memory[..messageLength];
            var message = messageMemory.Span;
            PrepareMessageHeader(message, (byte)HostMessageType.SetControlItem, messageLength); // 2 bytes
            PrepareMessageControlItem(message, ControlItemType.SetFrequency); // 2 bytes

            message[4] = (byte)channelId;
            message[5] = (byte)(frequency & 0xFF);
            message[6] = (byte)((frequency >> 8) & 0xFF);
            message[7] = (byte)((frequency >> 16) & 0xFF);
            message[8] = (byte)((frequency >> 24) & 0xFF);
            message[9] = (byte)((frequency >> 32) & 0xFF);

            return messageMemory;
        }

        private static void PrepareMessageHeader(Span<byte> message, byte type, ushort messageLength)
        {
            if (type < 0 || type > 7)
                throw new ArgumentOutOfRangeException(nameof(type));

            if (messageLength < 0 || messageLength > 0x1FFF)
                throw new ArgumentOutOfRangeException(nameof(messageLength), "Length must be from 0 to 8191.");

            ushort lengthLsb = (ushort)(messageLength & 0xFF);
            ushort lengthMsb = (ushort)((messageLength >> 8) & 0x1F);
            var header = (ushort)((lengthLsb) | (type << 13) | (lengthMsb << 8));

            message[0] = (byte)(header & 0xFF);
            message[1] = (byte)((header >> 8) & 0xFF);
        }

        private static void PrepareMessageControlItem(Span<byte> message, ControlItemType controlItemType)
        {
            ushort controlParameterItem = (ushort)controlItemType;
            message[2] = (byte)(controlParameterItem & 0xFF);
            message[3] = (byte)((controlParameterItem >> 8) & 0xFF);
        }
        #endregion

    }
}
