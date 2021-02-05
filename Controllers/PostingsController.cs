using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using sol_Job_Bank.Data;
using sol_Job_Bank.Models;
using sol_Job_Bank.Utilities;
using sol_Job_Bank.ViewModels;
using static sol_Job_Bank.Utilities.EmailSender;

namespace sol_Job_Bank.Controllers
{
    [Authorize]
    public class PostingsController : Controller
    {

        //for sending email
        private readonly IMyEmailSender _emailSender;

        private readonly JobBankContext _context;
       

        public PostingsController(JobBankContext context, IMyEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET/POST: MedicalTrials/Notification/5
        public async Task<IActionResult> Notification(int? id, string Subject, string emailContent, string TrialName)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["id"] = id;
            ViewData["TrialName"] = TrialName;

            if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
            {
                ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
            }
            else
            {
                int folksCount = 0;
                try
                {
                    //Send a Notice.
                    List<EmailAddress> folks = (from p in _context.Applications.Include(p => p.Applicant)
                                                where p.PostingID == id
                                                select new EmailAddress
                                                {
                                                    Name = p.Applicant.FullName,
                                                    Address = p.Applicant.eMail
                                                }).ToList();
                    folksCount = folks.Count();
                    if (folksCount > 0)
                    {
                        var msg = new EmailMessage()
                        {
                            ToAddresses = folks,
                            Subject = Subject,
                            Content = "<p>" + emailContent + "</p><p>Please access the <strong>Niagara College</strong> web site to review.</p>"

                        };
                        await _emailSender.SendToManyAsync(msg);
                        ViewData["Message"] = "Message sent to " + folksCount + " Applicants"
                            + ((folksCount == 1) ? "." : "s.");
                    }
                    else
                    {
                        ViewData["Message"] = "Message NOT sent!  No Applicant in Job Posting";
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.GetBaseException().Message;
                    ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Applicants"
                        + ((folksCount == 1) ? "" : "s") + " in the Job posting.";
                }
            }
            return View();
        }

        // GET: Postings
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            var postings = _context.Postings
                .Include(p => p.Position)
                .OrderBy(p => p.Position.Name);
            int pageSize = 5;//Change as required int pageSize;//This is the value we will pass to PaginatedList
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
                new SelectList(new[] { "3", "5", "10", "20", "30", "40", "50", "100", "500" }, pageSize.ToString()); var pagedData = await PaginatedList<Posting>.CreateAsync(postings.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Postings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posting = await _context.Postings
                .Include(p => p.Position)
                .ThenInclude(p => p.PositionSkills)
                .ThenInclude(p=> p.Skill)
                .Include(p => p.PostingDocuments)
                .Include(p => p.Applications).ThenInclude(p => p.Applicant)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (posting == null)
            {
                return NotFound();
            }

            return View(posting);
        }

        // GET: Postings/Create
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Postings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]

        public async Task<IActionResult> Create([Bind("ID,NumberOpen,ClosingDate,StartDate,PositionID")] Posting posting
            , List<IFormFile> theFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(posting, theFiles);
                    _context.Add(posting);
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
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have multiple postings for the same position with the same closing date.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateDropDownLists(posting);
            return View(posting);
        }

        // GET: Postings/Edit/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posting = await _context.Postings
                .Include(p=>p.PostingDocuments)
                .SingleOrDefaultAsync(p=>p.ID==id);
            if (posting == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(posting);
            return View(posting);
        }

        // POST: Postings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, List<IFormFile> theFiles)
        {
            var postingToUpdate = await _context.Postings
                .Include(p => p.PostingDocuments)
                .SingleOrDefaultAsync(p => p.ID == id);
            if (postingToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Posting>(postingToUpdate, "",
                d => d.NumberOpen, d => d.ClosingDate, d => d.StartDate, d => d.PositionID))
            {
                try
                {
                    await AddDocumentsAsync(postingToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostingExists(postingToUpdate.ID))
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
                        ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have multiple postings for the same position with the same closing date.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            PopulateDropDownLists(postingToUpdate);
            return View(postingToUpdate);
        }

        // GET: Postings/Delete/5
        [Authorize(Roles =  "Supervisor, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posting = await _context.Postings
                .Include(p => p.Position)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (posting == null)
            {
                return NotFound();
            }

            return View(posting);
        }

        // POST: Postings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var posting = await _context.Postings.FindAsync(id);
            try
            {
                _context.Postings.Remove(posting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete Posting. Remember, you cannot delete a Posting once applications have been submitted.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(posting);

        }

        public FileContentResult Download(int id)
        {
            var theFile = _context.PostingDocuments
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .SingleOrDefault();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Posting posting, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        PostingDocument d = new PostingDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        posting.PostingDocuments.Add(d);
                    };
                }
            }
        }

        //Broke the Populate approach down into 2 methods for future use.
        private void PopulateDropDownLists(Posting posting = null)
        {
            ViewData["PositionID"] = PositionSelectList(posting?.PositionID);
        }
        private SelectList PositionSelectList(int? id)
        {
            var dQuery = from d in _context.Positions
                         orderby d.Name
                         select d;
            return new SelectList(dQuery, "ID", "Name", id);
        }

        private bool PostingExists(int id)
        {
            return _context.Postings.Any(e => e.ID == id);
        }
    }
}
