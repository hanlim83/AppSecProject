using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class SubnetsController : Controller
    {
        private readonly PlatformResourcesContext _context;

        public SubnetsController(PlatformResourcesContext context)
        {
            _context = context;
        }

        // GET: PlatformManagement/Subnets
        public async Task<IActionResult> Index()
        {
            return View(await _context.Subnets.ToListAsync());
        }

        // GET: PlatformManagement/Subnets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subnet = await _context.Subnets
                .FirstOrDefaultAsync(m => m.ID == id);
            if (subnet == null)
            {
                return NotFound();
            }

            return View(subnet);
        }

        // GET: PlatformManagement/Subnets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PlatformManagement/Subnets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Type,IPv4CIDR,IPv6CIDR,AWSVPCSubnetReference")] Subnet subnet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subnet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subnet);
        }

        // GET: PlatformManagement/Subnets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subnet = await _context.Subnets.FindAsync(id);
            if (subnet == null)
            {
                return NotFound();
            }
            return View(subnet);
        }

        // POST: PlatformManagement/Subnets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Type,IPv4CIDR,IPv6CIDR,AWSVPCSubnetReference")] Subnet subnet)
        {
            if (id != subnet.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subnet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubnetExists(subnet.ID))
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
            return View(subnet);
        }

        // GET: PlatformManagement/Subnets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subnet = await _context.Subnets
                .FirstOrDefaultAsync(m => m.ID == id);
            if (subnet == null)
            {
                return NotFound();
            }

            return View(subnet);
        }

        // POST: PlatformManagement/Subnets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subnet = await _context.Subnets.FindAsync(id);
            _context.Subnets.Remove(subnet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubnetExists(int id)
        {
            return _context.Subnets.Any(e => e.ID == id);
        }
    }
}
