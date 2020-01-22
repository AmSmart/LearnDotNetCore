using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LearnDotNetCore.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        [Route("/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            ViewBag.Title = statusCode + " Error";
            ViewBag.Message = "The resource you're trying to access does not exist or has been deleted";
            logger.LogWarning($"Error Path: {statusCodeResult.OriginalPath}" +
                $"\nError Path Base: {statusCodeResult.OriginalPathBase}\nQuery String: {statusCodeResult.OriginalQueryString}");
            return View("Error");
        }

        [Route("/Error/Exception")]
        public IActionResult Exception()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.Title = "Oops!";
            ViewBag.Message = "Something went wrong!\n We;ll work to fix it. Report below if problem persists";
            logger.LogError($"Error: {exceptionDetails.Error}\nError Path: {exceptionDetails.Path}");
            return View("Error");
        }
    }
}
