using System;
using System.Threading.Tasks;
using eStavba.Data;
using eStavba.Models;
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

            // Check if the role is already assigned to the user
            var existingAssignment = await _context.RoleAssignments
                .FirstOrDefaultAsync(ra => ra.UserId == userId && ra.RoleId == roleId);

            if (existingAssignment != null)
            {
                _logger.LogWarning("Role {RoleId} is already assigned to user {UserId}.", roleId, userId);
                throw new InvalidOperationException("This role is already assigned to the user.");
            }

            // Create a new role assignment
            var roleAssignment = new RoleAssignment
            {
                UserId = userId,
                RoleId = roleId,
                AssignedDate = DateTime.UtcNow
            };

            try
            {
                // Add the role assignment to the database
                _context.RoleAssignments.Add(roleAssignment);
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
