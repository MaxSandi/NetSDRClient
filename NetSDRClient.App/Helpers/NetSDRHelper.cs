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
    }
}
