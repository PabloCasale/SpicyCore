using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpicyCore.Data;
using SpicyCore.Models;
using SpicyCore.Models.ViewModels;

namespace SpicyCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoryController(ApplicationDbContext db)
        {
            this._db = db;
        }

        //GET INDEX
        public async Task<IActionResult> Index()
        {
            var subcategories = await _db.SubCategories.Include(x=>x.Categories).ToListAsync();
            return View(subcategories);
        }

        //GET - CREATE
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategories.
                                        OrderBy(x => x.Name).
                                        Select(x => x.Name).
                                        Distinct().
                                        ToListAsync()
            };

            return View(model);
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategories
                    .Include(x => x.Categories)
                    .Where(x => x.Name == model.SubCategory.Name && x.Categories.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    this.StatusMessage = $"Error: Sub Category exist under {doesSubCategoryExists.First().Categories.Name} category. Please use another name";
                }
                else
                {
                    _db.SubCategories.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategories
                                        .OrderBy(x => x.Name)
                                        .Select(x => x.Name)
                                        .ToListAsync(),
                StatusMessage = this.StatusMessage
            };
            return View(modelVM);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await (from subCategory in _db.SubCategories
                             where subCategory.CategoryId == id
                             select subCategory).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));

        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategories.SingleOrDefaultAsync(x=>x.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategories.
                                        OrderBy(x => x.Name).
                                        Select(x => x.Name).
                                        Distinct().
                                        ToListAsync()
            };

            return View(model);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategories
                    .Include(x => x.Categories)
                    .Where(x => x.Name == model.SubCategory.Name && x.Categories.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    this.StatusMessage = $"Error: Sub Category exist under {doesSubCategoryExists.First().Categories.Name} category. Please use another name";
                }
                else
                {
                    var subCategoryFromDB = await _db.SubCategories.FindAsync(id);
                    subCategoryFromDB.Name = model.SubCategory.Name;

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategories
                                        .OrderBy(x => x.Name)
                                        .Select(x => x.Name)
                                        .ToListAsync(),
                StatusMessage = this.StatusMessage
            };
            //101
            modelVM.SubCategory.Id = id;
            return View(modelVM);
        }
    }
}