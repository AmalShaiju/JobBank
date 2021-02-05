using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using sol_Job_Bank.Data;
using sol_Job_Bank.Models;
using sol_Job_Bank.Utilities;
using sol_Job_Bank.ViewModels;

namespace sol_Job_Bank.Controllers
{
    [Authorize(Roles = "Staff, Supervisor, Admin")]
    public class PositionsController : Controller
    {
        private readonly JobBankContext _context;

        public PositionsController(JobBankContext context)
        {
            _context = context;
        }

        // GET: Positions
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            var positions = _context.Positions
                .Include(p => p.Occupation)
                .Include(p => p.PositionSkills).ThenInclude(p => p.Skill)
                .OrderBy(p => p.Name);
            int pageSize;//This is the value we will pass to PaginatedList
            if (pageSizeID.HasValue)
            {
                //Value selected from DDL so use and save it to Cookie
                pageSize = pageSizeID.GetValueOrDefault();
                CookieHelper.CookieSet(HttpContext, "pageSizeValue", pageSize.ToString(), 30);
            }
            else
            {
                //Not selected so see if it is in Cookie
                pageSize = Convert.ToInt32(HttpContext.Request.Cookies["pageSizeValue"]);
            }
            pageSize = (pageSize == 0) ? 3 : pageSize;//Neither Selected or in Cookie so go with default
            ViewData["pageSizeID"] =
                new SelectList(new[] { "3", "5", "10", "20", "30", "40", "50", "100", "500" }, pageSize.ToString());
            var pagedData = await PaginatedList<Position>.CreateAsync(positions.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Positions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.Occupation)
                .Include(p => p.PositionSkills).ThenInclude(p => p.Skill)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }

        // GET: Positions/Create
        [Authorize(Roles = "Supervisor, Admin")]

        public IActionResult Create()
        {
            Position position = new Position();
            PopulateAssignedSkillData(position);
            PopulateDropDownLists();
            return View();
        }

        // POST: Positions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,Salary,OccupationID")] Position position,
            string[] selectedOptions)
        {
            try
            {
                UpdatePositionSkills(selectedOptions, position);
                if (ModelState.IsValid)
                {
                    _context.Add(position);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate position names.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            PopulateAssignedSkillData(position);
            PopulateDropDownLists(position);
            return View(position);
        }

        // GET: Positions/Edit/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.PositionSkills).ThenInclude(p => p.Skill)
                .SingleOrDefaultAsync(p => p.ID == id);
            if (position == null)
            {
                return NotFound();
            }
            PopulateAssignedSkillData(position);
            PopulateDropDownLists(position);
            return View(position);
        }

        // POST: Positions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions)
        {
            var positionToUpdate = await _context.Positions
                .Include(p => p.PositionSkills).ThenInclude(p => p.Skill)
                .SingleOrDefaultAsync(p => p.ID == id);
            //Check that you got it or exit with a not found error
            if (positionToUpdate == null)
            {
                return NotFound();
            }

            UpdatePositionSkills(selectedOptions, positionToUpdate);

            if (await TryUpdateModelAsync<Position>(positionToUpdate, "",
                d => d.Name, d => d.Description, d => d.Salary, d => d.OccupationID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositionExists(positionToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate position names.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            PopulateAssignedSkillData(positionToUpdate);
            PopulateDropDownLists(positionToUpdate);
            return View(positionToUpdate);
        }

        // GET: Positions/Delete/5
        [Authorize(Roles = " Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.Occupation)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }

        // POST: Positions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            try
            {
                _context.Positions.Remove(position);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Position. Remember, you cannot delete a Position that has Postings.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(position);

        }

        private void PopulateAssignedSkillData(Position position)
        {
            var allOptions = _context.Skills;
            var currentOptionsHS = new HashSet<int>(position.PositionSkills.Select(b => b.SkillID));
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Name
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Name
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdatePositionSkills(string[] selectedOptions, Position positionToUpdate)
        {
            if (selectedOptions == null)
            {
                positionToUpdate.PositionSkills = new List<PositionSkill>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(positionToUpdate.PositionSkills.Select(b => b.SkillID));
            foreach (var s in _context.Skills)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))
                {
                    if (!currentOptionsHS.Contains(s.ID))
                    {
                        positionToUpdate.PositionSkills.Add(new PositionSkill
                        {
                            SkillID = s.ID,
                            PositionID = positionToUpdate.ID
                        });
                    }
                }
                else
                {
                    if (currentOptionsHS.Contains(s.ID))
                    {
                        PositionSkill positionToRemove = positionToUpdate.PositionSkills.SingleOrDefault(d => d.SkillID == s.ID);
                        _context.Remove(positionToRemove);
                    }
                }
            }
        }

        //Broke the Populate approach down into 2 methods for future use.
        private void PopulateDropDownLists(Position position = null)
        {
            ViewData["OccupationID"] = OccupationSelectList(position?.OccupationID);
        }
        private SelectList OccupationSelectList(int? id)
        {
            var dQuery = from d in _context.Occupations
                         orderby d.Title
                         select d;
            return new SelectList(dQuery, "ID", "Title", id);
        }

        private bool PositionExists(int id)
        {
            return _context.Positions.Any(e => e.ID == id);
        }
    }
}
