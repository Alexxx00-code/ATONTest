using Application.Interfaces;
using Application.Models;
using Domain.Interfaces;
using Domain.Models;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository repository)
        {
            userRepository = repository;
        }

        public UserModel? CheakPassword(string login, string password)
        {
            var user = userRepository.Get().FirstOrDefault(i => i.Login == login && i.RevokedOn == null);
            if (user == null) 
            {
                return null;
            }

            if (VerifyPassword(user.Password, password))
            {
                return FillUserModel(user);
            }
            else
            {
                return null;
            }

        }

        public UserModel Create(CreateUserModel model)
        {
            var (success, msg) = ValidatePassword(model.Password);

            if (!success)
            {
                throw new InvalidOperationException(msg);
            }

            if (userRepository.Get().FirstOrDefault(i => model.Login == i.Login) != null)
            {
                throw new InvalidOperationException("Login is already used");
            }

            var newUser = new User
            {
                Guid = Guid.NewGuid(),
                Gender = model.Gender,
                Login = model.Login,
                Name = model.Name,
                Birthday = model.Birthday,
                Admin = model.Admin,
                CreatedBy = model.CreatedBy,
                CreatedOn = DateTime.UtcNow,
                Password = GetHash(model.Password),
            };

            (success, msg) = Validate(newUser);

            if (!success)
            {
                throw new InvalidOperationException(msg);
            }

            var userRes = userRepository.Create(newUser);

            return FillUserModel(userRes);
        }

        public UserModel? GetByLogin(string login)
        {
            var user = userRepository.Get().FirstOrDefault(i => login == i.Login);
            if (user == null)
            {
                return null;
            }

            return FillUserModel(user);
        }

        public List<UserModel> GetList(uint minAge = 0, bool revoked = false)
        {
            var list = userRepository.Get();
            if (minAge > 0)
            {
                list = list.Where(i => DateTime.UtcNow.AddYears(-1 * (int)minAge) > i.Birthday);
            }

            if (!revoked)
            {
                list = list.Where(i => i.RevokedOn == null);
            }

            return FillUserModel(list).ToList();
        }

        public UserModel RemoveUser(string login, bool hard, string adminName)
        {
            var user = userRepository.Get().FirstOrDefault(i => login == i.Login && i.RevokedOn == null);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (hard)
            {
                user = userRepository.Delete(user.Guid);
            }
            else
            {
                user.RevokedBy = adminName;
                user.RevokedOn = DateTime.UtcNow;
                user = userRepository.Update(user);
            }

            return FillUserModel(user);
        }

        public UserModel RestoreUser(string login)
        {
            var user = userRepository.Get().FirstOrDefault(i => login == i.Login && i.RevokedOn != null);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.RevokedBy = null;
            user.RevokedOn = null;
            user = userRepository.Update(user);

            return FillUserModel(user);
        }

        public UserModel UpdatePassword(UpdatePassword updatePassword)
        {
            var (success, msg) = ValidatePassword(updatePassword.Password);

            if (!success)
            {
                throw new InvalidOperationException(msg);
            }

            var user = userRepository.Get().FirstOrDefault(i => updatePassword.Guid == i.Guid && i.RevokedOn == null);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.Password = GetHash(updatePassword.Password);
            user.ModifiedBy = updatePassword.ModifiedBy;
            user.ModifiedOn = DateTime.UtcNow;
            user = userRepository.Update(user);

            return FillUserModel(user);
        }

        public UserModel UpdateUser(UpdateUserModel updateUser)
        {
            var user = userRepository.Get().FirstOrDefault(i => updateUser.Guid == i.Guid && i.RevokedOn == null);
            if (user == null)
            {
                throw new InvalidOperationException("Revoked user not found");
            }

            if (user.Login != updateUser.Login && userRepository.Get().FirstOrDefault(i => updateUser.Login == i.Login) != null)
            {
                throw new InvalidOperationException("Login is already used");
            }

            user.Name = updateUser.Name;
            user.Login = updateUser.Login;
            user.Birthday = updateUser.Birthday;
            user.Gender = updateUser.Gender;
            user.ModifiedBy = updateUser.ModifiedBy;
            user.ModifiedOn = DateTime.UtcNow;

            var (success, msg) = Validate(user);

            if (!success)
            {
                throw new InvalidOperationException(msg);
            }

            user = userRepository.Update(user);

            return FillUserModel(user);
        }

        private string GetHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string hash, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private UserModel FillUserModel(User user)
        {
            var userModel = new UserModel
            {
                Guid = user.Guid,
                Birthday = user.Birthday,
                Gender = user.Gender,
                Login = user.Login,
                Name = user.Name,
                Admin = user.Admin,
                Revoked = user.RevokedOn != null,
            };
            return userModel;
        }

        private IQueryable<UserModel> FillUserModel(IQueryable<User> list)
        {
            return list
                .Select(user => 
                new UserModel {
                    Guid = user.Guid,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Login = user.Login,
                    Name = user.Name,
                    Admin = user.Admin,
                    Revoked = user.RevokedOn != null,
                });
        }

        private (bool, string) Validate(User user)
        {
            if (user.Birthday != null && user.Birthday > DateTime.UtcNow)
            {
                return (false, "Birthday is not valid");
            }

            var regLogin = new Regex("^[A-z0-9]+$");
            if (!regLogin.IsMatch(user.Login))
            {
                return (false, "Login is not valid");
            }

            var regName = new Regex("^[a-zа-яё]+$", RegexOptions.IgnoreCase);
            if (!regName.IsMatch(user.Name))
            {
                return (false, "Name is not valid");
            }

            return (true, "");
        }

        private (bool, string) ValidatePassword(string password)
        {
            var reg = new Regex("^[A-z0-9]+$");

            if (reg.IsMatch(password))
            {
                return (true, "");
            }
            else
            {
                return (false, "Password is not valid");
            }
        }
    }
}
