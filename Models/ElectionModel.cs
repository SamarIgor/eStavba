using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public string CurrentHouseManager { get; set; }

        public List<Candidate> Candidates { get; set; } = new();

        public ElectionState State { get; set; }
    }

    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Name { get; set; }
        public int Votes { get; set; }
    }

    public enum ElectionState
    {
        NotStarted,
        Ongoing,
        Completed
    }
}
