﻿using System;
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

            var challenge = await _context.Challenges
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

            var temp_challenge = await _context.Challenges
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

        private bool ChallengeExists(int id)
        {
            return _context.Challenges.Any(e => e.ID == id);
        }
    }
}
