using System;
using System.Collections.Generic;

namespace SmartProctor.Server.Services
{
    /// <summary>
    /// Singleton class stores the user tokens.
    /// Used by <see cref="UserServices"/>
    /// </summary>
    public class UserTokenStorage
    {
        private Dictionary<string, string> _tokenToUserDict = new Dictionary<string, string>();
        
        private static UserTokenStorage _instance;

        public static UserTokenStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserTokenStorage();
                }

                return _instance;
            }
        }

        private UserTokenStorage()
        {
        }

        /// <summary>
        /// Generates a new token for the user
        /// </summary>
        /// <param name="uid">The user ID</param>
        /// <returns>The generated token</returns>
        public string GenerateToken(string uid)
        {
            foreach (var pair in _tokenToUserDict)
            {
                if (pair.Value == uid)
                {
                    return pair.Key;
                }
            }
            
            var token = Guid.NewGuid().ToString();
            _tokenToUserDict[token] = uid;
            return token;
        }

        /// <summary>
        /// Verify the token, and removes it from the storage
        /// </summary>
        /// <param name="token">The token to be verified</param>
        /// <returns>User ID if the token is verified, null if invalid token</returns>
        public string VerifyAndRemoveToken(string token)
        {
            if (_tokenToUserDict.TryGetValue(token, out string uid))
            {
                _tokenToUserDict.Remove(token);
                return uid;
            }

            return null;
        }
    }
}