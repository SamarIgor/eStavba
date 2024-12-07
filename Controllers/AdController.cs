using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eStavba.Data;
using eStavba.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace eStavba.Controllers
{
    public class AdController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdController> _logger;

        public AdController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<AdController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // Index: Display all Ads
        public async Task<IActionResult> Index()
        {
            var ads = await _context.Ads.ToListAsync();  // Fetch all ads from the database
            return View(ads.OrderBy(a => a.Priority).ToList());
        }

        // Add Ad
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AdModel adModel)
        {
            _logger.LogInformation("Starting Add method");

            // Log the contents of the model
            _logger.LogInformation("Model Data: {StartDate}, {EndDate}, {WebsiteLink}, {PhoneNumber}, {Picture}, {Priority}, {Description}",
                adModel.StartDate, adModel.EndDate, adModel.WebsiteLink, adModel.PhoneNumber, adModel.Picture, adModel.Priority, adModel.Description);

            // Date validation: End date must be after Start date
            if (adModel.EndDate <= adModel.StartDate)
            {
                _logger.LogWarning("End date validation failed");
                ModelState.AddModelError("EndDate", "End date must be later than the start date.");
            }

            // Phone number validation: Should be 9 to 15 digits, starting with '+'
            if (!string.IsNullOrEmpty(adModel.PhoneNumber) &&
                (adModel.PhoneNumber.Length < 9 || adModel.PhoneNumber.Length > 15 ||
                 !adModel.PhoneNumber.StartsWith("+") ||
                 !adModel.PhoneNumber.Substring(1).All(char.IsDigit)))
            {
                _logger.LogWarning("Phone number validation failed");
                ModelState.AddModelError("PhoneNumber", "Phone number must be 9 to 15 digits and start with '+'.");
            }

            // Picture validation
            if (string.IsNullOrEmpty(adModel.Picture) || !Uri.IsWellFormedUriString(adModel.Picture, UriKind.Absolute))
            {
                _logger.LogWarning("Picture validation failed: Invalid URL");
                ModelState.AddModelError("Picture", "Provide a valid URL for the picture.");
            }

            // Priority validation: Must be a digit
            if (!int.TryParse(adModel.Priority.ToString(), out int priority) || priority <= 0)
            {
                _logger.LogWarning("Priority validation failed");
                ModelState.AddModelError("Priority", "Priority must be a positive digit.");
            }

            // Description validation: Must not exceed 150 characters
            if (!string.IsNullOrEmpty(adModel.Description) && adModel.Description.Length > 150)
            {
                _logger.LogWarning("Description validation failed");
                ModelState.AddModelError("Description", "Description must not exceed 150 characters.");
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid");
                _logger.LogInformation("Ad added successfully");
                _context.Ads.Add(adModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Log all validation errors
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                if (errors.Any())
                {
                    _logger.LogWarning("Validation error for {Key}: {Errors}", key, string.Join(", ", errors.Select(e => e.ErrorMessage)));
                }
            }

            _logger.LogWarning("ModelState is not valid, returning view with errors");
            return View(adModel);
        }

        // Edit Ad
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdModel adModel)
        {
            if (id != adModel.Id)
            {
                return NotFound();
            }

            // Date validation
            if (adModel.EndDate <= adModel.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be later than the start date.");
            }

            // Phone number validation
            if (!string.IsNullOrEmpty(adModel.PhoneNumber) &&
                (adModel.PhoneNumber.Length < 9 || adModel.PhoneNumber.Length > 15 ||
                 !adModel.PhoneNumber.StartsWith("+") ||
                 !adModel.PhoneNumber.Substring(1).All(char.IsDigit)))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must be 9 to 15 digits and start with '+'.");
            }

            // Picture validation
            if (string.IsNullOrEmpty(adModel.Picture) || !Uri.IsWellFormedUriString(adModel.Picture, UriKind.Absolute))
            {
                ModelState.AddModelError("Picture", "Provide a valid URL for the picture.");
            }

            // Priority validation: Must be a digit
            if (!int.TryParse(adModel.Priority.ToString(), out int priority) || priority <= 0)
            {
                ModelState.AddModelError("Priority", "Priority must be a positive digit.");
            }

            // Description validation: Must not exceed 150 characters
            if (!string.IsNullOrEmpty(adModel.Description) && adModel.Description.Length > 150)
            {
                ModelState.AddModelError("Description", "Description must not exceed 150 characters.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdExists(adModel.Id))
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

            return View(adModel);
        }

       // GET: Ad/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Ads == null)
            {
                return NotFound();
            }

            var ad = await _context.Ads
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }

        // POST: Ad/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool AdExists(int id)
        {
            return _context.Ads.Any(e => e.Id == id);
        }
    }
}
