using System.ComponentModel.DataAnnotations;

namespace FoodApplication.Web.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        public bool IsLocked { get; set; }
        public List<RoleSelectionViewModel> Roles { get; set; }
    }
}
