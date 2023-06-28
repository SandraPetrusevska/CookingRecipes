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
using Microsoft.AspNetCore.Identity;
using CookingRecipes.Areas.Identity.Data;

namespace CookingRecipes.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly CookingRecipesContext _context;
        private readonly UserManager<CookingRecipesUser> _userManager;

        public ReviewsController(CookingRecipesContext context, UserManager<CookingRecipesUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.UserName;

            if (userId != null)
            {
                var reviews = await _context.Review
                    .Where(r => r.AppUser == userId)
                    .Include(r => r.Recipe)
                    .ToListAsync();
                if (reviews != null)
                {
                    return View(reviews);
                }
                else
                {
                    string htmlContent = "<h5>You have not left any reviews</h5>";
                    return Content(htmlContent, "text/html");
                }
            }

            return NotFound();
            //var cookingRecipesContext = _context.Review.Include(r => r.Recipe);
            //return View(await cookingRecipesContext.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Review == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Recipe)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create
        [Authorize(Roles = "User")]
        public async Task <IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;


            if (userId != null)
            {
                var userRecipes = await _context.UserRecipes
                    .Where(b => b.AppUser == userId)
                    .Select(b => b.Recipe)
                    .ToListAsync();

                ViewData["RecipeId"] = new SelectList(userRecipes, "Id", "Title");
            }
            
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([Bind("Id,AppUser,Comment,Rating,RecipeId")] Review review)
        {
            ModelState.Remove("AppUser");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                review.AppUser = user?.UserName;

                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecipeId"] = new SelectList(_context.Recipe, "Id", "Title", review.RecipeId);
            return View(review);
        }

        // GET: Reviews/Edit/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Review == null)
            {
                return NotFound();
            }

            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var username = user?.UserName;

            if (review.AppUser != username)
            {
                return Forbid();
            }

            ViewData["RecipeId"] = new SelectList(_context.Recipe, "Id", "Title", review.RecipeId);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUser,Comment,Rating,RecipeId")] Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
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
            ViewData["RecipeId"] = new SelectList(_context.Recipe, "Id", "Title", review.RecipeId);
            return View(review);
        }

        // GET: Reviews/Delete/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Review == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Recipe)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var username = user?.UserName;

            if (review.AppUser != username)
            {
                return Forbid();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Review == null)
            {
                return Problem("Entity set 'CookingRecipesContext.Review'  is null.");
            }
            var review = await _context.Review.FindAsync(id);
            if (review != null)
            {
                _context.Review.Remove(review);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
          return (_context.Review?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
