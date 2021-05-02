using System;
using System.Collections.Generic;
using SmartProctor.Server.Data.Entities;
using SmartProctor.Server.Data.Repositories;
using SmartProctor.Server.Utils;

namespace SmartProctor.Server.Services
{
    /// <summary>
    /// Interface of user related service tier (business logic) methods.
    /// Implemented in <see cref="SmartProctor.Server.Services.UserServices"/>
    /// </summary>
    public interface IUserServices : IBaseServices<User>
    {
        /// <summary>
        /// Verifies login credentials, <see cref="userName"/> can be user ID, email or phone.
        /// <remarks>This will not sets the login cookies, should only be called by
        /// <see cref="SmartProctor.Server.Controllers.User.LoginController"/></remarks>
        /// </summary>
        /// <param name="userName">User ID, email or phone</param>
        /// <param name="password">Password</param>
        /// <returns>User ID if success, null if fails</returns>
        string Login(string userName, string password);
        
        /// <summary>
        /// Generates token that will be used for DeepLens login
        /// </summary>
        /// <param name="uid">current user Id</param>
        /// <returns>The token used for logging in on the DeepLens device</returns>
        string GenerateOneTimeToken(string uid);
        
        /// <summary>
        /// Validates the login token used for DeepLens login
        /// </summary>
        /// <param name="token">The login token</param>
        /// <returns>User ID if success, null if fails</returns>
        string ValidateOneTimeToken(string token);
        
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="id">User ID, should be unique</param>
        /// <param name="nickName">Nick name, not needed to be unique</param>
        /// <param name="password">Password</param>
        /// <param name="email">Email, should be unique</param>
        /// <param name="phone">Phone number, should be unique</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int Register(string id, string nickName, string password, string email, string phone);

        /// <summary>
        /// Changes the password of the user
        /// </summary>
        /// <param name="userName">User ID</param>
        /// <param name="oldPassword">The current password</param>
        /// <param name="newPassword">The new password</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int ChangePassword(string userName, string oldPassword, string newPassword);
        
        /// <summary>
        /// Modifies the user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="nickName">Nick name, not needed to be unique</param>
        /// <param name="email">Email, should be unique</param>
        /// <param name="phone">Phone number, should be unique</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int ChangeUserInfo(string id, string nickName, string email, string phone);
    }

    /// <summary>
    /// Implementation of user related business logic defined in <see cref="IUserServices"/>
    /// </summary>
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

        public string GenerateOneTimeToken(string uid)
        {
            return UserTokenStorage.Instance.GenerateToken(uid);
        }

        public string ValidateOneTimeToken(string token)
        {
            return UserTokenStorage.Instance.VerifyAndRemoveToken(token);
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
                // The password should be hashed with the username with salt
                Password = MD5Helper.HashPassword(id, password),
                Email = email,
                Phone = phone,
                Avatar = ""
            };
            SaveObject(u);
            return ErrorCodes.Success;
        }

        /// <summary>
        /// Check whether the user ID exists
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>True if exists</returns>
        private bool CheckIfIdExists(string id)
        {
            return GetCount(u => u.Id == id) != 0;
        }

        /// <summary>
        /// Check whether the email address exists
        /// </summary>
        /// <param name="email">email address</param>
        /// <returns>True if exists</returns>
        private bool CheckIfEmailExists(string email)
        {
            return GetCount(u => u.Email == email) != 0;
        }

        /// <summary>
        /// Check whether the phone exists
        /// </summary>
        /// <param name="phone">phone number</param>
        /// <returns>True if exists</returns>
        private bool CheckIfPhoneExists(string phone)
        {
            return GetCount(u => u.Phone == phone) != 0;
        }

        public int ChangePassword(string userName, string oldPassword, string newPassword)
        {
            // The password should be hashed with the username with salt
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