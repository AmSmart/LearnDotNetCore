using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LearnDotNetCore.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            ViewBag.Title = statusCode + " Error";
            ViewBag.Message = "The resource you're trying to access does not exist or has been deleted";
            //Log error
            return View("Error");
        }

        [Route("/Error/Exception")]
        public IActionResult Exception()
        {
            ViewBag.Title = "Oops!";
            ViewBag.Message = "Something went wrong!\n We;ll work to fix it. Report below if problem persists";
            return View("Error");
        }
    }
}
