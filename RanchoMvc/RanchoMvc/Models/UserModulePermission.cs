using System.ComponentModel.DataAnnotations;

namespace RanchoMvc.Models
{
    public class UserModulePermission
    {
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string UserId { get; set; }

        [Required, MaxLength(50)]
        public string Module { get; set; }

        public bool CanView { get; set; }
        public bool CanEdit { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
