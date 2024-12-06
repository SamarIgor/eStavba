using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using eStavba.Models;

public class RoleAssignment
{
    [Key]
    public int Id { get; set; } // Primary Key
    [ForeignKey(nameof(Role))] // Links to the IdentityRole table
    public string RoleId { get; set; } // Foreign Key from AspNetRoles
    [ForeignKey(nameof(User))] // Links to the IdentityUser table
    public string UserId { get; set; } // Foreign Key from AspNetUsers
    public DateTime AssignedDate { get; set; } // Date the role was assigned

    public IdentityRole Role { get; set; } // Navigation Property
    public virtual User? User { get; set;} // Navigation Property
}



