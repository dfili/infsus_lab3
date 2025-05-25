using tamb.Controllers;
using tamb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace UnitTests
{
	public class HomeControllerTest
	{
		private readonly HomeController _controller;

		public HomeControllerTest()
		{
			var mockLogger = new Mock<ILogger<HomeController>>();
			_controller = new HomeController(mockLogger.Object);
		}

		[Fact]
		public void Index_Returns_ViewResult()
		{
			var result = _controller.Index();
			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public void Privacy_Returns_ViewResult()
		{
			var result = _controller.Privacy();
			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public void Error_Returns_ViewResult_WithModel()
		{
			var mockLogger = new Mock<ILogger<HomeController>>();
			var controller = new HomeController(mockLogger.Object);

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			var result = controller.Error();

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
			Assert.NotNull(model.RequestId);
		}
	}
}
