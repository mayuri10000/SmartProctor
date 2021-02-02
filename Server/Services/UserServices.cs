using SmartProctor.Server.Data.Entities;
using SmartProctor.Server.Data.Repositories;
using SmartProctor.Server.Utils;

namespace SmartProctor.Server.Services
{
    public interface IUserServices : IBaseServices<User>
    {
        bool CheckUserNameAndPassword(string userName, string password);
        int Register(string id, string nickName, string password, string email, string phone);

        bool CheckIfIdExists(string id);
        bool CheckIfEmailExists(string email);
        bool CheckIfPhoneExists(string phone);
    }

    public class UserServices : BaseServices<User>, IUserServices
    {
        public UserServices(IUserRepository repo) : base(repo)
        {
        }

        public bool CheckUserNameAndPassword(string userName, string password)
        {
            var user = GetObject(u => u.Id == userName || u.Email == userName || u.Phone == userName);
            if (user == null)
            {
                return false;
            }
            
            return MD5Helper.HashPassword(user.Id, password) == user.Password;
        }

        public int Register(string id, string nickName, string password, string? email, string? phone)
        {
            throw new System.NotImplementedException();
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
    }
}