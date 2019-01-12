using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using UserSide.Models;

namespace UserSide.Controllers
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
        }

        // GET: Challenges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenge
                .FirstOrDefaultAsync(m => m.ID == id);
            if (challenge == null)
            {
                return NotFound();
            }
            //Stop field from being populated at View
            challenge.Flag = null;

            var competition = await _context.Competitions.FindAsync(challenge.CompetitionID);
            string bucketName = competition.BucketName;
            var category = await _context.CompetitionCategories.FindAsync(challenge.CompetitionCategoryID);
            string folderName = category.CategoryName;
            string fileName = challenge.FileName;
            Regex pattern = new Regex("[+]");
            string tempFileName = pattern.Replace(fileName, "%2B");
            tempFileName.Replace(' ', '+');

            ViewData["FileLink"] = "https://s3-ap-southeast-1.amazonaws.com/" + bucketName + "/" + folderName + "/" + tempFileName;

            return View(challenge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details([Bind("Flag")] Challenge challenge, int? id)
        {
            //if (ModelState.IsValid)
            //{
            //    _context.Add(challenge);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}
            if (id == null)
            {
                return NotFound();
            }

            var temp_challenge = await _context.Challenge
                .FirstOrDefaultAsync(m => m.ID == id);
            if (temp_challenge == null)
            {
                return NotFound();
            }

            if (challenge.Flag.Equals(temp_challenge.Flag))
            {
                //Flag is correct
                //Add points to score and stuff
                return RedirectToAction("Index", "Challenges", new { id });
            }
            //Wrong flag
            return RedirectToAction("Details", "Challenges", new { id });
            //return View(id);
        }

        // GET: Challenges/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Challenges/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,Value,Flag,CompetitionID,CompetitionCategoryID")] Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(challenge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(challenge);
        }

        // GET: Challenges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenge.FindAsync(id);
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

            var challenge = await _context.Challenge
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
            var challenge = await _context.Challenge.FindAsync(id);
            _context.Challenge.Remove(challenge);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChallengeExists(int id)
        {
            return _context.Challenge.Any(e => e.ID == id);
        }
    }
}
