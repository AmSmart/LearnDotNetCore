﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnDotNetCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LearnDotNetCore.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            ViewBag.Title = "Create Role";
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            ViewBag.Title = "Create Role";

            if (ModelState.IsValid)
            {
                IdentityRole idRole = new IdentityRole()
                {
                    Name = model.RoleName
                };

                IdentityResult result = await roleManager.CreateAsync(idRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Admin");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            ViewBag.Title = "All Roles";
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.Message = "The given role Id does not match any in the database";
                ViewBag.Title = "Not Found";
                return View("Error");
            }

            var model = new EditRoleViewModel()
            {
                RoleId = role.Id,
                RoleName = role.Name
            };
            foreach (var user in userManager.Users.ToList())
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                    model.Users.Add(user.UserName);
            }
            ViewBag.Title = "Edit Role";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            ViewBag.Title = "Edit Role";
            var role = await roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                ViewBag.Message = "The given role Id does not match any in the database";
                ViewBag.Title = "Not Found";
                return View("Error");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }            
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.Title = "Edit Users in Role";
            var role = await roleManager.FindByIdAsync(roleId);
            ViewBag.RoleId = roleId;
            if(role == null)
            {
                ViewBag.Message = "The queried role does not exist";
                return View("Error");
            }
            var model = new List<UserRoleViewModel>();

            foreach(var user in userManager.Users.ToList())
            {
                var userInRole = new UserRoleViewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    userInRole.IsSelected = true;
                }
                else
                {
                    userInRole.IsSelected = false;
                }
                model.Add(userInRole);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.Title = "Not Found";
                ViewBag.Message = "The queried role does not exist";
                return View("Error");
            }

            foreach (var userRoleVM in model)
            {
                var user = await userManager.FindByIdAsync(userRoleVM.UserId);    
                if(!await userManager.IsInRoleAsync(user,role.Name) && userRoleVM.IsSelected)
                {
                    await userManager.AddToRoleAsync(user, role.Name);
                }
                else if(await userManager.IsInRoleAsync(user,role.Name) && !userRoleVM.IsSelected)
                {
                    await userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }
            return RedirectToAction("EditRole", new { id = roleId });
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            ViewBag.Title = "List All Users";
            var model = userManager.Users.ToList();
            return View(model);
        }
    }
}