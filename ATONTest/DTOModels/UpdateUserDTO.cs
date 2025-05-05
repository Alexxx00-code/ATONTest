using System.ComponentModel.DataAnnotations;

namespace ATONTest.DTOModels
{
    public class UpdateUserDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }
    }
}
