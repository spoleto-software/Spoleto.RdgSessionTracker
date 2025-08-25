namespace Spoleto.RdgSessionTracker.Tests
{
    public class RdgEventReaderTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetEvents()
        {
            // Arrange
            var reader = new RdgEventReader();
            var option = ConfigurationHelper.GetTestOption();

            // Act
            var sessions = reader.GetEvents(option.Since, option.To, option.MachineName);

            // Assert
            Assert.That(sessions, Is.Not.Null);
        }


        [Test]
        public void GetSummarySessions()
        {
            // Arrange
            var reader = new RdgEventReader();
            var option = ConfigurationHelper.GetTestOption();

            // Act
            var sessions = reader.GetSummarySessions(option.Since, option.To, option.MachineName);

            // Assert
            Assert.That(sessions, Is.Not.Null);
        }


        [Test]
        public void GetMergedEvents()
        {
            // Arrange
            var reader = new RdgEventReader();
            var option = ConfigurationHelper.GetTestOption();

            // Act
            var sessions = reader.GetMergedEvents(option.Since, option.To, TimeSpan.FromMinutes(1), option.MachineName);

            // Assert
            Assert.That(sessions, Is.Not.Null);
        }
    }
}