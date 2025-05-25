using Microsoft.AspNetCore.Mvc;
using tamb.Controllers;
using Xunit;

namespace UnitTests
{
    public class PeopleControllerTest
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            var controller = new PeopleController();

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }
    }
}
