using System;
using System.Collections.Generic;

using eStavba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Drawing2D;
using System.Security.Claims;
using eStavba.Data;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class BillsController : Controller
{

    private readonly ApplicationDbContext _context;

    public BillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public IActionResult GetBillsForCurrentUser()
    {
        string currentUserId = GetCurrentUserId();

        var userBills = _context.Bills.Where(b => b.UserId == currentUserId).ToList();

        return View(userBills);
    }

    public IActionResult GetBillsForUser()
    {
        string currentUserId = GetCurrentUserId(); 

        var userBills = _context.Bills.Where(b => b.UserId == currentUserId).ToList();

        return View(userBills);
    }

    public IActionResult MarkBillAsPaid(int billId)
    {
        var bill = _context.Bills.Find(billId);

        if (bill != null)
        {
            bill.IsPaid = true;
            _context.SaveChanges();
        }

        return RedirectToAction("GetBillsForCurrentUser");
    }

    [Authorize]
     public IActionResult Create()
        {
            return View();
        }


    [Authorize]
    [HttpPost]
      public IActionResult Create(Bills bill)
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bill.UserId = userId;


            _context.Bills.Add(bill);
            _context.SaveChanges();

            return RedirectToAction("MyBills");
    }

    [Authorize]
        public IActionResult Delete(int id)
        {
            var bill = _context.Bills.Find(id);

            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult DeleteConfirmed(int id)
        {
            var bill = _context.Bills  
                .FirstOrDefault(t => t.Id == id);

            if (bill == null)
            {
                return NotFound();
            }

            _context.Bills.Remove(bill);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    public IActionResult ReportNewBill(string userId, string billType, decimal amount, DateTime dueDate)
    {
        var newBill = new Bills
        {
            UserId = userId,
            BillType = billType,
            Amount = amount,
            DueDate = dueDate,
            IsPaid = false
        };

        _context.Bills.Add(newBill);
        _context.SaveChanges();

        return RedirectToAction("GetBillsForUser", new { userId });
    }

    public IActionResult MyBills()
    {
        string currentUserId = GetCurrentUserId();

        var userBills = _context.Bills.Where(b => b.UserId == currentUserId).ToList();

        var viewModel = new BillsViewModel
        {
            Bills = userBills,
        };

        return View(viewModel);
    }

}
