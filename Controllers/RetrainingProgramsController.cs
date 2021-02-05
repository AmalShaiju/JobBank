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
    [Authorize(Roles = "Supervisor,Admin")]

    public class RetrainingProgramsController : Controller
    {
        private readonly JobBankContext _context;

        public RetrainingProgramsController(JobBankContext context)
        {
            _context = context;
        }

        // GET: RetrainingPrograms
        public async Task<IActionResult> Index()
        {
            return View(await _context.RetrainingPrograms.ToListAsync());
        }

        // GET: RetrainingPrograms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retrainingProgram = await _context.RetrainingPrograms
                .FirstOrDefaultAsync(m => m.ID == id);
            if (retrainingProgram == null)
            {
                return NotFound();
            }

            return View(retrainingProgram);
        }

        // GET: RetrainingPrograms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RetrainingPrograms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] RetrainingProgram retrainingProgram)
        {
            if (ModelState.IsValid)
            {
                _context.Add(retrainingProgram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(retrainingProgram);
        }

        // GET: RetrainingPrograms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retrainingProgram = await _context.RetrainingPrograms.FindAsync(id);
            if (retrainingProgram == null)
            {
                return NotFound();
            }
            return View(retrainingProgram);
        }

        // POST: RetrainingPrograms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] RetrainingProgram retrainingProgram)
        {
            if (id != retrainingProgram.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(retrainingProgram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RetrainingProgramExists(retrainingProgram.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(retrainingProgram);
        }

        // GET: RetrainingPrograms/Delete/5
            
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retrainingProgram = await _context.RetrainingPrograms
                .FirstOrDefaultAsync(m => m.ID == id);
            if (retrainingProgram == null)
            {
                return NotFound();
            }

            return View(retrainingProgram);
        }

        // POST: RetrainingPrograms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var retrainingProgram = await _context.RetrainingPrograms.FindAsync(id);
            _context.RetrainingPrograms.Remove(retrainingProgram);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RetrainingProgramExists(int id)
        {
            return _context.RetrainingPrograms.Any(e => e.ID == id);
        }
    }
}
