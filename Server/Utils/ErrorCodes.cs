using System.Collections.Generic;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Utils
{
    public static class ErrorCodes
    {
        public const int UnknownError = -1;
        public const int Success = 0;
        public const int NotLoggedIn = 1000;
        public const int UserNameOrPasswordWrong = 1001;
        public const int UserIdExists = 1002;
        public const int EmailExists = 1003;
        public const int PhoneExists = 1004;
        public const int OldPasswordMismatch = 1005;

        public const int ExamNotExist = 2000;
        public const int ExamNotPermitToTake = 2001;
        public const int ExamNotPermitToProctor = 2002;
        public const int ExamNotBegin = 2003;
        public const int ExamExpired = 2004;
        public const int QuestionNotExist = 2005;
        public const int ExamNotPermitToEdit = 2006;

        public static Dictionary<int, string> MessageMap = new Dictionary<int, string>()
        {
            {  -1, "Unknown Error"},
            
            { 0000, "Success" },
            
            { 1000, "Please log in first" },
            { 1001, "User name or password incorrect" },
            { 1002, "User name already exists" },
            { 1003, "Email already exists" },
            { 1004, "Phone already exists" },
            { 1005, "Old Password Wrong"},
            
            { 2000, "This exam does not exist"},
            { 2001, "You have no permission to take this exam"},
            { 2002, "You have no permission to proctor this exam"},
            { 2003, "This exam have not begin yet."},
            { 2004, "This exam has expired"},
            { 2005, "The question number does not exist in the exam"}
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