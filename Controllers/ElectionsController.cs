using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eStavba.Data;
using Microsoft.EntityFrameworkCore;
using eStavba.Models;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using eStavba.Services;

namespace eStavba.Controllers
{
    public class ElectionsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        private readonly RoleService _roleService;
        public ElectionsController(UserManager<User> userManager, 
        RoleManager<IdentityRole> roleManager, 
        ApplicationDbContext context, 
        RoleService roleService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _roleService = roleService;
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

            var hasVotedForVoting = await _context.Votes
                        .Where(v => v.ElectionId == lastElection.Id && v.UserId == currentUserId
                        && v.VoteType != null)
                        .CountAsync();

            var hasVoted = await _context.Votes
                        .Where(v => v.ElectionId == lastElection.Id && v.UserId == currentUserId
                        && v.CandidateId != null)
                        .CountAsync();

            ViewBag.election = lastElection;
            ViewBag.endDate = lastElection.EndDate;

            TimeSpan timeRemaining = lastElection.EndDate - DateTime.UtcNow;

            if (lastElection.State == ElectionState.YesNo) {
                var countYesVotes = await _context.Votes
                    .Where(v => v.ElectionId == lastElection.Id && v.VoteType == VoteType.Yes)
                    .CountAsync();

                var countNoVotes = await _context.Votes
                    .Where(v => v.ElectionId == lastElection.Id && v.VoteType == VoteType.No)
                    .CountAsync();

                ViewBag.countNoVotes = countNoVotes;
                ViewBag.CountYesVotes = countYesVotes;

                if (timeRemaining.TotalSeconds > 0 && hasVotedForVoting == 0) return View("YesNo");

                else if (timeRemaining.TotalSeconds <= 0 && countYesVotes > countNoVotes) {
                
                    lastElection.State = ElectionState.Ongoing;
                    lastElection.EndDate = DateTime.UtcNow.AddDays(3);
                    _context.Update(lastElection);
                    await _context.SaveChangesAsync();

                    timeRemaining = lastElection.EndDate - DateTime.UtcNow;

                    ViewBag.candidates = lastElection.Candidates;

                    return View("Vote");
                } 

                return View("Index");

            } else if (lastElection.State == ElectionState.Ongoing 
                    || lastElection.State == ElectionState.ReVoting) 
            {
                ViewBag.candidates = lastElection.Candidates;
                ViewBag.election = lastElection;

                if (hasVoted == 0) return View("Vote");
                
                var candidatesWithVotes = lastElection.Candidates
                    .Select(candidate => new
                    {
                        candidate.Name,
                        VoteCount = _context.Votes.Count(v => v.CandidateId == candidate.Id && v.ElectionId == lastElection.Id)
                    })
                    .OrderByDescending(c => c.VoteCount)
                    .ToList();

                ViewBag.CandidatesWithVotes = candidatesWithVotes;

                if (timeRemaining.TotalSeconds <= 0) {

                    var highestVoteCount = candidatesWithVotes.First().VoteCount;
                    var tiedCandidates = candidatesWithVotes
                        .Where(c => c.VoteCount == highestVoteCount)
                        .ToList();

                    if (tiedCandidates.Count > 1) {
                        
                        lastElection.EndDate = DateTime.UtcNow.AddDays(2);
                        lastElection.Candidates = lastElection.Candidates
                            .Where(c => tiedCandidates.Any(tc => tc.Name == c.Name))
                            .ToList();

                        _context.Update(lastElection);
                        await _context.SaveChangesAsync();

                        foreach (var candidate in lastElection.Candidates)
                        {
                            var candidateVotes = _context.Votes.Where(v => v.CandidateId == candidate.Id);
                            _context.Votes.RemoveRange(candidateVotes); // Reset the votes
                        }
                        await _context.SaveChangesAsync();

                        lastElection.State = ElectionState.ReVoting;

                        _context.Update(lastElection);
                        await _context.SaveChangesAsync();
                        
                        ViewBag.candidates = lastElection.Candidates;
                        ViewBag.election = lastElection;
                        return View("Vote");

                    } else {

                        lastElection.State = ElectionState.Completed;
                        _context.Update(lastElection);
                        await _context.SaveChangesAsync();

                    }
                }
            } 

            if (currentAdmin != null)
            {
                var adminRoleId = _context.Roles.FirstOrDefault(r => r.NormalizedName == "ADMIN").Id;
                var userRoleId = _context.Roles.FirstOrDefault(r => r.NormalizedName == "MEMBER").Id;
                var adminUserId = _context.UserRoles.FirstOrDefault(u => u.RoleId == adminRoleId).UserId;
                var election = _context.Elections.FirstOrDefault(x => x.CurrentHouseManager == currentAdmin.Email);

                if (ViewBag.election == null) ViewBag.election = election;

                if (lastElection.State == ElectionState.Completed) {
                    // Find the candidate with the most votes
                    var winner = lastElection.Candidates
                        .OrderByDescending(c => c.Votes.Count)
                        .FirstOrDefault();

                    var winnerUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == winner.UserId);
                    var isWinnerAdmin = await _userManager.IsInRoleAsync(winnerUser, "Admin");

                    if (winner != null) 
                    {

                        var isAdminMember = await _userManager.IsInRoleAsync(currentAdmin, "Admin");
                        await _roleService.AssignRole(currentAdmin.Id, userRoleId);


                        // Assign the "Admin" role to the winner
                        await _roleService.AssignRole(winner.UserId, adminRoleId);
                        adminUser = await _userManager.GetUsersInRoleAsync("Admin");
                        currentAdmin = adminUser.FirstOrDefault();

                        TempData["Message"] = $"The new admin is {winner.Name}. The election is now complete.";
                    }
                    
                }
                

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

        [HttpPost]
        public async Task<IActionResult> AreYouSureToPropose() {
            if (Request.Form.ContainsKey("confirmValue") && Request.Form["confirmValue"] == "true") 
                return await ProposeElection();
            else return View();
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
                ViewBag.endDate = election.EndDate;
                // set electionVote to true on default. He proposed it, duuh
                ViewBag.iDo = "Yes";

                _context.Elections.Add(election);
                await _context.SaveChangesAsync();

            } 

            return View("YesNo");  
        }

