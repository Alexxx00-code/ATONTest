namespace Application.Models
{
    public class UserModel
    {
        public Guid Guid { get; set; }

        public string Login { get; set; }

        public string Name { get; set; }

        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public bool Admin { get; set; }

        public bool Revoked { get; set; }
    }
}
