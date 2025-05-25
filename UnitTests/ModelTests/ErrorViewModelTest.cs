using tamb.Models;
using Xunit;

namespace UnitTests.ModelTests
{
    public class ErrorViewModelTest
    {
        [Fact]
        public void ShowRequestId_ReturnsTrue_WhenRequestIdIsNotNullOrEmpty()
        {
            var model = new ErrorViewModel { RequestId = "test123" };
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNullOrEmpty()
        {
            var model = new ErrorViewModel { RequestId = "" };
            Assert.False(model.ShowRequestId);
        }
    }
}
