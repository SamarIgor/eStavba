using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eStavba.Data;
using Microsoft.EntityFrameworkCore;
using eStavba.Models;

namespace eStavba.Controllers
{
    public class ElectionsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public ElectionsController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Display current admin and election options
        public async Task<IActionResult> Index()
        {   

            var adminUser = await _userManager.GetUsersInRoleAsync("Admin");
            var currentAdmin = adminUser.FirstOrDefault();
            var lastElection = await _context.Elections
                .Include(e => e.Candidates)  // Include Candidates to fetch them with the election
                .OrderByDescending(e => e.EndDate)
                .FirstOrDefaultAsync();

            var currentUserId = _userManager.GetUserId(User);

            var hasVoted = await _context.Votes
                        .Where(v => v.ElectionId == lastElection.Id && v.UserId == currentUserId)
                        .CountAsync();

            ViewBag.election = lastElection;

            if (lastElection.State == ElectionState.YesNo) {
                if (hasVoted > 0) {
                    var countYesVotes = await _context.Votes
                        .Where(v => v.ElectionId == lastElection.Id && v.VoteType == VoteType.Yes)
                        .CountAsync();
                    var countNoVotes = await _context.Votes
                        .Where(v => v.ElectionId == lastElection.Id && v.VoteType == VoteType.No)
                        .CountAsync();

                    ViewBag.countNoVotes = countNoVotes;
                    ViewBag.CountYesVotes = countYesVotes;
                }
                else return View("YesNo");
            }

            if (currentAdmin != null)
            {
                var adminRoleId = _context.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
                var adminUserId = _context.UserRoles.FirstOrDefault(u => u.RoleId == adminRoleId).UserId;
                var election = _context.Elections.FirstOrDefault(x => x.CurrentHouseManager == currentAdmin.Email);
                if (ViewBag.election == null) ViewBag.election = election;
                

                // Correctly reference the current user's ID in the query
                var adminAssignment = _context.RoleAssignments
                    .FirstOrDefault(ra => ra.RoleId == adminRoleId && ra.UserId == adminUserId);

                if (adminAssignment != null)
                {
                    var daysAsAdmin = (DateTime.UtcNow - adminAssignment.AssignedDate).Days;
                    ViewBag.DaysInPosition = daysAsAdmin; // Store as integer
                }
                else
                {
                    ViewBag.DaysInPosition = 0; // Default to 0 if no admin assignment found
                }

                ViewBag.CurrentAdmin = currentAdmin.UserName;
                
            }
            else
            {
                ViewBag.CurrentAdmin = "No Admin Assigned";
                ViewBag.DaysInPosition = 0;
            }

            return View();
        }

        // POST: Propose a new election if admin has served for 3+ months
        [HttpPost]
        public async Task<IActionResult> ProposeElection()
        {
            var lastElection = await _context.Elections
                .Include(e => e.Candidates)  // Include Candidates to fetch them with the election
                .OrderByDescending(e => e.EndDate)
                .FirstOrDefaultAsync();

            var currentUser = await _userManager.GetUserAsync(User);

            var lastDate = DateTime.UtcNow - lastElection.EndDate;

            if (lastElection.State == ElectionState.Completed && lastDate.Days >= 90) {
                var adminUser = await _userManager.GetUsersInRoleAsync("Admin");
                var currentAdmin = adminUser.FirstOrDefault();  // Get the first admin or null if none exists

                string currentHouseManager = currentAdmin?.UserName ?? "No current admin";  // If no admin exists, set to "ADMIN NO"

                // If no ongoing election or the current user is not the initiator, start a new election
                var election = new ElectionModel
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(3),  // Election duration, adjust as necessary
                    State = ElectionState.YesNo,
                    InitiatorUserId = currentUser.Id,  // Set the InitiatorUserId here
                    CurrentHouseManager = currentHouseManager  // Set CurrentHouseManager to the current admin's UserName (or "ADMIN NO" if no admin)
                };

                ViewBag.election = election;

                _context.Elections.Add(election);
                await _context.SaveChangesAsync();

            } 

