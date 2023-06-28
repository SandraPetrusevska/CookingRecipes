using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CookingRecipes.Data;
using CookingRecipes.Models;
using CookingRecipes.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Identity;
using CookingRecipes.Areas.Identity.Data;
using static System.Reflection.Metadata.BlobBuilder;

namespace CookingRecipes.Controllers
{
    public class RecipesController : Controller
    {
        private readonly CookingRecipesContext _context;
        readonly IBufferedFileUploadService _bufferedFileUploadService;
        private readonly UserManager<CookingRecipesUser> _userManager;


        public RecipesController(CookingRecipesContext context, IBufferedFileUploadService bufferedFileUploadService, UserManager<CookingRecipesUser> userManager)
        {
            _context = context;
            _bufferedFileUploadService = bufferedFileUploadService;
            _userManager = userManager;
        }

        // GET: Recipes
        public async Task<IActionResult> Index(string? SearchTitle, string? SearchCategory)
        {
            var cookingRecipesContext = _context.Recipe.Include(r => r.Chef);
            IQueryable<Recipe> recipes = _context.Recipe.AsQueryable();

            IQueryable<string> categories = _context.Category.OrderBy(c => c.Id).Select(c => c.CategoryName);
            ViewBag.Categories = new SelectList(categories.ToList());

            if (!string.IsNullOrEmpty(SearchTitle))
            {
                recipes = recipes.Where(s => s.Title.Contains(SearchTitle));
            }
            if (!string.IsNullOrEmpty(SearchCategory))
            {
                recipes = recipes.Where(x => x.RecipeCategories.Any(z => z.Category.CategoryName == SearchCategory));
            }

            recipes = recipes.Include(x => x.Chef)
                             .Include(x => x.Reviews);


            return View(await recipes.ToListAsync());
        }

        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Recipe == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipe
                .Include(r => r.Chef)
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            ViewBag.MyRecipes = false;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            if (userId != null)
            {
                var userBooks = await _context.UserRecipes.Where(r => r.AppUser == userId && r.RecipeId == id).ToListAsync();
                if (userBooks.Any())
                {
                    ViewBag.MyRecipes = true;
                }
            }


            return View(recipe);
        }

        // GET: Recipes/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ChefId"] = new SelectList(_context.Chef, "Id", "FullName");

