using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> Get();

        User Create(User user);

        User Update(User user);

        User Delete(Guid guid);
    }
}