            return View("YesNo");  
        }

        [HttpPost]
        public async Task<IActionResult> VoteForElection(string vote) {
            var lastElection = await _context.Elections
                .Include(e => e.Candidates)  // Include Candidates to fetch them with the election
                .OrderByDescending(e => e.EndDate)
                .FirstOrDefaultAsync();
            
            ViewBag.election = lastElection;

            var currentUser = await _userManager.GetUserAsync(User);

            var lastDate = DateTime.UtcNow - lastElection.EndDate;
            Vote voteYes = new Vote 
            {
                ElectionId = lastElection.Id,
                Election = lastElection,
                UserId = currentUser.Id,
                User = currentUser,
                VoteType = VoteType.Yes
            };

            Vote voteNo = new Vote 
            {
                ElectionId = lastElection.Id,
                Election = lastElection,
                UserId = currentUser.Id,
                User = currentUser,
                VoteType = VoteType.No
            };

            if (lastElection.State == ElectionState.YesNo) {
                if (vote == "Yes") {
                   _context.Votes.Add(voteYes);
                } else {
                    _context.Votes.Add(voteNo);
                }
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("Index");
        }

        // stari propose
                    // var currentUser = await _userManager.GetUserAsync(User);

            // // Fetch the latest election in the NotStarted state (if any)
            // var yesNoElection = await _context.Elections
            //     .Include(e => e.Candidates)  // Include Candidates to fetch them with the election
            //     .OrderByDescending(e => e.EndDate)
            //     .FirstOrDefaultAsync(e => e.State == ElectionState.YesNo);

            // // Check if there is an ongoing election
            // if (yesNoElection != null)
            // {
            //     // Check if the current user is the initiator of the election
            //     if (yesNoElection.InitiatorUserId == currentUser.Id)
            //     {
            //         // Count the number of candidates
            //         var candidatesCount = yesNoElection.Candidates.Count;  // Now using Candidates from the loaded election

            //         // Calculate the sufficient votes required based on the number of candidates
            //         var sufficientVotesRequired = (int)Math.Floor((double)candidatesCount / 2) + 1;

            //         // If the initial voting did not pass, enforce a 15-day cooldown for the initiator
            //         var initialVotingResult = await _context.Votes
            //             .Where(v => v.ElectionId == yesNoElection.Id && v.VoteType == VoteType.Yes)
            //             .CountAsync();

            //         if (initialVotingResult < sufficientVotesRequired)  // Ensure this is the correct threshold
            //         {
            //             var lastVoteDate = yesNoElection.StartDate; // Set this appropriately based on your logic
            //             var cooldownPeriod = TimeSpan.FromDays(15);

            //             // Check if the cooldown period has passed
            //             if (DateTime.UtcNow < lastVoteDate.Add(cooldownPeriod))
            //             {
            //                 TempData["Message"] = "You cannot initiate a new election yet. Please wait 15 more days.";
            //                 return RedirectToAction("Index");  // Or whatever action is appropriate
            //             }
            //         }
            //     }
            // }

            // // Fetch the current admin (if one exists)
            // var adminUser = await _userManager.GetUsersInRoleAsync("Admin");
            // var currentAdmin = adminUser.FirstOrDefault();  // Get the first admin or null if none exists

            // string currentHouseManager = currentAdmin?.UserName ?? "ADMIN NO";  // If no admin exists, set to "ADMIN NO"

            // // If no ongoing election or the current user is not the initiator, start a new election
            // var election = new ElectionModel
            // {
            //     StartDate = DateTime.UtcNow,
            //     EndDate = DateTime.UtcNow.AddDays(3),  // Election duration, adjust as necessary
            //     State = ElectionState.YesNo,
            //     InitiatorUserId = currentUser.Id,  // Set the InitiatorUserId here
            //     CurrentHouseManager = currentHouseManager  // Set CurrentHouseManager to the current admin's UserName (or "ADMIN NO" if no admin)
            // };

            // ViewBag.election = election;

        // GET: Voting page
        public IActionResult Vote()
        {
            // Fetch users eligible for voting
            var users = _userManager.Users.ToList();
            var currentUserId = _userManager.GetUserId(User);

            var candidates = users.Where(u => u.Id != currentUserId).Select(u => new
            {
                u.Id,
                u.UserName
            }).ToList();

            ViewBag.Candidates = candidates;
            return View();
        }

        // POST: Submit vote
        [HttpPost]
        public async Task<IActionResult> SubmitVote(string candidateId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == candidateId)
            {
                TempData["Message"] = "Ne mozhete da glasate za samite sebe.";
                return RedirectToAction("Vote");
            }

            // Save vote to the database (not shown here, depends on implementation)

            TempData["Message"] = "Glasot e uspesno dostaven!";
            return RedirectToAction("Index");
        }

        // GET: Election results
        public IActionResult Results()
        {
            // Fetch votes from the database and determine the winner
            // If a tie occurs, prepare for a re-vote
            ViewBag.Result = "Neresen rezultat. Treba preglasuvanje.";
            return View();
        }
    }
}
