using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;
using System.Diagnostics;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
                            ApplicationDbContext context,
                            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                    {
                        var myClaims = await _context.Claims
                            .Include(c => c.CurrentStatus)
                            .Where(c => c.UserId == user.Id)
                            .OrderByDescending(c => c.SubmissionDate)
                            .ToListAsync();

                        ViewBag.User = user;

                        // Pass TempData to view
                        if (TempData["Success"] != null)
                        {
                            ViewBag.Success = TempData["Success"];
                        }
                        if (TempData["Error"] != null)
                        {
                            ViewBag.Error = TempData["Error"];
                        }

                        return View("LecturerDashboard", myClaims);
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                    {
                        return RedirectToAction("CoordinatorDashboard", "Claims");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Manager"))
                    {
                        return RedirectToAction("ManagerDashboard", "Claims");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "HR"))
                    {
                        return RedirectToAction("Dashboard", "HR");
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TestUsers()
        {
            return View();
        }

        public async Task<IActionResult> DiagnoseHR()
        {
            return View();
        }

        public IActionResult CreateHRUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHRUserPost()
        {
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                // Ensure HR role exists
                if (!await roleManager.RoleExistsAsync("HR"))
                {
                    await roleManager.CreateAsync(new IdentityRole("HR"));
                }

                // Create HR user
                var hrUser = new ApplicationUser
                {
                    UserName = "hr@cmcs.com",
                    Email = "hr@cmcs.com",
                    FirstName = "HR",
                    LastName = "Manager",
                    HourlyRate = 0,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(hrUser, "HR123!");

                if (result.Succeeded)
                {
                    // Assign HR role
                    await userManager.AddToRoleAsync(hrUser, "HR");
                    TempData["Success"] = "HR user created successfully! You can now login with hr@cmcs.com and password HR123!";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["Error"] = $"Failed to create HR user: {errors}";

                    // Try with simpler password
                    hrUser = new ApplicationUser
                    {
                        UserName = "hr@cmcs.com",
                        Email = "hr@cmcs.com",
                        FirstName = "HR",
                        LastName = "Manager",
                        HourlyRate = 0,
                        EmailConfirmed = true
                    };

                    var simpleResult = await userManager.CreateAsync(hrUser, "Password123!");
                    if (simpleResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(hrUser, "HR");
                        TempData["Success"] = "HR user created with simpler password! Use Password123! to login.";
                    }
                    else
                    {
                        TempData["Error"] = $"Failed with both passwords: {string.Join(", ", simpleResult.Errors.Select(e => e.Description))}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating HR user: {ex.Message}";
            }

            return RedirectToAction("DiagnoseHR");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixHRRole()
        {
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

            var hrUser = await userManager.FindByEmailAsync("hr@cmcs.com");
            if (hrUser != null)
            {
                // Check if user already has HR role
                var isInRole = await userManager.IsInRoleAsync(hrUser, "HR");
                if (!isInRole)
                {
                    var result = await userManager.AddToRoleAsync(hrUser, "HR");
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "HR role assigned successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Failed to assign HR role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Info"] = "HR role was already assigned.";
                }
            }
            else
            {
                TempData["Error"] = "HR user not found.";
            }

            return RedirectToAction("DiagnoseHR");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
