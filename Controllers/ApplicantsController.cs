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

namespace sol_Job_Bank.Controllers
{
    [Authorize(Roles = "Staff, Supervisor, Admin")]

    public class ApplicantsController : Controller
    {
        private readonly JobBankContext _context;

        public ApplicantsController(JobBankContext context)
        {
            _context = context;
        }

        // GET: Applicants
        public async Task<IActionResult> Index(string SearchName, string SearchPhone, string SearchEmail, int? page, int? pageSizeID,
            int? PostingID, int? SkillID, string actionButton, string sortDirection = "asc", string sortField = "Applicant")
        {
            //Prepare SelectLists for filters
            var dQuery = (from d in _context.Postings.Include(p => p.Position)
                          orderby d.Position.Name, d.ClosingDate
                          select d).ToList();
            ViewData["PostingID"] = new SelectList(dQuery, "ID", "PostingSummary");
            ViewData["SkillID"] = new SelectList(_context.Skills.OrderBy(s => s.Name), "ID", "Name");
            ViewData["Filtering"] = "";  //Assume not filtering

            //Start with Includes
            var applicants = from a in _context.Applicants
               .Include(a => a.RetrainingProgram)
                .Include(d => d.ApplicantDocuments)
                 .Include(p => p.ApplicantPhoto).ThenInclude(p => p.FileContent)
               .Include(a => a.ApplicantSkills)
               .ThenInclude(a => a.Skill)
                             select a;

            //Add as many filters as needed
            if (SkillID.HasValue)
            {
                applicants = applicants.Where(p => p.ApplicantSkills.Any(c => c.SkillID == SkillID));
                ViewData["Filtering"] = " show";
            }
            if (PostingID.HasValue)
            {
                applicants = applicants.Where(p => p.Applications.Any(c => c.PostingID == PostingID));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(SearchName))
            {
                applicants = applicants.Where(p => p.LastName.ToUpper().Contains(SearchName.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchName.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(SearchEmail))
            {
                applicants = applicants.Where(p => p.eMail.ToUpper().Contains(SearchEmail.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset page to start

                if (actionButton != "Filter")//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by
            if (sortField == "eMail")
            {
                if (sortDirection == "asc")
                {
                    applicants = applicants
                        .OrderBy(p => p.eMail);
                }
                else
                {
                    applicants = applicants
                        .OrderByDescending(p => p.eMail);
                }
            }
            else if (sortField == "Phone")
            {
                if (sortDirection == "asc")
                {
                    applicants = applicants
                        .OrderBy(p => p.Phone);
                }
                else
                {
                    applicants = applicants
                        .OrderByDescending(p => p.Phone);
                }
            }
            else //Sorting by Applicant Name
            {
                if (sortDirection == "asc")
                {
                    applicants = applicants
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    applicants = applicants
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
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
            var pagedData = await PaginatedList<Applicant>.CreateAsync(applicants.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Applicants/Details/5
        public async Task<IActionResult> Details(int? id, string returnURL)
        {
            if (String.IsNullOrEmpty(returnURL))
            {
                returnURL = Request.Headers["Referer"].ToString();
            }
            ViewData["returnURL"] = returnURL;

            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicants
                .Include(a => a.RetrainingProgram)
                .Include(d => d.ApplicantDocuments)

                .Include(p => p.ApplicantPhoto).ThenInclude(p => p.FileContent)
                .Include(a => a.Applications)
                .ThenInclude(a => a.Posting)
                .ThenInclude(a => a.Position)//We need this for the summary property in Posting
                .FirstOrDefaultAsync(m => m.ID == id);
            if (applicant == null)
            {
                return NotFound();
            }

            return View(applicant);
        }

        // GET: Applicants/Create
        public IActionResult Create()
        {
            //Add all (unchecked) Conditions to ViewBag
            var applicant = new Applicant();
            PopulateAssignedSkillData(applicant);
            PopulateDropDownLists();
            return View();
        }

        // POST: Applicants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,SIN,Phone,eMail,RetrainingProgramID")] Applicant applicant, IFormFile thePicture, string[] selectedOptions, List<IFormFile> theFiles)
        {
            try
            {
                //Add the selected skills
                if (selectedOptions != null)
                {
                    foreach (var skill in selectedOptions)
                    {
                        var skillToAdd = new ApplicantSkill { ApplicantID = applicant.ID, SkillID = int.Parse(skill) };
                        applicant.ApplicantSkills.Add(skillToAdd);
                    }
                }
                if (ModelState.IsValid)
                {
                    await AddPicture(applicant, thePicture);
                    _context.Add(applicant);
                    await AddDocumentsAsync(applicant, theFiles);
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
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have duplicate eMail addresses.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateDropDownLists(applicant);
            return View(applicant);
        }

        // GET: Applicants/Edit/5
        public async Task<IActionResult> Edit(int? id, string returnURL)
        {
            if (String.IsNullOrEmpty(returnURL))
            {
                returnURL = Request.Headers["Referer"].ToString();
            }
            ViewData["returnURL"] = returnURL;

            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicants
                .Include(a => a.RetrainingProgram)
                .Include(d => d.ApplicantDocuments)
                .Include(p => p.ApplicantPhoto).ThenInclude(p => p.FileContent)
                .Include(a => a.ApplicantSkills)
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.ID == id);
            if (applicant == null)
            {
                return NotFound();
            }
            PopulateAssignedSkillData(applicant);
            PopulateDropDownLists(applicant);
            return View(applicant);
        }

        // POST: Applicants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string chkRemoveImage, IFormFile thePicture, string returnURL, string[] selectedOptions, Byte[] RowVersion, List<IFormFile> theFiles)
        {
            ViewData["returnURL"] = returnURL;
            var applicantToUpdate = await _context.Applicants
                .Include(d => d.ApplicantDocuments)
                .Include(a => a.RetrainingProgram)
                .Include(a => a.ApplicantSkills)
                 .Include(p => p.ApplicantPhoto).ThenInclude(p => p.FileContent)
                .SingleOrDefaultAsync(a => a.ID == id);
            //Check that you got it or exit with a not found error
            if (applicantToUpdate == null)
            {
                return NotFound();
            }

            //Update the skills
            UpdateApplicantSkills(selectedOptions, applicantToUpdate);

            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(applicantToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<Applicant>(applicantToUpdate, "",
                a => a.FirstName, a => a.MiddleName, a => a.FirstName, a => a.LastName,
                a => a.SIN, a => a.Phone, a => a.eMail, a => a.RetrainingProgramID))
            {


                try
                {
                    if (chkRemoveImage != null)
                    {
                        applicantToUpdate.ApplicantPhoto = null;
                    }
                    else
                    {
                        await AddPicture(applicantToUpdate, thePicture);
                    }
                    await AddDocumentsAsync(applicantToUpdate, theFiles);
                    await _context.SaveChangesAsync();

                    //If no referrer then go back to index
                    if (String.IsNullOrEmpty(returnURL))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return RedirectToAction("Details", new { applicantToUpdate.ID, returnURL });
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException ex)// Added for concurrency
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Applicant)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Patient was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Applicant)databaseEntry.ToObject();
                        if (databaseValues.FirstName != clientValues.FirstName)
                            ModelState.AddModelError("FirstName", "Current value: "
                                + databaseValues.FirstName);
                        if (databaseValues.MiddleName != clientValues.MiddleName)
                            ModelState.AddModelError("MiddleName", "Current value: "
                                + databaseValues.MiddleName);
                        if (databaseValues.LastName != clientValues.LastName)
                            ModelState.AddModelError("LastName", "Current value: "
                                + databaseValues.LastName);
                        if (databaseValues.SIN != clientValues.SIN)
                            ModelState.AddModelError("SIN", "Current value: "
                                + databaseValues.SIN);
                        if (databaseValues.Phone != clientValues.Phone)
                            ModelState.AddModelError("Phone", "Current value: "
                                + String.Format("{0:(###) ###-####}", databaseValues.Phone));
                        if (databaseValues.eMail != clientValues.eMail)
                            ModelState.AddModelError("eMail", "Current value: "
                                + databaseValues.eMail);


                        //A little extra work for the nullable foreign key.  No sense going to the database and asking for something
                        //we already know is not there.
                        if (databaseValues.RetrainingProgramID != clientValues.RetrainingProgramID)
                        {
                            if (databaseValues.RetrainingProgramID.HasValue)
                            {
                                RetrainingProgram databaseRetrainingProgram = await _context.RetrainingPrograms.SingleOrDefaultAsync(i => i.ID == databaseValues.RetrainingProgramID);
                                ModelState.AddModelError("MedicalTrialID", $"Current value: {databaseRetrainingProgram?.Name}");
                            }
                            else

                            {
                                ModelState.AddModelError("MedicalTrialID", $"Current value: None");
                            }
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to List' hyperlink.");
                        applicantToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have duplicate eMail addresses.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }

            //Validaiton Error so give the user another chance.
            PopulateAssignedSkillData(applicantToUpdate);
            PopulateDropDownLists(applicantToUpdate);
            return View(applicantToUpdate);
        }

        // GET: Applicants/Delete/5
        [Authorize(Roles = "Supervisor, Admin")]

        public async Task<IActionResult> Delete(int? id, string returnURL)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (String.IsNullOrEmpty(returnURL))
            {
                returnURL = Request.Headers["Referer"].ToString();
            }
            ViewData["returnURL"] = returnURL;
            var applicant = await _context.Applicants
                .Include(a => a.RetrainingProgram)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (applicant == null)
            {
                return NotFound();
            }

            return View(applicant);
        }

        // POST: Applicants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, string returnURL)
        {
            var applicant = await _context.Applicants
                .Include(a => a.RetrainingProgram)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                _context.Applicants.Remove(applicant);

                await _context.SaveChangesAsync();
                //If no referrer then go back to index
                if (String.IsNullOrEmpty(returnURL))
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return Redirect(returnURL);
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(applicant);

        }

        private void PopulateAssignedSkillData(Applicant applicant)
        {
            var allOptions = _context.Skills;
            var currentOptionIDs = new HashSet<int>(applicant.ApplicantSkills.Select(b => b.SkillID));
            var checkBoxes = new List<OptionVM>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new OptionVM
                {
                    ID = option.ID,
                    DisplayText = option.Name,
                    Assigned = currentOptionIDs.Contains(option.ID)
                });
            }
            ViewData["SkillOptions"] = checkBoxes;
        }
        private void UpdateApplicantSkills(string[] selectedOptions, Applicant applicantToUpdate)
        {
            if (selectedOptions == null)
            {
                applicantToUpdate.ApplicantSkills = new List<ApplicantSkill>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var applicantOptionsHS = new HashSet<int>
                (applicantToUpdate.ApplicantSkills.Select(c => c.SkillID));//IDs of the currently selected skills
            foreach (var option in _context.Skills)
            {
                if (selectedOptionsHS.Contains(option.ID.ToString()))
                {
                    if (!applicantOptionsHS.Contains(option.ID))
                    {
                        applicantToUpdate.ApplicantSkills.Add(new ApplicantSkill { ApplicantID = applicantToUpdate.ID, SkillID = option.ID });
                    }
                }
                else
                {
                    if (applicantOptionsHS.Contains(option.ID))
                    {
                        ApplicantSkill conditionToRemove = applicantToUpdate.ApplicantSkills.SingleOrDefault(c => c.SkillID == option.ID);
                        _context.Remove(conditionToRemove);
                    }
                }
            }
        }

        //Broke the Populate approach down into 2 methods for future use.
        private void PopulateDropDownLists(Applicant applicant = null)
        {
            ViewData["RetrainingProgramID"] = RetrainingProgramSelectList(applicant?.RetrainingProgramID);
        }
        private SelectList RetrainingProgramSelectList(int? id)
        {
            var dQuery = from d in _context.RetrainingPrograms
                         orderby d.Name
                         select d;
            return new SelectList(dQuery, "ID", "Name", id);
        }

        private bool ApplicantExists(int id)
        {
            return _context.Applicants.Any(e => e.ID == id);
        }

        private async Task AddPicture(Applicant applicant, IFormFile thePicture)
        {
            //get the picture and save it with the Patient
            if (thePicture != null)
            {
                string mimeType = thePicture.ContentType;
                long fileLength = thePicture.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("image"))
                    {
                        ApplicantPhoto p = new ApplicantPhoto
                        {
                            FileName = Path.GetFileName(thePicture.FileName)
                        };
                        using (var memoryStream = new MemoryStream())
                        {
                            await thePicture.CopyToAsync(memoryStream);
                            p.FileContent.Content = memoryStream.ToArray();
                            p.MimeType = mimeType;
                        }
                        applicant.ApplicantPhoto = p;
                    }
                }
            }
        }

        public FileContentResult Download(int id)
        {
            var theFile = _context.ApplicantDocuments
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .SingleOrDefault();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Applicant applicant, List<IFormFile> theFiles)
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
                        ApplicantDocument d = new ApplicantDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        applicant.ApplicantDocuments.Add(d);
                    };
                }
            }
        }

    }
}