            var categories = _context.Category.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.CategoryName
            }).ToList();

            ViewData["RecipeCategories"] = categories;
            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Ingredients,Description,Image,DownloadUrl,Time,Serves,ChefId")] Recipe recipe, int[] selectedCategories, IFormFile file, IFormFile filePdf)
        {
            ModelState.Remove("file");
            ModelState.Remove("filePdf");

            if (ModelState.IsValid)
            {
                try
                {
                    recipe.Image = await _bufferedFileUploadService.UploadFile(file);
                    recipe.DownloadUrl = await _bufferedFileUploadService.UploadFile(filePdf);


                    if (!string.IsNullOrEmpty(recipe.Image) || !string.IsNullOrEmpty(recipe.DownloadUrl))
                    {
                        ViewBag.Message = "File Upload Successful";
                    }
                    else
                    {
                        ViewBag.Message = "File Upload Failed";
                    }
                }
                catch (Exception ex)
                {
                    //Log ex
                    ViewBag.Message = "File Upload Failed";
                }
                _context.Add(recipe);
                await _context.SaveChangesAsync();
                if (selectedCategories != null)
                {
                    foreach (var cid in selectedCategories)
                    {
                        var recipeCategory = new RecipeCategory
                        {
                            CategoryId = cid,
                            RecipeId = recipe.Id
                        };
                        _context.Add(recipeCategory);
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChefId"] = new SelectList(_context.Chef, "Id", "FullName", recipe.ChefId);
            return View(recipe);
        }

        // GET: Recipes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Recipe == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipe.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            var recipeCategories = _context.RecipeCategory
               .Where(r => r.RecipeId == recipe.Id)
               .Select(c => c.CategoryId)
               .ToList();

            var categories = _context.Category.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName,
                Selected = recipeCategories.Contains(c.Id)
            }).ToList();

            ViewData["RecipeCategories"] = categories;
            ViewData["ChefId"] = new SelectList(_context.Chef, "Id", "FullName", recipe.ChefId);
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Ingredients,Description,Image,DownloadUrl,Time,Serves,ChefId")] Recipe recipe, int[] selectedCategories, IFormFile file, IFormFile filePdf)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            ModelState.Remove("file");
            ModelState.Remove("filePdf");

            if (ModelState.IsValid)
            {
                var oldRecipe = await _context.Recipe.FindAsync(recipe.Id);

                if (file?.Length > 0)
                {
                    try
                    {
                        recipe.Image = await _bufferedFileUploadService.UploadFile(file);
                        if (!string.IsNullOrEmpty(recipe.Image))
                        {
                            ViewBag.Message = "File Upload Successful";
                        }
                        else
                        {
                            ViewBag.Message = "File Upload Failed";
                        }
                    }
                    catch (Exception ex)
                    {
                        //Log ex
                        ViewBag.Message = "File Upload Failed";
                    }
                }
                else
                {
                    recipe.Image = oldRecipe.Image;
                }

                if (filePdf?.Length > 0)
                {
                    try
                    {
                        recipe.DownloadUrl = await _bufferedFileUploadService.UploadFile(filePdf);
                        if (!string.IsNullOrEmpty(recipe.DownloadUrl))
                        {
                            ViewBag.Message = "File Upload Successful";
                        }
                        else
                        {
                            ViewBag.Message = "File Upload Failed";
                        }
                    }
                    catch (Exception ex)
                    {
                        //Log ex
                        ViewBag.Message = "File Upload Failed";
                    }
                }
                else
                {
                    recipe.DownloadUrl = oldRecipe.DownloadUrl;
                }

                try
                {
                    _context.Entry(oldRecipe).State = EntityState.Detached;
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();
                    if (selectedCategories != null)
                    {
                        var old = _context.RecipeCategory.Where(r => r.RecipeId == recipe.Id);
                        _context.RecipeCategory.RemoveRange(old);
                        foreach (var cId in selectedCategories)
                        {
                            var recipeCategory = new RecipeCategory
                            {
                                CategoryId = cId,
                                RecipeId = recipe.Id
                            };
                            _context.Update(recipeCategory);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.Id))
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

            ViewData["ChefId"] = new SelectList(_context.Chef, "Id", "FullName", recipe.ChefId);

            
            var categories = _context.Category.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName
            }).ToList(); 

            ViewData["RecipeCategories"] = categories; 

            return View(recipe);
        }

        // GET: Recipes/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Recipe == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipe
                .Include(r => r.Chef)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Recipe == null)
            {
                return Problem("Entity set 'CookingRecipesContext.Recipe'  is null.");
            }
            var recipe = await _context.Recipe.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipe.Remove(recipe);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(int id)
        {
          return (_context.Recipe?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Add(int id)
        {
            if (id == null || _context.Recipe == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;

            var userRecipe = new UserRecipes
            {
                AppUser = userId,
                RecipeId = id
            };

            _context.UserRecipes.Add(userRecipe);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyRecipes");
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyRecipes(string SearchCategory, string SearchTitle)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;

            IQueryable<string> categories = _context.Category.OrderBy(c => c.Id).Select(c => c.CategoryName);
            ViewBag.Categories = new SelectList(categories.ToList());

            IQueryable<Recipe> recipes = _context.Recipe.AsQueryable();

            IQueryable<string> genreQuery = _context.Category.OrderBy(r => r.Id).Select(r => r.CategoryName).Distinct();

            if (!string.IsNullOrEmpty(SearchTitle))
            {
                recipes = recipes.Where(s => s.Title.Contains(SearchTitle));
            }

            if (!string.IsNullOrEmpty(SearchCategory))
            {
                recipes = recipes.Where(x => x.RecipeCategories.Any(c => c.Category.CategoryName == SearchCategory));
            }

            recipes = recipes.Include(m => m.Chef)
                         .Include(m => m.Reviews)
                         .Where(r => r.UserRecipes.Any(u => u.AppUser == userId));


            return View(recipes);
        }
    }
}
