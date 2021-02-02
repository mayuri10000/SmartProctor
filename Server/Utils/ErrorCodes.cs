using System.Collections.Generic;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Utils
{
    public static class ErrorCodes
    {
        public const int Success = 0;
        public const int NotLoggedIn = 1000;
        public const int UserNameOrPasswordWrong = 1001;
        public const int UserIdExists = 1002;
        public const int EmailExists = 1003;
        public const int PhoneExists = 1004;
        public const int OldPasswordMismatch = 1005;

        public static Dictionary<int, string> MessageMap = new Dictionary<int, string>()
        {
            { 0000, "Success" },
            
            { 1000, "Please log in first" },
            { 1001, "User name or password incorrect" },
            { 1002, "User name already exists" },
            { 1003, "Email already exists" },
            { 1004, "Phone already exists" },
            { 1005, "Old Password Wrong"}
        };

        public static BaseResponseModel CreateSimpleResponse(int errorCode)
        {
            return new BaseResponseModel()
            {
                Code = errorCode,
                Message = MessageMap[errorCode]
            };
        }
    }
}