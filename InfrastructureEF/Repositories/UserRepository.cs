using Domain.Interfaces;
using Domain.Models;
using InfrastructureEF.DataBase;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureEF.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataBaseContext context;

        public UserRepository(DataBaseContext dataBase)
        {
            context = dataBase;
        }

        public User Create(User user)
        {
            context.Add(user);
            context.SaveChanges();
            return user;
        }

        public User Delete(Guid guid)
        {
            var user = context.Users.Find(guid);
            context.Users.Remove(user);
            context.SaveChanges();
            return user;
        }

        public IQueryable<User> Get()
        {
            return context.Users.AsNoTracking();
        }

        public User Update(User user)
        {
            var odlUser = context.Users.Find(user.Guid);

            odlUser.Name = user.Name;
            odlUser.Gender = user.Gender;
            odlUser.Birthday = user.Birthday;
            odlUser.Login = user.Login;
            odlUser.Password = user.Password;
            odlUser.CreatedBy = user.CreatedBy;
            odlUser.CreatedOn = user.CreatedOn;
            odlUser.ModifiedBy = user.ModifiedBy;
            odlUser.ModifiedOn = user.ModifiedOn;
            odlUser.RevokedBy = user.RevokedBy;
            odlUser.RevokedOn = user.RevokedOn;

            context.SaveChanges();
            return odlUser;
        }
    }
}
