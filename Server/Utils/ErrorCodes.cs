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
        public const int QuestionNotAnswered = 2005;
        public const int ExamNotPermitToEdit = 2006;
        public const int ExamAlreadyJoined = 2007;
        public const int ExamAlreadyProctored = 2008;
        public const int ExamTakerBanned = 2009;
        public const int ExamMaxTakerReached = 2010;

        public static BaseResponseModel CreateSimpleResponse(int errorCode)
        {
            return new BaseResponseModel()
            {
                Code = errorCode,
            };
        }
    }
}