        [HttpPost]
        public async Task<IActionResult> VoteForElection(string electionVote, string candidateVote) {
            var lastElection = await _context.Elections
                .Include(e => e.Candidates)  // Include Candidates to fetch them with the election
                .OrderByDescending(e => e.EndDate)
                .FirstOrDefaultAsync();

            ViewBag.election = lastElection;
            ViewBag.endDate = lastElection.EndDate;

            var currentUser = await _userManager.GetUserAsync(User);

            var lastDate = DateTime.UtcNow - lastElection.EndDate;
            Vote voteYes = new Vote 
            {
                ElectionId = lastElection.Id,
                Election = lastElection,
                UserId = currentUser.Id,
                User = (User) currentUser,
                VoteType = VoteType.Yes
            };

            Vote voteNo = new Vote 
            {
                ElectionId = lastElection.Id,
                Election = lastElection,
                UserId = currentUser.Id,
                User = (User) currentUser,
                VoteType = VoteType.No
            };

            if (lastElection.State == ElectionState.YesNo) {
                if (electionVote == "Yes") {
                   _context.Votes.Add(voteYes);
                } else {
                    _context.Votes.Add(voteNo);
                }

                if (candidateVote == "Yes") {
                     var isAlreadyCandidate = lastElection.Candidates
                        .Any(c => c.UserId == currentUser.Id);

                    if (!isAlreadyCandidate)
                    {
                        Candidate candidate = new Candidate
                        {
                            UserId = currentUser.Id,
                            Name = currentUser.UserName,
                            ElectionModelId = lastElection.Id
                        };

                        lastElection.Candidates.Add(candidate);
                        _context.Update(lastElection);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST: Submit vote
        [HttpPost]
        public async Task<IActionResult> SubmitVote(int candidateId)
        {
            var currentUser = await _userManager.GetUserAsync(User);


            var lastElection = await _context.Elections
                .Include(e => e.Candidates)  // Include candidates
                .OrderByDescending(e => e.EndDate)
                .FirstOrDefaultAsync();


            ViewBag.candidates = lastElection.Candidates;
            // Find the selected candidate from the election's candidates
            Candidate candidate = lastElection.Candidates.FirstOrDefault(c => c.Id == candidateId);

            // Check if the user is trying to vote for themselves
            if (currentUser.Id.Equals(candidate.UserId))
            {
                TempData["Message"] = "You cannot vote for yourself.";
                return RedirectToAction("Index");
            }

            // Check if the current user has already voted in this election
            var hasVoted = await _context.Votes
                .Where(v => v.ElectionId == lastElection.Id && v.UserId == currentUser.Id && v.CandidateId != null)
                .CountAsync();

            if (hasVoted > 0)
            {
                TempData["Message"] = "You have already voted.";
                return RedirectToAction("Index");
            }

            // Add the vote to the database
            Vote voteForCandidate = new Vote 
            {
                ElectionId = lastElection.Id,
                Election = lastElection,
                UserId = currentUser.Id,
                User = (User) currentUser,
                CandidateId = candidateId
            };

            candidate.Votes.Add(voteForCandidate);
            
            _context.Update(candidate);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Your vote has been successfully submitted!";
            return RedirectToAction("Index");
        }
    }
}
