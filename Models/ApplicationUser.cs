using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spode.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [StringLength(100)]
        public string? Name { get; set; }

        [PersonalData]
        public string? ProfilePicture { get; set; }

        [NotMapped]
        public IList<string>? Roles { get; set; }

        // Navigation properties
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<CartItem>? CartItems { get; set; }
    }
} 