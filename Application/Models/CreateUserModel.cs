namespace Application.Models
{
    public class CreateUserModel
    {
        public string Login {  get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public bool Admin { get; set; }

        public string CreatedBy { get; set; }
    }
}
