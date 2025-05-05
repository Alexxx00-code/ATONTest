using Application.Models;

namespace Application.Interfaces
{
    public interface IUserService
    {
        UserModel Create(CreateUserModel model);

        UserModel? CheakPassword(string login, string password);

        UserModel UpdateUser(UpdateUserModel updateUser);

        UserModel UpdatePassword(UpdatePassword updatePassword);

        List<UserModel> GetList(uint minAge, bool revoked);

        UserModel? GetByLogin(string login);

        UserModel RemoveUser(string login, bool hard, string adminName);

        UserModel RestoreUser(string login);

    }
}
