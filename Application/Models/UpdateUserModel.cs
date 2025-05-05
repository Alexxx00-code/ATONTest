namespace Application.Models
{
    public class UpdateUserModel
    {
        public Guid Guid { get; set; }

        public string Login { get; set; }

        public string Name { get; set; }

        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public string ModifiedBy { get; set; }
    }
}
