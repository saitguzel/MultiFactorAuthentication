using Google.Authenticator;
using Microsoft.AspNetCore.Mvc;
using MultiFactorAuthenticationDemo.Models;
using System.Text;

namespace MultiFactorAuthenticationDemo.Controllers;

public class LoginController : Controller
{
    #region Fields

    private readonly IConfiguration _configuration;

    #endregion Fields

    #region Public Constructors

    public LoginController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    #endregion Public Constructors

    #region Public Methods

    public ActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("IsValidTwoFactorAuthentication") == "true")
        {
            return View();
        }
        else
        {
            return RedirectToAction("Login");
        }
    }

    [HttpGet]
    [Route("login")]
    public IActionResult Login()
    {
        GlobalData.isLoggedIn = false;
        var message = TempData["message"];
        ViewBag.Message = message;
        return View();
    }

    [HttpGet,Route("Logout")]
    public ActionResult Logout()
    {
        HttpContext.Session.Remove("UserName");
        HttpContext.Session.Remove("IsValidTwoFactorAuthentication");
        HttpContext.Session.Remove("UserUniqueKey");
        return RedirectToAction("Login");
    }

    public ActionResult MultiFactorAuthenticate()
    {
        var token = Request.Form["CodeDigit"];
        TwoFactorAuthenticator TwoFacAuth = new();
        string? UserUniqueKey = HttpContext.Session.GetString("UserUniqueKey");

        // bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, token);
        bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, token, TimeSpan.FromSeconds(30)); // will be valid for 30 seconds
        if (isValid)
        {
            HttpContext.Session.SetString("IsValidTwoFactorAuthentication", "true");
            GlobalData.isLoggedIn = true;
            return RedirectToAction("Dashboard");
        }
        TempData["message"] = "Google Two Factor PIN is expired or wrong";
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Route("multi-factor-authentication")]
    public IActionResult MultiFactorAuthentication()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Login");
        }
        string? username = HttpContext.Session.GetString("Username");
        string authKey = _configuration.GetValue<string>("AuthenticatorKey");
        string userUniqueKey = username + authKey;
        // Two Factor Authentication Setup
        TwoFactorAuthenticator twoFacAuth = new();
        var setupInfo = twoFacAuth.GenerateSetupCode(
            "MultiFactorAuthenticationDemo",
            username,
            ConvertSecretToBytes(userUniqueKey, false),
            300
        );
        HttpContext.Session.SetString("UserUniqueKey", userUniqueKey);
        ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
        ViewBag.SetupCode = setupInfo.ManualEntryKey;
        return View();
    }

    private static byte[] ConvertSecretToBytes(string secret, bool secretIsBase32) =>
           secretIsBase32 ? Base32Encoding.ToBytes(secret) : Encoding.UTF8.GetBytes(secret);

    [HttpPost]
    [Route("login")]
    public ActionResult Verify(LoginModel login)
    {
        string? username = HttpContext.Session.GetString("Username");
        string isValidStr = HttpContext.Session.GetString("IsValidTwoFactorAuthentication");
        bool? isValidTwoFactorAuthentication = isValidStr != null ? bool.Parse(isValidStr) : (bool?)null;

        if (username == null || isValidTwoFactorAuthentication == false || isValidTwoFactorAuthentication == null)
        {
            if (login.Username == "Admin" && login.Password == "12345")
            {
                HttpContext.Session.SetString("Username", login.Username);
                return RedirectToAction("MultiFactorAuthentication");
            }
        }
        return RedirectToAction("Index");
    }

    #endregion Public Methods
}