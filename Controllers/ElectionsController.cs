using Microsoft.AspNetCore.Mvc;
using eStavba.Models;
using System;
using System.Linq;
using eStavba.Data;
using Microsoft.AspNetCore.Authorization;

namespace eStavba.Controllers
{
    [Authorize]
    public class ElectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ElectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var election = _context.Elections
                .OrderByDescending(e => e.StartDate)
                .FirstOrDefault();

            ViewBag.CanStartNewElection = election == null || election.EndDate.AddMonths(3) <= DateTime.Now;
            return View(election);
        }
/*
        [HttpPost]
        public IActionResult StartNewElection()
        {
            var election = new ElectionModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(3),
                State = ElectionState.Ongoing
            };

            _context.Elections.Add(election);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Vote(int electionId, int candidateId)
        {
            var election = _context.Elections.Find(electionId);
            if (election == null || election.State != ElectionState.Ongoing)
                return BadRequest("Election not available.");

            var candidate = election.Candidates.FirstOrDefault(c => c.Id == candidateId);
            if (candidate == null)
                return NotFound();

            candidate.Votes++;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProposeCandidate(int electionId, string userId, string name)
        {
            var election = _context.Elections.Find(electionId);
            if (election == null || election.State != ElectionState.Ongoing)
                return BadRequest("Election not available.");

            var candidate = new Candidate
            {
                UserId = userId,
                Name = name
            };

            election.Candidates.Add(candidate);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult FinalizeElection(int electionId)
        {
            var election = _context.Elections.Find(electionId);
            if (election == null || election.State != ElectionState.Ongoing)
                return BadRequest("Election not available.");

            election.State = ElectionState.Completed;

            var winner = election.Candidates
                .OrderByDescending(c => c.Votes)
                .FirstOrDefault();

            if (election.Candidates.Count > 1 &&
                election.Candidates[0].Votes == election.Candidates[1].Votes)
            {
                election.State = ElectionState.NotStarted;
            }
            else
            {
                election.CurrentHouseManager = winner?.Name;
                // Update roles here
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
         */
    }
}
