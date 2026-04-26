using System.ComponentModel.DataAnnotations;

namespace RanchoMvc.Models
{
    public class Plan
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string ShortDescription { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string PriceLabel { get; set; }

        public string ImageUrl { get; set; }

        [MaxLength(50)]
        public string Icon { get; set; }

        public bool IsActive { get; set; } = true;

        public int Order { get; set; } = 0;

        public string Includes { get; set; }
    }
}
