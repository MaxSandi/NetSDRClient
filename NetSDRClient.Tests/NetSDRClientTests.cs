using Moq;
using NetSDRClientApp.Services;

namespace NetSDRClient.Tests
{
    public class NetSDRClientTests
    {
        private NetSDRClientApp.NetSDRClient _client;
        private Mock<INetSDRNetworkService> _mockNetworkService;

        [SetUp]
        public void Setup()
        {
            _mockNetworkService = new Mock<INetSDRNetworkService>();
            _client = new(_mockNetworkService.Object, string.Empty);
        }

        [Test]
        public async Task StartAsync_ShouldReturnTrue_WhenCommandSucceeds()
        {
            // Arrange
            _mockNetworkService
                .Setup(service => service.SendCommandAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(true);

            // Act
            var result = await _client.StartAsync();

            // Assert
            Assert.True(result);
            _mockNetworkService.Verify(service => service.SendCommandAsync(It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task StartAsync_ShouldReturnFalse_WhenCommandFails()
        {
            // Arrange
            _mockNetworkService
                .Setup(service => service.SendCommandAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(false);

            // Act
            var result = await _client.StartAsync();

            // Assert
            Assert.False(result);
            _mockNetworkService.Verify(service => service.SendCommandAsync(It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task Start_ShouldReturnCorrectCommandData()
        {
            // Arrange
            byte[] commandMessage = [0x08, 0x00, 0x18, 0x00, 0x80, 0x02, 0x80, 0x00];
            _mockNetworkService.Setup(x => x.SendCommandAsync(commandMessage)).ReturnsAsync(true).Verifiable();

            // Act
            bool result = await _client.StartAsync();

            // Assert
            Assert.IsTrue(result);
            _mockNetworkService.Verify();
        }

        [Test]
        public async Task StopAsync_ShouldReturnTrue_WhenCommandSucceeds()
        {
            // Arrange
            _mockNetworkService
                .Setup(service => service.SendCommandAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(true);

            // Act
            var result = await _client.StopAsync();

            // Assert
            Assert.True(result);
            _mockNetworkService.Verify(service => service.SendCommandAsync(It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task StopAsync_ShouldReturnFalse_WhenCommandFails()
        {
            // Arrange
            _mockNetworkService
                .Setup(service => service.SendCommandAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(false);

            // Act
            var result = await _client.StopAsync();

            // Assert
            Assert.False(result);
            _mockNetworkService.Verify(service => service.SendCommandAsync(It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task Stop_ShouldReturnCorrectCommandData()
        {
            // Arrange
            byte[] commandMessage = [0x08, 0x00, 0x18, 0x00, 0x00, 0x01, 0x00, 0x00];
            _mockNetworkService.Setup(x => x.SendCommandAsync(commandMessage)).ReturnsAsync(true).Verifiable();

            // Act
            bool result = await _client.StopAsync();

            // Assert
            Assert.IsTrue(result);
            _mockNetworkService.Verify();
        }

        [Test]
        public async Task SetFrequencyAsync_ShouldReturnTrue_WhenCommandSucceeds()
        {
            // Arrange
            ulong newFrequency = 15_000_000; // Example frequency
            _mockNetworkService
                .Setup(service => service.SendCommandAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(true);

            // Act
            var result = await _client.SetFrequencyAsync(newFrequency);

            // Assert
            Assert.True(result);
            _mockNetworkService.Verify(service => service.SendCommandAsync(It.IsAny<byte[]>()), Times.Once);

            // Verify that the frequency was updated correctly
            Assert.That(_client._currentFrequency, Is.EqualTo(newFrequency));
        }

        [Test]
        public async Task SetFrequencyAsync_ShouldReturnFalse_WhenCommandFails()
        {
            // Arrange
            ulong newFrequency = 15_000_000; // Example frequency
            _mockNetworkService
                .Setup(service => service.SendCommandAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(false);

            // Act
            var result = await _client.SetFrequencyAsync(newFrequency);

            // Assert
            Assert.False(result);
            _mockNetworkService.Verify(service => service.SendCommandAsync(It.IsAny<byte[]>()), Times.Once);
            Assert.That(_client._currentFrequency, Is.Not.EqualTo(newFrequency));
        }

        [Test]
        public async Task SetFrequencyAsync_ShouldReturnCorrectCommandData()
        {
            // Arrange
            byte[] commandMessage = [0x0A, 0x00, 0x20, 0x00, 0x00, 0x90, 0xC6, 0xD5, 0x00, 0x00];
            _mockNetworkService.Setup(x => x.SendCommandAsync(commandMessage)).ReturnsAsync(true).Verifiable();

            // Act
            bool result = await _client.SetFrequencyAsync(14_010_000);

            // Assert
            Assert.IsTrue(result);
            _mockNetworkService.Verify();
        }
    }
}