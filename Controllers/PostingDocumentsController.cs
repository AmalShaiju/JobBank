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

    public class PostingDocumentsController : Controller
    {
        private readonly JobBankContext _context;

        public PostingDocumentsController(JobBankContext context)
        {
            _context = context;
        }

        // GET: PostingDocuments
        public async Task<IActionResult> Index(int? PostingID, string SearchName)
        {
            //Prepare SelectList for filter
            var dQuery = (from d in _context.Postings
                          .Include(p => p.Position)
                          orderby d.Position.Name, d.ClosingDate
                          select d).ToList();
            ViewData["PostingID"] = new SelectList(dQuery, "ID", "PostingSummary");

            var documents = from p in _context.PostingDocuments
                            .Include(p => p.Posting).ThenInclude(p=>p.Position)
                            select p;

            //Add as many filters as needed
            if (PostingID.HasValue)
            {
                documents = documents.Where(p => p.PostingID == PostingID);
            }
            if (!String.IsNullOrEmpty(SearchName))
            {
                documents = documents.Where(p => p.FileName.ToUpper().Contains(SearchName.ToUpper()));
            }
            //Order by File Name
            documents = documents.OrderBy(p => p.FileName);

            return View(await documents.ToListAsync());
        }

        // GET: PostingDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postingDocument = await _context.PostingDocuments
                .Include(p => p.Posting).ThenInclude(p => p.Position)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (postingDocument == null)
            {
                return NotFound();
            }

            return View(postingDocument);
        }

        // GET: PostingDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postingDocument = await _context.PostingDocuments
                .Include(p => p.Posting).ThenInclude(p => p.Position)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (postingDocument == null)
            {
                return NotFound();
            }

            //PopulateDropDownLists(postingDocument);
            return View(postingDocument);
        }

        // POST: PostingDocuments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var postingDocumentToUpdate = await _context.PostingDocuments
                .Include(p => p.Posting).ThenInclude(p=>p.Position)
                .SingleOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (postingDocumentToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<PostingDocument>(postingDocumentToUpdate, "",
                a => a.FileName, a => a.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostingDocumentExists(postingDocumentToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            //PopulateDropDownLists(postingDocumentToUpdate);
            return View(postingDocumentToUpdate);
        }

        // GET: PostingDocuments/Delete/5

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postingDocument = await _context.PostingDocuments
                .Include(p => p.Posting).ThenInclude(p => p.Position)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (postingDocument == null)
            {
                return NotFound();
            }

            return View(postingDocument);
        }

        // POST: PostingDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postingDocument = await _context.PostingDocuments
                .Include(p => p.Posting).ThenInclude(p => p.Position)
                .FirstOrDefaultAsync(m => m.ID == id);
            _context.PostingDocuments.Remove(postingDocument);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public FileContentResult Download(int id)
        {
            var theFile = _context.PostingDocuments
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .SingleOrDefault();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        //Broke the Populate approach down into 2 methods for future use.
        private void PopulateDropDownLists(PostingDocument postingDocument = null)
        {
            ViewData["PostingID"] = PostingSelectList(postingDocument?.PostingID);
        }
        private SelectList PostingSelectList(int? id)
        {
            //Note that we cannot use summary properties in the orderby clause
            //becuase they don't exist in the database.  However, by calling .ToList() we get a 
            //List of Postings fully materialized so the summary property can be used 
            //for the dataTextField in the SelectList itself.
            var dQuery = (from d in _context.Postings.Include(p => p.Position)
                          orderby d.Position.Name, d.ClosingDate
                          select d).ToList();
            return new SelectList(dQuery, "ID", "PostingSummary", id);
        }

        private bool PostingDocumentExists(int id)
        {
            return _context.PostingDocuments.Any(e => e.ID == id);
        }
    }
}
