using Microsoft.AspNetCore.Mvc;
using MultiFactorAuthenticationDemo.Models;
using System.Diagnostics;

namespace MultiFactorAuthenticationDemo.Controllers;

public class HomeController : Controller
{
    #region Fields

    private readonly ILogger<HomeController> _logger;

    #endregion Fields

    #region Public Constructors

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Index()
    {
        ViewBag.UserName =  HttpContext.Session.GetString("Username");
        return View();
    }


    #endregion Public Methods
}