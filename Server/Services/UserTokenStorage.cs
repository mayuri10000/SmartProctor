using System;
using System.Collections.Generic;

namespace SmartProctor.Server.Services
{
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
            _tokenToUserDict.Add(token, uid);
            return token;
        }

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