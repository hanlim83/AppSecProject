using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminSide.Data;
using AdminSide.Models;

namespace AdminSide.Controllers
{
    public class CategoryDefaultsController : Controller
    {
        private readonly CompetitionContext _context;

        public CategoryDefaultsController(CompetitionContext context)
        {
            _context = context;
        }

        // GET: CategoryDefaults
        public async Task<IActionResult> Index()
        {
            return View(await _context.CategoryDefault.ToListAsync());
        }

        // GET: CategoryDefaults/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryDefault = await _context.CategoryDefault
                .FirstOrDefaultAsync(m => m.ID == id);
            if (categoryDefault == null)
            {
                return NotFound();
            }

            return View(categoryDefault);
        }

        // GET: CategoryDefaults/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoryDefaults/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CategoryName")] CategoryDefault categoryDefault)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoryDefault);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryDefault);
        }

        // GET: CategoryDefaults/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryDefault = await _context.CategoryDefault.FindAsync(id);
            if (categoryDefault == null)
            {
                return NotFound();
            }
            return View(categoryDefault);
        }

        // POST: CategoryDefaults/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CategoryName")] CategoryDefault categoryDefault)
        {
            if (id != categoryDefault.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoryDefault);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryDefaultExists(categoryDefault.ID))
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
            return View(categoryDefault);
        }

        // GET: CategoryDefaults/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryDefault = await _context.CategoryDefault
                .FirstOrDefaultAsync(m => m.ID == id);
            if (categoryDefault == null)
            {
                return NotFound();
            }

            return View(categoryDefault);
        }

        // POST: CategoryDefaults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoryDefault = await _context.CategoryDefault.FindAsync(id);
            _context.CategoryDefault.Remove(categoryDefault);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryDefaultExists(int id)
        {
            return _context.CategoryDefault.Any(e => e.ID == id);
        }
    }
}
