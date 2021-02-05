using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sol_Job_Bank.Data;
using sol_Job_Bank.ViewModels;

namespace sol_Job_Bank.Controllers
{
    [Authorize(Roles = "Security")]
    public class UserRolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserRolesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await (from u in _context.Users
                               .OrderBy(u => u.UserName)
                               select new UserVM
                               {
                                   Id = u.Id,
                                   UserName = u.UserName
                               }).ToListAsync();
            foreach (var u in users)
            {
                var user = await _userManager.FindByIdAsync(u.Id);
                u.userRoles = await _userManager.GetRolesAsync(user);
            };
            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            var _user = await _userManager.FindByIdAsync(id);//IdentityRole
            if (_user == null)
            {
                return NotFound();
            }
            UserVM user = new UserVM
            {
                Id = _user.Id,
                UserName = _user.UserName,
                userRoles = await _userManager.GetRolesAsync(_user)
            };
            PopulateAssignedRoleData(user);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string Id, string[] selectedOptions)
        {
            var _user = await _userManager.FindByIdAsync(Id);//IdentityRole
            UserVM user = new UserVM
            {
                Id = _user.Id,
                UserName = _user.UserName,
                userRoles = await _userManager.GetRolesAsync(_user)
            };

            try
            {
                if(_user.UserName != User.Identity.Name)
                {
                    await UpdateUserRolesAsync(selectedOptions, user);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to make changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty,
                                "Unable to save changes.");
            }
            PopulateAssignedRoleData(user);
            return View(user);
        }

        private void PopulateAssignedRoleData(UserVM user)
        {
            var allOptions = _context.Roles;
            var currentOptionsHS = user.userRoles;
            var selected = new List<RoleVM>();
            var available = new List<RoleVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.Name))
                {
                    selected.Add(new RoleVM
                    {
                        RoleId = s.Id,
                        RoleName = s.Name
                    });
                }
                else
                {
                    available.Add(new RoleVM
                    {
                        RoleId = s.Id,
                        RoleName = s.Name
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.RoleName), "RoleName", "RoleName");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.RoleName), "RoleName", "RoleName");
        }
        private async Task UpdateUserRolesAsync(string[] selectedOptions, UserVM userToUpdate)
        {
            var currentOptionsHS = userToUpdate.userRoles;
            var _user = await   _userManager.FindByIdAsync(userToUpdate.Id);//IdentityUser

            IList<IdentityRole> allRoles = _context.Roles.ToList<IdentityRole>();
                

            if (selectedOptions == null)
            {
                //No roles selected so just remove any currently assigned
                foreach (var r in currentOptionsHS)
                {
                    await _userManager.RemoveFromRoleAsync(_user, r);
                    return;
                }
            }
                

            foreach (var s in allRoles)
            {
                if (selectedOptions.Contains(s.Name))
                {
                    if (!currentOptionsHS.Contains(s.Name))
                    {
                        await _userManager.AddToRoleAsync(_user, s.Name);
                    }                                                                                                                                                                                           
                }
                else
                {
                    if (currentOptionsHS.Contains(s.Name))
                    {
                        await _userManager.RemoveFromRoleAsync(_user, s.Name);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}