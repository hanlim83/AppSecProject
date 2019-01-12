using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminSide.Data;
using AdminSide.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.IO;
using Amazon.S3.Transfer;
using Amazon.S3;

namespace AdminSide.Controllers
{
    public class ChallengesController : Controller
    {
        IAmazonS3 S3Client { get; set; }

        private readonly CompetitionContext _context;

        public ChallengesController(CompetitionContext context, IAmazonS3 s3Client)
        {
            _context = context;
            this.S3Client = s3Client;
        }

        // GET: Challenges
        public async Task<IActionResult> Index(int? id)
        {
            ViewData["NavigationShowAll"] = true;
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .Include(c1 => c1.Challenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);

            //return View(await _context.Challenges.ToListAsync());
        }

        // GET: Challenges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["NavigationShowAll"] = true;
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenges
                .FirstOrDefaultAsync(m => m.ID == id);
            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        //Update Details code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, [Bind("ID,Name,Description,Value,Flag,CompetitionID,CompetitionCategoryID")] Challenge challenge)
        {
            if (id != challenge.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(challenge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChallengeExists(challenge.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Challenges", new { id = challenge.CompetitionID });
            }
            return View(challenge);
        }

        // GET: Challenges/Create
        public async Task<IActionResult> Create(int? id)
        {
            ViewData["NavigationShowAll"] = true;
            if (id == null)
            {
                return NotFound();
            }

            var vm = new ChallengesViewModelIEnumerable();

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .Include(c1 => c1.Challenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            vm.Competition = competition;
            var dictionary = new Dictionary<int, string>
            {

            };
            foreach (var category in competition.CompetitionCategories)
            {
                //vm.CategoriesList.Add(new SelectListItem { Value = categoryDefault.CategoryName, Text = categoryDefault.CategoryName });
                dictionary.Add(category.ID, category.CategoryName);
            }
            ViewBag.SelectList = new SelectList(dictionary, "Key", "Value");

            return View(vm);
            //return View();
        }

        // POST: Challenges/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,Value,Flag,FileName,CompetitionID,CompetitionCategoryID")] Challenge challenge, List<IFormFile> files)
        {
            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .Include(c1 => c1.Challenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == challenge.CompetitionID);

            foreach (var category in competition.CompetitionCategories)
            {
                if (category.ID == challenge.CompetitionCategoryID)
                {
                    foreach (var file in files)
                    {
                        challenge.FileName = file.FileName;
                        await UploadFileToS3(file, competition.BucketName, category.CategoryName);
                    }
                }
            }
            //Testing codes
            
            //Testing codes

            if (ModelState.IsValid)
            {
                //call method to add file
                _context.Add(challenge);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Challenges", new { id = challenge.CompetitionID });
            }
            return View(challenge);
        }

        //Upload file to S3
        public async Task UploadFileToS3(IFormFile file, string bucketName, string folderName)
        {
            using (var newMemoryStream = new MemoryStream())
            {
                file.CopyTo(newMemoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = folderName + "/" + file.FileName,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(S3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
        }

        // GET: Challenges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
            {
                return NotFound();
            }
            return View(challenge);
        }

        // POST: Challenges/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description,Value,Flag,CompetitionID,CompetitionCategoryID")] Challenge challenge)
        {
            if (id != challenge.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(challenge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChallengeExists(challenge.ID))
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
            return View(challenge);
        }

        // GET: Challenges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenges
                .FirstOrDefaultAsync(m => m.ID == id);
            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }



        // POST: Challenges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChallengeExists(int id)
        {
            return _context.Challenges.Any(e => e.ID == id);
        }
    }
}
