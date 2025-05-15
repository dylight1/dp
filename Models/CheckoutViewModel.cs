using System.ComponentModel.DataAnnotations;

namespace Spode.Models
{
    public class CheckoutViewModel
    {
        // Customer Information
        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        // Shipping Information
        [Required]
        [Display(Name = "Address Line 1")]
        [StringLength(100, MinimumLength = 5)]
        public string AddressLine1 { get; set; } = string.Empty;

        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required]
        [Display(Name = "City")]
        [StringLength(50, MinimumLength = 2)]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "State/Province")]
        [StringLength(50, MinimumLength = 2)]
        public string State { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Postal/Zip Code")]
        [StringLength(20, MinimumLength = 4)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; } = "Russia";

        // Order notes
        [Display(Name = "Order Notes")]
        [StringLength(500)]
        public string? OrderNotes { get; set; }

        // Payment Method (basic implementation)
        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";
    }
} 