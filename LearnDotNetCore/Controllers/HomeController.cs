using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LearnDotNetCore.Models;
using LearnDotNetCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using LearnDotNetCore.Security;
using Microsoft.AspNetCore.DataProtection;

namespace LearnDotNetCore.Controllers
{
    public class HomeController : Controller
    {
        private IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IDataProtector dataProtector;

        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment hostEnvironment,
            IDataProtectionProvider protectionProvider, DataProtectionPurposeStrings purposeStrings)
        {
            _employeeRepository = employeeRepository;
            this.hostEnvironment = hostEnvironment;
            dataProtector = protectionProvider.CreateProtector(purposeStrings.EmployeeIdRouteValue);
        }

        public ViewResult Index()
        {
            var employeeList = _employeeRepository.GetAllEmployees()
                .Select(x => 
                {
                    x.EncryptedId = dataProtector.Protect(x.Id.ToString());
                    return x;
                });
            ViewBag.Title = "All Employees";
            return View(employeeList);
        }

        public ViewResult Details(string id)
        {
            string decryptedId = dataProtector.Unprotect(id);
            Employee employee = _employeeRepository.GetEmployee(Convert.ToInt32(decryptedId));
            if(employee == null)
            {
                //Error view is used to display an error here
                ViewBag.Title = "Error";
                ViewBag.Message = "Emplyee Not found";
                return View("Error");
            }
            var vm = new HomeDetailsViewModel() 
            {
                Employee = employee,
                PageTitle = "Employee Details"
            };
            ViewBag.Title = $"Employee {decryptedId}";
            return View(vm);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Title = "New Employee";
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Employee model)
        {
            ViewBag.Title = "New Employee";
            if (ModelState.IsValid)
            {
                if(model.Photo != null)
                {
                    await SavePhoto(model);
                }
                _employeeRepository.AddEmployee(model);
                return RedirectToAction("Details", new { id = model.Id });
            }
            return View(model);
            
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var employee = _employeeRepository.GetEmployee(id);
            ViewBag.Title = "Edit Employee";
            return View(employee);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(Employee model)
        {
            if (ModelState.IsValid)
            {
                Employee oldEmployee = _employeeRepository.GetEmployee(model.Id);
                if(model.Photo != null)
                {
                    if (oldEmployee.PhotoFileName != null)
                    {
                        string oldPhotoPath = Path.Combine(hostEnvironment.WebRootPath, "images",
                        oldEmployee.PhotoFileName);
                        System.IO.File.Delete(oldPhotoPath);
                    }

                    await SavePhoto(model);
                }
                _employeeRepository.UpdateEmployee(model);
                return RedirectToAction("Details", new { id = model.Id });
            }
            return View(model);
        }

        private async Task SavePhoto(Employee model)
        {
            string fileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
            string photoPath = Path.Combine(hostEnvironment.WebRootPath, "images", fileName);
            using (var fs = new FileStream(photoPath, FileMode.Create))
            {
                await model.Photo.CopyToAsync(fs);
            }
            model.PhotoFileName = fileName;
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            _employeeRepository.DeleteEmployee(id);
            return RedirectToAction("Index", "Home");
        }
    }
}