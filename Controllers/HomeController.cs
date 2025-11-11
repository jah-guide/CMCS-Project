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
                }
            }

            return View();
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