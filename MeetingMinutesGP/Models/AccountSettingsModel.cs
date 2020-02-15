using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class AccountSettingsModel
    {
        [Display(Name = "Full Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Full name is required")]
        public string Name { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Birthdate")]
        [DataType(DataType.Date)]
        //[DisplayFormat(ApplyFormatInEditMode =true, DataFormatString = "{0:MM/dd/yyyy}")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> Birthdate { get; set; }


        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"(01)[0-9]{9}", ErrorMessage = "Not a valid phone number")]
        public string Phone { get; set; }

        [Display(Name = "Company")]
        public string Company { get; set; }

        [Display(Name = "Position")]
        public string Position { get; set; }

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Minimum eight characters, at least one uppercase letter, one lowercase letter and one number.")]

        [MinLength(8, ErrorMessage = "Mininmum 8 characters is required")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}