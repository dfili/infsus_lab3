using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tamb.Models;

namespace tamb.Controllers;

public class PeopleController : Controller
{
public IActionResult Index()
    {
        return View();
    }
}

