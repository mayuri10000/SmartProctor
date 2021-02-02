using SmartProctor.Server.Data.Entities;
using SmartProctor.Server.Data.Repositories;
using SmartProctor.Server.Utils;

namespace SmartProctor.Server.Services
{
    public interface IUserServices : IBaseServices<User>
    {
        string Login(string userName, string password);
        int Register(string id, string nickName, string password, string email, string phone);

        bool CheckIfIdExists(string id);
        bool CheckIfEmailExists(string email);
        bool CheckIfPhoneExists(string phone);

        int ChangePassword(string userName, string oldPassword, string newPassword);
        int ChangeUserInfo(string id, string nickName, string email, string phone);
    }

    public class UserServices : BaseServices<User>, IUserServices
    {
        public UserServices(IUserRepository repo) : base(repo)
        {
        }

        public string Login(string userName, string password)
        {
            var user = GetObject(u => u.Id == userName || u.Email == userName || u.Phone == userName);
            if (user == null)
            {
                return null;
            }

            if (MD5Helper.HashPassword(user.Id, password) == user.Password)
            {
                return user.Id;
            }

            return null;
        }

        public int Register(string id, string nickName, string password, string email, string phone)
        {
            if (CheckIfIdExists(id))
            {
                return ErrorCodes.UserIdExists;
            }

            if (email != null && CheckIfEmailExists(email))
            {
                return ErrorCodes.EmailExists;
            }

            if (phone != null && CheckIfPhoneExists(phone))
            {
                return ErrorCodes.PhoneExists;
            }
            
            User u = new User()
            {
                Id = id,
                NickName = nickName,
                Password = MD5Helper.HashPassword(id, password),
                Email = email,
                Phone = phone
            };
            SaveObject(u);
            return ErrorCodes.Success;
        }

        public bool CheckIfIdExists(string id)
        {
            return GetCount(u => u.Id == id) != 0;
        }

        public bool CheckIfEmailExists(string email)
        {
            return GetCount(u => u.Email == email) != 0;
        }

        public bool CheckIfPhoneExists(string phone)
        {
            return GetCount(u => u.Phone == phone) != 0;
        }

        public int ChangePassword(string userName, string oldPassword, string newPassword)
        {
            var oldHash = MD5Helper.HashPassword(userName, oldPassword);
            var newHash = MD5Helper.HashPassword(userName, newPassword);
            var u = GetObject(userName);
            if (u.Password != oldHash)
            {
                return ErrorCodes.OldPasswordMismatch;
            }

            u.Password = newHash;
            SaveObject(u);
            return ErrorCodes.Success;
        }

        public int ChangeUserInfo(string id, string nickName, string email, string phone)
        {
            var u = GetObject(id);
            if (email != u.Email && CheckIfEmailExists(email))
            {
                return ErrorCodes.EmailExists;
            }

            if (phone != u.Phone && CheckIfPhoneExists(phone))
            {
                return ErrorCodes.PhoneExists;
            }
            
            u.NickName = nickName;
            u.Email = email;
            u.Phone = phone;
            SaveObject(u);
            return ErrorCodes.Success;
        }
    }
}