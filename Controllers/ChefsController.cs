using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CookingRecipes.Data;
using CookingRecipes.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace CookingRecipes.Controllers
{
    public class ChefsController : Controller
    {
        private readonly CookingRecipesContext _context;

        public ChefsController(CookingRecipesContext context)
        {
            _context = context;
        }

        // GET: Chefs
        public async Task<IActionResult> Index(string? SearchFirstName, string? SearchLastName, string? SearchNationality)
        {
            IQueryable<Chef> chefs = _context.Chef.AsQueryable();
            if (!string.IsNullOrEmpty(SearchFirstName))
            {
                chefs = chefs.Where(s => s.FirstName.Contains(SearchFirstName));
            }
            if (!string.IsNullOrEmpty(SearchLastName))
            {
                chefs = chefs.Where(x => x.LastName.Contains(SearchLastName));
            }
            if (!string.IsNullOrEmpty(SearchNationality))
            {
                chefs = chefs.Where(x => x.Nationality.Contains(SearchNationality));
            }
            //movies = movies.Include(m => m.Director);
            return View(await chefs.ToListAsync());
        }

        // GET: Chefs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Chef == null)
            {
                return NotFound();
            }

            var chef = await _context.Chef
                .Include(r => r.Recipes)
                .ThenInclude(r => r.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chef == null)
            {
                return NotFound();
            }

            return View(chef);
        }

        // GET: Chefs/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chefs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Nationality,Gender")] Chef chef)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chef);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chef);
        }

        // GET: Chefs/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Chef == null)
            {
                return NotFound();
            }

            var chef = await _context.Chef.FindAsync(id);
            if (chef == null)
            {
                return NotFound();
            }
            return View(chef);
        }

        // POST: Chefs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,BirthDate,Nationality,Gender")] Chef chef)
        {
            if (id != chef.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chef);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChefExists(chef.Id))
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
            return View(chef);
        }

        // GET: Chefs/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Chef == null)
            {
                return NotFound();
            }

            var chef = await _context.Chef
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chef == null)
            {
                return NotFound();
            }

            return View(chef);
        }

        // POST: Chefs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Chef == null)
            {
                return Problem("Entity set 'CookingRecipesContext.Chef'  is null.");
            }
            var chef = await _context.Chef.FindAsync(id);
            if (chef != null)
            {
                _context.Chef.Remove(chef);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChefExists(int id)
        {
          return (_context.Chef?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
