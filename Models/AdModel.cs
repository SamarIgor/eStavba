using System.ComponentModel.DataAnnotations;

namespace eStavba.Models
{
    public class AdModel
    {
        public int Id { get; set; }  // Primary key

        [Required]
        public DateTime StartDate { get; set; }  // Start date for the ad

        [Required]
        public DateTime EndDate { get; set; }  // End date for the ad

        [StringLength(255)]
        public string? WebsiteLink { get; set; }  // Website link for the ad

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }  // Phone number for the ad

        [Required]
        [StringLength(255)]  // Increase the length to handle longer URLs
        public string Picture { get; set; }  // Picture link

        [Required]
        public int Priority { get; set; }  // Priority for sorting

        [StringLength(150)]
        public string Description { get; set; }  // Description for the ad
    }
}
