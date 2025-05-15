using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tamb.Models;

namespace tamb.Controllers;

public class CalendarController : Controller
{
public IActionResult Index()
    {
        return View();
    }
}

