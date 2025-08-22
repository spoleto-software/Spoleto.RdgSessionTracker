namespace Spoleto.RdgSessionTracker.Tests
{
    public class RdgEventReaderTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetAllAccessReportEvents()
        {
            // Arrange
            var reader = new RdgEventReader();
            var option = ConfigurationHelper.GetTestOption();

            // Act
            var sessions = reader.GetSummarySessions(option.Since, option.To, option.MachineName);

            // Assert
            Assert.That(sessions, Is.Not.Null);
        }
    }
}