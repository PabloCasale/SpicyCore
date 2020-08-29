using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpicyCore.Data;
using SpicyCore.Models;
using SpicyCore.Models.ViewModels;
using SpicyCore.Utility;

namespace SpicyCore.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            this._db = db;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel indexVM = new IndexViewModel()
            {
                MenuItems = await _db.MenuItems.Include(x => x.Categories).Include(x => x.SubCategories).ToListAsync(),
                Categories = await _db.Categories.ToListAsync(),
                Coupons = await _db.Coupons.Where(x => x.IsActive == true).ToListAsync()
            };

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var count = _db.ShoppingCarts.Where(x => x.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);
            }

            return View(indexVM);
        }


        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var menuItemsFromDb = await _db.MenuItems
                .Include(x => x.Categories)
                .Include(x => x.SubCategories)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            ShoppingCart cart = new ShoppingCart()
            {
                MenuItem = menuItemsFromDb,
                MenuItemId = menuItemsFromDb.Id
            };

            return View(cart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart cart)
        {
            cart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _db.ShoppingCarts
                                            .Where(x => x.ApplicationUserId == cart.ApplicationUserId
                                            && x.MenuItemId == cart.MenuItemId)
                                            .FirstOrDefaultAsync();
                if (cartFromDb == null)
                {
                    await _db.ShoppingCarts.AddAsync(cart);
                }
                else
                {
                    cartFromDb.Count += cart.Count;
                }
                await _db.SaveChangesAsync();

                var count = _db.ShoppingCarts.Where(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);
                return RedirectToAction("Index");
            }
            else
            {
                var menuItemsFromDb = await _db.MenuItems
                .Include(x => x.Categories)
                .Include(x => x.SubCategories)
                .Where(x => x.Id == cart.MenuItemId)
                .FirstOrDefaultAsync();

                ShoppingCart cartObj = new ShoppingCart()
                {
                    MenuItem = menuItemsFromDb,
                    MenuItemId = menuItemsFromDb.Id
                };

                return View(cartObj);
            }

        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
