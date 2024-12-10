using NetSDRClientApp.Helpers;
using NetSDRClientApp.Model;
using System.Buffers;

namespace NetSDRClient.Tests
{
    internal class NetSDRHelperTests
    {
        [Test]
        public void Start_ShouldReturnCorrectCommandData()
        {
            // Arrange
            byte[] commandMessage = [0x0A, 0x00, 0x20, 0x00, 0x00, 0x90, 0xC6, 0xD5, 0x00, 0x00];
            ushort messageLength = 10; // 4 fixed + 6 parameters
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

            // Act
            var result = NetSDRHelper.PrepareSetFrequencyMessage(memoryOwner, messageLength, ChannelId.Channel1, 14_010_000);

            // Assert
            Assert.That(commandMessage, Is.EqualTo(result.ToArray()));
        }
    }
}
