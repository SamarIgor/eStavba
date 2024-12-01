using System;
using System.Threading.Tasks;
using eStavba.Data;
using eStavba.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eStavba.Services
{
    public class RoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(ApplicationDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Assigns a role to a user with the specified user ID and role ID.
        /// </summary>
        /// <param name="userId">The ID of the user to assign the role to.</param>
        /// <param name="roleId">The ID of the role to assign.</param>
        /// <exception cref="ArgumentException">Thrown when the user ID or role ID is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the role is already assigned to the user.</exception>
        /// <exception cref="ApplicationException">Thrown when a database error occurs.</exception>
        public async Task AssignRole(string userId, string roleId)
        {
            // Check if the user and role exist in the database
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

            if (!userExists || !roleExists)
            {
                _logger.LogWarning("Invalid User ID or Role ID. UserId: {UserId}, RoleId: {RoleId}", userId, roleId);
                throw new ArgumentException("Invalid User ID or Role ID.");
            }

            // Remove existing role assignments from RoleAssignments and AspNetUserRoles
            var existingAssignments = await _context.RoleAssignments
                .Where(ra => ra.UserId == userId)
                .ToListAsync();

            if (existingAssignments.Any())
            {
                _context.RoleAssignments.RemoveRange(existingAssignments);
                _logger.LogInformation("Removed existing roles from RoleAssignments for user {UserId}.", userId);
            }

            var existingAspNetUserRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (existingAspNetUserRoles.Any())
            {
                _context.UserRoles.RemoveRange(existingAspNetUserRoles);
                _logger.LogInformation("Removed existing roles from AspNetUserRoles for user {UserId}.", userId);
            }

            // Add the new role assignment to RoleAssignments
            var roleAssignment = new RoleAssignment
            {
                UserId = userId,
                RoleId = roleId,
                AssignedDate = DateTime.UtcNow
            };
            _context.RoleAssignments.Add(roleAssignment);

            // Add the new role assignment to AspNetUserRoles
            var userRole = new IdentityUserRole<string>
            {
                UserId = userId,
                RoleId = roleId
            };
            _context.UserRoles.Add(userRole);

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();

                _logger.LogInformation("Role {RoleId} assigned to user {UserId} on {AssignedDate}.", roleId, userId, roleAssignment.AssignedDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while assigning role {RoleId} to user {UserId}.", roleId, userId);
                throw new ApplicationException("An error occurred while assigning the role.", ex);
            }
        }


    }
}
