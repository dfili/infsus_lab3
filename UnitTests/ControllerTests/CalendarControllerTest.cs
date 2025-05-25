using tamb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ControllerTest
{
	public class CalendarControllerTest
	{
		[Fact]
		public void Index_Returns_ViewResult()
		{
			var controller = new CalendarController();

			var result = controller.Index();

			Assert.IsType<ViewResult>(result);
		}
	}
}

