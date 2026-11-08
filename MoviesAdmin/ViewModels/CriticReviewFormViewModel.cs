
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MoviesAdmin.ViewModels
{
    public class CriticReviewFormViewModel
    {
        [Range(1, int.MaxValue)]
        public int MovieId { get; set; }

        [BindNever]
        [ValidateNever]
        public string MovieTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(0, 5, ErrorMessage = "Rating must be from 0 to 5.")]
        public int? Rating { get; set; }

        [Required(ErrorMessage = "Please enter a review.")]
        [StringLength(1000,
            ErrorMessage = "Review cannot exceed 1000 characters.")]
        [Display(Name = "Review")]
        public string Description { get; set; } = string.Empty;
    }
}
