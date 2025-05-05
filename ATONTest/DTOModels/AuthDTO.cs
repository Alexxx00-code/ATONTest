using System.ComponentModel.DataAnnotations;

namespace ATONTest.DTOModels
{
    public class AuthDTO
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
