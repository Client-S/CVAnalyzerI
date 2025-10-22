using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Auth()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            var email = User.FindFirstValue(ClaimTypes.Email);

            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            var result = new
            {
                IsAuthenticated = isAuthenticated,
                UserId = userId ?? "NULL",
                UserName = userName ?? "NULL",
                Email = email ?? "NULL",
                ClaimCount = claims.Count,
                AllClaims = claims
            };

            return Json(result);
        }

        [HttpGet]
        public IActionResult AuthView()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            var email = User.FindFirstValue(ClaimTypes.Email);

            var claims = User.Claims.Select(c => $"{c.Type} = {c.Value}").ToList();

            ViewBag.IsAuthenticated = isAuthenticated;
            ViewBag.UserId = userId ?? "Not Found";
            ViewBag.UserName = userName ?? "Not Found";
            ViewBag.Email = email ?? "Not Found";
            ViewBag.Claims = claims;

            return View();
        }
    }

}
