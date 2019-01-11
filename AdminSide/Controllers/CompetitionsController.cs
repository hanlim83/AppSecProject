using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminSide.Data;
using AdminSide.Models;
using Amazon.S3.Model;
using Amazon.S3;
using System.Net;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace AdminSide.Controllers
{
    public class CompetitionsController : Controller
    {
        IAmazonS3 S3Client { get; set; }

        private readonly CompetitionContext _context;

        public CompetitionsController(CompetitionContext context, IAmazonS3 s3Client)
        {
            _context = context;
            this.S3Client = s3Client;
        }

        // GET: Competitions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Competitions.ToListAsync());
        }

        // GET: Competitions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["NavigationShowAll"] = true;
            //Testing for dynamic navbar
            ViewData["routeID"] = id;
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);
        }

        // GET: Competitions/Create
        public IActionResult Create()
        {
            var vm = new CategoriesViewModelIEnumerable();
            //vm.PopulateCategoriesList();
            foreach (var categoryDefault in _context.CategoryDefault)
            {
                vm.CategoriesList.Add(new SelectListItem { Value = categoryDefault.CategoryName, Text = categoryDefault.CategoryName });
            }
            return View(vm);
        }

        // POST: Competitions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ID,CompetitionName,Status,BucketName")] Competition competition, [Bind("CategoryName")] CompetitionCategory competitionCategory, string[] SelectedCategories)
        public async Task<IActionResult> Create(CategoriesViewModelIEnumerable model)
        //public async Task<IActionResult> Create(string[] CategoriesList)
        {
            /*foreach (var CategoryName in model.SelectedCategories)
            {
                Console.WriteLine(CategoryName);
            }*/
            //Console.WriteLine(model.competition.BucketName);
            //Tested and working^
            //Console.WriteLine(model.SelectedCategories.ElementAt(0));
            //Console.WriteLine(competitionCategory.Categories.ElementAt(0).CategoryName);
            
            if (ModelState.IsValid)
            {
                //_context.Add(model.competition);
                //await _context.SaveChangesAsync();
                //var competitionID = model.competition.ID;

                //CompetitionCategory competitionCategory = new CompetitionCategory();


                //Generate bucketname programtically
                model.Competition.BucketName = GenerateBucketName(model.Competition.CompetitionName);
                model.Competition.CompetitionCategories = new Collection<CompetitionCategory>();
                foreach (var CategoryName in model.SelectedCategories)
                {
                    //model.competitionCategory = _context.CompetitionCategories.Find("CompetitionID");
                    //model.competitionCategory.CompetitionID = model.competition.ID;
                    //model.competitionCategory.CategoryName = CategoryName;

                    //competitionCategory.CategoryName = CategoryName;
                    //competitionCategory.CompetitionID = model.competition.ID;
                    
                    //CompetitionCategoriesTempList.Add

                    model.Competition.CompetitionCategories.Add(new CompetitionCategory { CompetitionID=model.Competition.ID, CategoryName=CategoryName});
                    
                    //competitionCategory.competitionID = _context.Competitions.Find("ID");\
                    //model.competition = new Competition();
                    //model.competition.CompetitionCategories.Add(new CompetitionCategory { CompetitionID = model.competition.ID, CategoryName = CategoryName });
                    //await _context.SaveChangesAsync();
                }
                _context.Add(model.Competition);
                await _context.SaveChangesAsync();
                try
                {
                    PutBucketResponse response = await S3Client.PutBucketAsync(model.Competition.BucketName);
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        //return RedirectToAction("");
                    }
                    else
                    {
                        //return RedirectToAction("");
                        //return RedirectToAction(nameof(Index));
                    }

                }
                catch (AmazonS3Exception e)
                {
                    ViewData["Exception"] = e.Message;
                    //return View();
                }
                //return RedirectToAction(nameof(Index));
            }
            else
            {

            }
            //return View(competition);
            //return View(model);
            //Testing create folder dynamically
            foreach (var CategoryName in model.SelectedCategories)
            {
                CreateFolder(model.Competition.BucketName, CategoryName);
            }
            //Testing create folder dynamically
            return RedirectToAction("Index");
        }

        private string GenerateBucketName(string competitionName)
        {
            //Make bucket name conform to standards (All lower case & no space)
            competitionName = competitionName.Replace(" ", string.Empty);
            competitionName = competitionName.ToLower();
            //Append 15 digit secure random numbers
            //var randIntArray = new int[15];
            string randNumbers = "";
            //for (int i=0; i<15; i++)
            //{
                RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                var byteArray = new byte[4];
                provider.GetBytes(byteArray);

                //convert 4 bytes to an integer
                var randomInteger = BitConverter.ToUInt32(byteArray, 0);
                //randIntArray[i] = randomInteger;
                randNumbers = randNumbers + randomInteger;
            //}
            
            string generatedBucketName = competitionName + randNumbers;
            return generatedBucketName;
        }

        private void CreateFolder(string bucketName, string folderName)
        {
            //string bucketName;
            /*string keyName= folderName + "/";
            string filePath=null;
            var fileTransferUtility = new TransferUtility(S3Client);
            await fileTransferUtility.UploadAsync(filePath, bucketName, keyName);
            Console.WriteLine("Upload 2 completed");*/

            PutObjectRequest putObjectRequest = new PutObjectRequest
            {

                BucketName = bucketName,
                //StorageClass = S3StorageClass.Standard,
                //ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                //CannedACL = S3CannedACL.Private,
                Key = folderName + "/",
                //ContentBody = awsFolderName
            };
            S3Client.PutObjectAsync(putObjectRequest);
        }

        // GET: Competitions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions.FindAsync(id);
            if (competition == null)
            {
                return NotFound();
            }
            return View(competition);
        }

        // POST: Competitions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CompetitionName,Status")] Competition competition)
        {
            if (id != competition.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(competition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompetitionExists(competition.ID))
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
            return View(competition);
        }

        // GET: Competitions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Code testing for category deletion in progress here
            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .FirstOrDefaultAsync(m => m.ID == id);
            //Code testing for category deletion in progress here
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);
        }

        // POST: Competitions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //var competition = await _context.Competitions.FindAsync(id);
            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .FirstOrDefaultAsync(m => m.ID == id);
            foreach (var category in competition.CompetitionCategories)
            {
                await ClearBucket(competition.BucketName, category.CategoryName);
            }
            DeleteBucket(competition.BucketName);
            _context.Competitions.Remove(competition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task ClearBucket(string bucketName, string folderName)
        {
            try
            {
                //Old delete codes
                //var deleteObjectRequest = new DeleteObjectRequest
                //{
                //    BucketName = bucketName,
                //    Key = folderName + "/"
                //};

                //Console.WriteLine("Deleting an object");
                //S3Client.DeleteObjectAsync(deleteObjectRequest);

                //New delete codes
                DeleteObjectsRequest request2 = new DeleteObjectsRequest();
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = folderName

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
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        private void DeleteBucket(string bucketName)
        {
            //Add code to delete everything in the bucket first
            //Can delete on the condition the bucket is empty
            S3Client.DeleteBucketAsync(new DeleteBucketRequest() { BucketName = bucketName, UseClientRegion = true });
        }

        private bool CompetitionExists(int id)
        {
            return _context.Competitions.Any(e => e.ID == id);
        }
    }
}
