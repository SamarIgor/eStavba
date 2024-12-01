using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace eStavba.Models
{
    public class ElectionModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? CurrentHouseManager { get; set; }

        public List<Candidate> Candidates { get; set; } = new();

        public ElectionState State { get; set; } = ElectionState.NotStarted;
        [ForeignKey("InitiatorUserId")]
        public string InitiatorUserId { get; set; }  

    }

    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Name { get; set; }
        public List<Vote> Votes { get; set; } = new();

        [ForeignKey("Election")]
        public int ElectionModelId { get; set; }

    }

    public enum ElectionState
    {
        NotStarted,
        YesNo,
        Ongoing,
        Completed
    }

    public class Vote
    {
        [Key]
        public int Id { get; set; }

        public int ElectionId { get; set; }
        public ElectionModel Election { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public VoteType? VoteType { get; set; }

        public int? CandidateId { get; set; } 
    }


    public enum VoteType
    {   
        Yes,
        No
    }
}
