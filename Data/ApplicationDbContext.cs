﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using eStavba.Models;

namespace eStavba.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base(options)
        {
        }

        public DbSet<ForumReplyModel> ForumReplies { get; set; }
        public DbSet<ForumThreadModel> ForumThreads { get; set; }
        public DbSet<AnnouncementModel> Announcement { get; set; }
        public DbSet<Bills> Bills { get; set; }
        public DbSet<ReportProblemModel> ReportedProblems { get; set; }
        public DbSet<ElectionModel> Elections { get; set; }
        public DbSet<RoleAssignment> RoleAssignments { get; set; }
        public DbSet<Vote> Votes { get; set; } 
        public DbSet<User> Users { get; set; }
        public DbSet<AdModel> Ads { get; set; }
    }
}