using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class ResetPasswordModel
    {
        [Display(Name = "New Password*")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Minimum eight characters, at least one uppercase letter, one lowercase letter and one number.")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password*")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm password and new password do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ResetCode { get; set; }
    }
}