using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LearnDotNetCore.Models;
using LearnDotNetCore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace LearnDotNetCore.Controllers
{
    public class HomeController : Controller
    {
        private IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment hostEnvironment;

        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment hostEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostEnvironment = hostEnvironment;
        }

        public ViewResult Index()
        {
            var employeeList = _employeeRepository.GetAllEmployees();
            ViewBag.Title = "All Employees";
            return View(employeeList);
        }

        public ViewResult Details(int? id)
        {
            var vm = new HomeDetailsViewModel() 
            {
                Employee = _employeeRepository.GetEmployee(id??1),
                PageTitle = "Employee Details"
            };
            ViewBag.Title = "Employee 1";
            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Title = "New Employee";
            return View();
        }

        [HttpPost]
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
            return View();
            
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _employeeRepository.GetEmployee(id);
            ViewBag.Title = "Edit Employee";
            return View(employee);
        }

        [HttpPost]
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
    }
}