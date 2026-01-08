using Data;
using Harvest_Hub.ViewModels;
using HarvestHub.WebApp.Models;
using HarvestHub.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.WebApp.Controllers
{
    public class FertilizerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FertilizerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fertilizer/Index
        public async Task<IActionResult> Index(int? categoryId, string? search)
        {
            var viewModel = new FertilizerMarketplaceViewModel
            {
                Categories = await _context.FertilizerCategories
                    .Where(c => c.IsActive)
                    .ToListAsync(),
                Cities = await _context.Cities
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                SelectedCategoryId = categoryId,
                SearchQuery = search
            };

            var productsQuery = _context.FertilizerProducts
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(search) ||
                    p.Brand.Contains(search) ||
                    p.Description.Contains(search));
            }

            viewModel.Products = await productsQuery.ToListAsync();

            return View(viewModel);
        }

        // GET: Fertilizer/ProductDetails/5
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.FertilizerProducts
                .Include(p => p.Category)
                .Include(p => p.StoreProducts)
                    .ThenInclude(sp => sp.Store)
                        .ThenInclude(s => s.City)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Fertilizer/StoreDirectory
        public async Task<IActionResult> StoreDirectory(int? cityId, string? search)
        {
            var viewModel = new StoreDirectoryViewModel
            {
                Cities = await _context.Cities
                    .Where(c => c.IsActive && c.Region == "South Punjab")
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                SelectedCityId = cityId,
                SearchQuery = search
            };

            var storesQuery = _context.AgriSupplyStores
                .Include(s => s.City)
                .Where(s => s.IsActive);

            if (cityId.HasValue)
            {
                storesQuery = storesQuery.Where(s => s.CityId == cityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                storesQuery = storesQuery.Where(s =>
                    s.StoreName.Contains(search) ||
                    s.OwnerName.Contains(search) ||
                    s.Address.Contains(search));
            }

            viewModel.Stores = await storesQuery.ToListAsync();

            return View(viewModel);
        }

        // GET: Fertilizer/StoreDetails/5
        public async Task<IActionResult> StoreDetails(int id)
        {
            var store = await _context.AgriSupplyStores
                .Include(s => s.City)
                .Include(s => s.StoreProducts)
                    .ThenInclude(sp => sp.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
            {
                TempData["ErrorMessage"] = "Store not found.";
                return RedirectToAction(nameof(StoreDirectory));
            }

            return View(store);
        }
    }
}
