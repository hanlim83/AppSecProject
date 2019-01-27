using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminSide.Data;
using AdminSide.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using Amazon.S3.Transfer;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using System;

namespace AdminSide.Controllers
{
    [Authorize]
    //the line above makes a page protected and will redirect user back to login
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
                .ThenInclude(c1 => c1.Challenges)
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

            ViewData["CompetitionID"] = challenge.CompetitionID;

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .ThenInclude(cc => cc.Challenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == challenge.CompetitionID);

            var dictionary = new Dictionary<int, string>
            {

            };
            foreach (var category in competition.CompetitionCategories)
            {
                dictionary.Add(category.ID, category.CategoryName);
            }
            ViewBag.SelectList = new SelectList(dictionary, "Key", "Value");

            return View(challenge);
        }

        //Update Details code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, [Bind("ID,Name,Description,Value,Flag,CompetitionID,CompetitionCategoryID")] Challenge challenge, List<IFormFile> files)
        {
            if (id != challenge.ID)
            {
                return NotFound();
            }

            if (files.Count != 0)
            {
                var competition = await _context.Competitions
                    .Include(c => c.CompetitionCategories)
                    .ThenInclude(cc => cc.Challenges)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ID == challenge.CompetitionID);

                var competitionCategory = await _context.CompetitionCategories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ID == challenge.CompetitionCategoryID);

                var temp_Challenge = await _context.Challenges
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ID == challenge.ID);

                await DeleteFile(competition.BucketName, competitionCategory.CategoryName, temp_Challenge.FileName);

                foreach (var file in files)
                {
                    challenge.FileName = file.FileName;
                    await UploadFileToS3(file, competition.BucketName, competitionCategory.CategoryName);
                }
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

        private async Task DeleteFile(string bucketName, string folderName, string filename)
        {
            try
            {
                DeleteObjectsRequest request2 = new DeleteObjectsRequest();
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = folderName + "/" + filename

                };

                ListObjectsResponse response = await S3Client.ListObjectsAsync(request);
                // Process response.
                foreach (S3Object entry in response.S3Objects)
                {

                    request2.AddKey(entry.Key);
                }
                request2.BucketName = bucketName;
                DeleteObjectsResponse response2 = await S3Client.DeleteObjectsAsync(request2);

            }
            catch (AmazonS3Exception e)
            {
                //Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        // GET: Challenges/Create
        public async Task<IActionResult> Create(int? id)
        {
            ViewData["NavigationShowAll"] = true;
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .ThenInclude(cc => cc.Challenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            ViewData["CompetitionID"] = competition.ID;
            var dictionary = new Dictionary<int, string>
            {

            };
            foreach (var category in competition.CompetitionCategories)
            {
                dictionary.Add(category.ID, category.CategoryName);
            }
            ViewBag.SelectList = new SelectList(dictionary, "Key", "Value");

            return View();
        }

        // POST: Challenges/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Value,Flag,FileName,CompetitionID,CompetitionCategoryID")] Challenge challenge, List<IFormFile> files)
        {
            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .ThenInclude(cc => cc.Challenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == challenge.CompetitionID);

            var competitionCategory = await _context.CompetitionCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == challenge.CompetitionCategoryID);

            if (ModelState.IsValid)
            {
                //Check this
                //foreach (var category in competition.CompetitionCategories)
                //{
                //    if (category.ID == challenge.CompetitionCategoryID)
                //    {
                        foreach (var file in files)
                        {
                            challenge.FileName = file.FileName;
                            await UploadFileToS3(file, competition.BucketName, competitionCategory.CategoryName);
                        }
                //    }
                //}

                _context.Challenges.Add(challenge);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Challenges", new { id = challenge.CompetitionID });
            }

            ViewData["CompetitionID"] = competition.ID;
            var dictionary = new Dictionary<int, string>
            {

            };
            foreach (var category in competition.CompetitionCategories)
            {
                dictionary.Add(category.ID, category.CategoryName);
            }
            ViewBag.SelectList = new SelectList(dictionary, "Key", "Value");

            return View();
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
            await DeleteFileAsync(challenge.CompetitionID, challenge.CompetitionCategoryID, challenge.FileName);
            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Challenges", new { id = challenge.CompetitionID });
        }

        private async Task DeleteFileAsync(int competitionID, int competitionCategoryID, string fileName)
        {
            var competition = await _context.Competitions.FindAsync(competitionID);
            string bucketName = competition.BucketName;
            var category = await _context.CompetitionCategories.FindAsync(competitionCategoryID);
            string folderName = category.CategoryName;
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = folderName + "/" + fileName
            };

            await S3Client.DeleteObjectAsync(deleteObjectRequest);
        }

        private bool ChallengeExists(int id)
        {
            return _context.Challenges.Any(e => e.ID == id);
        }
    }
}
