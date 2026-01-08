using Data;
using HarvestHub.WebApp.Models;
using HarvestHub.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFertilizerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminFertilizerController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Cities Management

        public async Task<IActionResult> ManageCities()
        {
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
           return View(cities);
        }

        [HttpGet]
        public IActionResult CreateCity()
        {
            return View(new City());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCity(City city)
        {
            if (ModelState.IsValid)
            {
                _context.Cities.Add(city);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "City added successfully!";
                return RedirectToAction(nameof(ManageCities));
            }
            return View(city);
        }

        [HttpGet]
        public async Task<IActionResult> EditCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null) return NotFound();
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(City city)
        {
            if (ModelState.IsValid)
            {
                _context.Cities.Update(city);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "City updated successfully!";
                return RedirectToAction(nameof(ManageCities));
            }
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "City deleted successfully!";
            }
            return RedirectToAction(nameof(ManageCities));
        }

        #endregion

        #region Categories Management

        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _context.FertilizerCategories.ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new FertilizerCategory());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(FertilizerCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.FertilizerCategories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category added successfully!";
                return RedirectToAction(nameof(ManageCategories));
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.FertilizerCategories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(FertilizerCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.FertilizerCategories.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(ManageCategories));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.FertilizerCategories.FindAsync(id);
            if (category != null)
            {
                _context.FertilizerCategories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            return RedirectToAction(nameof(ManageCategories));
        }

        #endregion

        #region Products Management

        public async Task<IActionResult> ManageFertilizerProducts(int? categoryId)
        {
            var query = _context.FertilizerProducts
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Categories = await _context.FertilizerCategories.ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.FertilizerCategories.ToListAsync();
            return View(new FertilizerProduct());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(FertilizerProduct product, IFormFile? imageFile)
        {
            try
            {
                // ===== DEBUGGING: Log what we received =====
                Console.WriteLine($"=== CREATE PRODUCT DEBUG ===");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"CategoryId: {product.CategoryId}");
                Console.WriteLine($"Brand: {product.Brand}");
                Console.WriteLine($"IsActive: {product.IsActive}");
                Console.WriteLine($"CreatedAt: {product.CreatedAt}");
                
                // Set default values BEFORE validation
                if (product.CreatedAt == default(DateTime))
                {
                    product.CreatedAt = DateTime.UtcNow;
                }
                if (!product.IsActive)
                {
                    product.IsActive = true;
                }
                
                // Check ModelState
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== VALIDATION ERRORS ===");
                    var errors = new List<string>();
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var errorMsg = $"{modelStateKey}: {error.ErrorMessage}";
                            Console.WriteLine(errorMsg);
                            errors.Add(errorMsg);
                        }
                    }
                    
                    // Show errors to user
                    TempData["ErrorMessage"] = "Validation Failed: " + string.Join(", ", errors);
                    ViewBag.Categories = await _context.FertilizerCategories.ToListAsync();
                    return View(product);
                }
                
                // Handle image upload
                if(imageFile != null && imageFile.Length > 0)
                {
                    var imagesFolder = Path.Combine("wwwroot", "fertilizer-images");
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    var imagePath = Path.Combine(imagesFolder, Guid.NewGuid() + Path.GetExtension(imageFile.FileName));
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/fertilizer-images/" + Path.GetFileName(imagePath);
                }

                Console.WriteLine("=== SAVING TO DATABASE ===");
                _context.FertilizerProducts.Add(product);
                await _context.SaveChangesAsync();
                Console.WriteLine("=== SAVE SUCCESSFUL ===");
                
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction(nameof(ManageFertilizerProducts));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EXCEPTION ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                ViewBag.Categories = await _context.FertilizerCategories.ToListAsync();
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.FertilizerProducts.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.FertilizerCategories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(FertilizerProduct product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imagesFolder = Path.Combine("wwwroot", "fertilizer-images");
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    var imagePath = Path.Combine(imagesFolder, Guid.NewGuid() + Path.GetExtension(imageFile.FileName));
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/fertilizer-images/" + Path.GetFileName(imagePath);
                }

                _context.FertilizerProducts.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(ManageFertilizerProducts));
            }

            // Log validation errors for debugging
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
            }

            ViewBag.Categories = await _context.FertilizerCategories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.FertilizerProducts.FindAsync(id);
            if (product != null)
            {
                _context.FertilizerProducts.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }
            return RedirectToAction(nameof(ManageFertilizerProducts));
        }

        #endregion

        #region Stores Management

        public async Task<IActionResult> ManageStores(int? cityId)
        {
            var query = _context.AgriSupplyStores
                .Include(s => s.City)
                .AsQueryable();

            if (cityId.HasValue)
            {
                query = query.Where(s => s.CityId == cityId.Value);
            }

            var stores = await query.OrderBy(s => s.StoreName).ToListAsync();
            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.SelectedCityId = cityId;

            return View(stores);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStore()
        {
            var viewModel = new CreateStoreViewModel
            {
                Cities = await _context.Cities.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStore(CreateStoreViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var store = new AgriSupplyStore
                {
                    StoreName = viewModel.StoreName,
                    StoreType = viewModel.StoreType,
                    OwnerName = viewModel.OwnerName,
                    CityId = viewModel.CityId,
                    Address = viewModel.Address,
                    ContactNumber = viewModel.ContactNumber,
                    WhatsAppNumber = viewModel.WhatsAppNumber,
                    Email = viewModel.Email,
                    Latitude = viewModel.Latitude,
                    Longitude = viewModel.Longitude,
                    IsVerified = viewModel.IsVerified,
                    IsActive = viewModel.IsActive
                };

                _context.AgriSupplyStores.Add(store);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Store added successfully!";
                return RedirectToAction(nameof(ManageStores));
            }

            viewModel.Cities = await _context.Cities.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditStore(int id)
        {
            var store = await _context.AgriSupplyStores.FindAsync(id);
            if (store == null) return NotFound();

            var viewModel = new CreateStoreViewModel
            {
                Id = store.Id,
                StoreName = store.StoreName,
                StoreType = store.StoreType,
                OwnerName = store.OwnerName,
                CityId = store.CityId,
                Address = store.Address,
                ContactNumber = store.ContactNumber,
                WhatsAppNumber = store.WhatsAppNumber,
                Email = store.Email,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                IsVerified = store.IsVerified,
                IsActive = store.IsActive,
                Cities = await _context.Cities.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStore(CreateStoreViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var store = await _context.AgriSupplyStores.FindAsync(viewModel.Id);
                if (store == null) return NotFound();

                store.StoreName = viewModel.StoreName;
                store.StoreType = viewModel.StoreType;
                store.OwnerName = viewModel.OwnerName;
                store.CityId = viewModel.CityId;
                store.Address = viewModel.Address;
                store.ContactNumber = viewModel.ContactNumber;
                store.WhatsAppNumber = viewModel.WhatsAppNumber;
                store.Email = viewModel.Email;
                store.Latitude = viewModel.Latitude;
                store.Longitude = viewModel.Longitude;
                store.IsVerified = viewModel.IsVerified;
                store.IsActive = viewModel.IsActive;

                _context.AgriSupplyStores.Update(store);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Store updated successfully!";
                return RedirectToAction(nameof(ManageStores));
            }

            viewModel.Cities = await _context.Cities.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.AgriSupplyStores.FindAsync(id);
            if (store != null)
            {
                _context.AgriSupplyStores.Remove(store);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Store deleted successfully!";
            }
            return RedirectToAction(nameof(ManageStores));
        }

        #endregion

        #region Store-Product Management

        [HttpGet]
        public async Task<IActionResult> ManageStoreProducts(int storeId)
        {
            var store = await _context.AgriSupplyStores
                .Include(s => s.City)
                .Include(s => s.StoreProducts)
                    .ThenInclude(sp => sp.Product)
                .FirstOrDefaultAsync(s => s.Id == storeId);

            if (store == null) return NotFound();

            var linkedProductIds = store.StoreProducts.Select(sp => sp.ProductId).ToList();
            var availableProducts = await _context.FertilizerProducts
                .Where(p => !linkedProductIds.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            var viewModel = new ManageStoreProductsViewModel
            {
                Store = store,
                StoreProducts = store.StoreProducts.ToList(),
                AvailableProducts = availableProducts
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToStore(int storeId, int productId, decimal price)
        {
            var storeProduct = new StoreProduct
            {
                StoreId = storeId,
                ProductId = productId,
                Price = price,
                IsInStock = true
            };

            _context.StoreProducts.Add(storeProduct);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product added to store successfully!";

            return RedirectToAction(nameof(ManageStoreProducts), new { storeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProductFromStore(int storeId, int productId)
        {
            var storeProduct = await _context.StoreProducts
                .FirstOrDefaultAsync(sp => sp.StoreId == storeId && sp.ProductId == productId);

            if (storeProduct != null)
            {
                _context.StoreProducts.Remove(storeProduct);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product removed from store successfully!";
            }

            return RedirectToAction(nameof(ManageStoreProducts), new { storeId });
        }

        #endregion
    }
}
