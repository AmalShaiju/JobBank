using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sol_Job_Bank.Data;
using sol_Job_Bank.Models;

namespace sol_Job_Bank.Controllers
{
    [Authorize(Roles = "Staff, Supervisor, Admin")]
    public class ApplicationsController : Controller
    {
        private readonly JobBankContext _context;

        public ApplicationsController(JobBankContext context)
        {
            _context = context;
        }

        // GET: Applications
        public async Task<IActionResult> Index()
        {
            var applications = _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Posting).ThenInclude(a => a.Position);
            return View(await applications.ToListAsync());
        }

        // GET: Applications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Posting)
                .ThenInclude(a => a.Position)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // GET: Applications/Create
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Applications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Comments,PostingID,ApplicantID")] Application application)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(application);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, an Applicant can only apply to a job posting once.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateDropDownLists(application);
            return View(application);
        }

        // GET: Applications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(application);
            return View(application);
        }

        // POST: Applications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)//, [Bind("ID,Comments,PostingID,ApplicantID")] Application application)
        {
            var applicationToUpdate = await _context.Applications.FindAsync(id);
            if (applicationToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Application>(applicationToUpdate, "",
                a=>a.Comments, a => a.PostingID, a => a.ApplicantID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationExists(applicationToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("", "Unable to save changes. Remember, an Applicant can only apply to a job posting once.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            PopulateDropDownLists(applicationToUpdate);
            return View(applicationToUpdate);
        }

        // GET: Applications/Delete/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Posting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // POST: Applications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            try
            {
                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(application);
            
        }

        //Broke the Populate approach down into 2 methods for future use.
        //Also note that we prepare 2 SelectLists, one for each foreign key
        private void PopulateDropDownLists(Application application = null)
        {
            ViewData["ApplicantID"] = ApplicantSelectList(application?.ApplicantID);
            ViewData["PostingID"] = PostingSelectList(application?.PostingID);
        }
        private SelectList PostingSelectList(int? id)
        {
            //Note that we cannot use summary properties in the orderby clause
            //becuase they don't exist in the database.  However, by calling .ToList() we get a 
            //List of Postings fully materialized so the summary property can be used 
            //for the dataTextField in the SelectList itself.
            var dQuery = (from d in _context.Postings.Include(p=>p.Position)
                          orderby d.Position.Name, d.ClosingDate
                          select d).ToList();
            return new SelectList(dQuery, "ID", "PostingSummary", id);
        }
        private SelectList ApplicantSelectList(int? id)
        {
            //Note that we cannot use summary properties in the orderby clause
            //becuase they don't exist in the database.  However, by calling .ToList() we get a 
            //List of Applicants fully materialized so the FormalName summary property can be used 
            //for the dataTextField in the SelectList itself.
            var dQuery = (from d in _context.Applicants
                         orderby d.LastName, d.FirstName, d.MiddleName
                         select d).ToList();
            return new SelectList(dQuery, "ID", "FormalName", id);
        }

        private bool ApplicationExists(int id)
        {
            return _context.Applications.Any(e => e.ID == id);
        }
    }
}
