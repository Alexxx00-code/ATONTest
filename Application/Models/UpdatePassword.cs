namespace Application.Models
{
    public class UpdatePassword
    {
        public Guid Guid { get; set; }

        public string Password { get; set; }

        public string ModifiedBy { get; set; }
    }
}
