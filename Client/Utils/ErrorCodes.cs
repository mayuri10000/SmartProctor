using System.Collections.Generic;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Utils
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

        public static Dictionary<int, string> MessageMap = new Dictionary<int, string>()
        {
            { UnknownError,            "Unknown Error"},
            
            { Success,                 "Success" },
            
            { NotLoggedIn,             "Please log in first" },
            { UserNameOrPasswordWrong, "User name or password incorrect" },
            { UserIdExists,            "User name already exists" },
            { EmailExists,             "Email already exists" },
            { PhoneExists,             "Phone already exists" },
            { OldPasswordMismatch,     "Old Password Wrong"},
            
            { ExamNotExist,            "This exam does not exist"},
            { ExamNotPermitToTake,     "You have no permission to take this exam"},
            { ExamNotPermitToProctor,  "You have no permission to proctor this exam"},
            { ExamNotBegin,            "This exam have not begin yet."},
            { ExamExpired,             "This exam has expired"},
            { QuestionNotAnswered,     "Question not answered"},
            { ExamNotPermitToEdit,     "You have no permission to edit this exam"},
            { ExamAlreadyJoined,       "You have already joined the exam"},
            { ExamAlreadyProctored,    "The selected user is already a proctor of the exam"},
            { ExamTakerBanned,         "You have been banned from the exam"},
            { ExamMaxTakerReached,     "This exam's taker count have already reached the limit."}
        };
    }
}