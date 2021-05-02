using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using SmartProctor.Server.Data;
using SmartProctor.Server.Data.Entities;
using SmartProctor.Server.Data.Repositories;
using SmartProctor.Server.Utils;
using SmartProctor.Shared;
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Services
{
    /// <summary>
    /// Interface of exam related service tier (business logic) methods.
    /// Implemented in <see cref="SmartProctor.Server.Services.ExamServices"/>
    /// </summary>
    public interface IExamServices : IBaseServices<Exam>
    {
        /// <summary>
        /// Called when entering the exam, verify the exam is eligible for
        /// current user to take
        /// </summary>
        /// <param name="eid">Exam Id</param>
        /// <param name="uid">User Id</param>
        /// <param name="banReason">If the user was banned, outputs the reason</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int Attempt(int eid, string uid, out string banReason);
        
        /// <summary>
        /// Verify the exam is eligible for current user to proctor
        /// </summary>
        /// <param name="eid">Exam Id</param>
        /// <param name="uid">User Id</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int EnterProctor(int eid, string uid);
        
        /// <summary>
        /// Returns the exam takers for the current exam
        /// </summary>
        /// <param name="eid">Exam Id</param>
        /// <returns>List of user ids of the test takers, <code>null</code> if error occurs</returns>
        IList<(string, string)> GetExamTakers(int eid);
        
        /// <summary>
        /// Returns the proctors for the current exam
        /// </summary>
        /// <param name="eid">Exam Id</param>
        /// <returns>List of user ids of the test proctors, null if error occurs</returns>
        IList<string> GetProctors(int eid);

        /// <summary>
        /// Joins a specific exam
        /// </summary>
        /// <param name="uid">The user ID of current user</param>
        /// <param name="eid">The exam ID of the exam to join</param>
        /// <param name="banReason">Returns reason of banning if the current user was banned from the exam</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int JoinExam(string uid, int eid, out string banReason);

        /// <summary>
        /// Adds proctor to the exam, should be used by exam creator
        /// </summary>
        /// <param name="currentUid">The current user ID, used for verifying that the current user is the owner of the exam</param>
        /// <param name="uid">The user ID to be added as proctor</param>
        /// <param name="eid">The exam ID</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int AddProctor(string currentUid, string uid, int eid);
        
        /// <summary>
        /// Get a list of exams that the user should take/proctor.
        /// </summary>
        /// <param name="uid">Current user ID</param>
        /// <param name="role">Role the user should be in the result exams (see <see cref="Consts"/>)</param>
        /// <returns></returns>
        IList<ExamDetails> GetExamsForUser(string uid, int role);

        /// <summary>
        /// Gets a list of question in one exam, used by both exam taker and creator.
        /// When used by exam takers, this method will success only within the exam session.
        /// </summary>
        /// <param name="uid">The current user ID, for role verification.</param>
        /// <param name="eid">The exam ID</param>
        /// <param name="questions">The list of questions in json format, null if failed</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int GetPaper(string uid, int eid, out IList<string> questions);

        /// <summary>
        /// Submits a new version of the exam paper (i.e. list of question)
        /// </summary>
        /// <param name="uid">The current user ID, for role verification</param>
        /// <param name="eid">The exam ID</param>
        /// <param name="questions">The list of question in json format</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int EditPaper(string uid, int eid, IList<string> questions);

        /// <summary>
        /// Submits the answer, should be used by the exam taker during the exam session.
        /// </summary>
        /// <param name="uid">Current user ID</param>
        /// <param name="eid">The exam ID</param>
        /// <param name="num">The question number to be answered</param>
        /// <param name="json">The answer data in json format</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int SubmitAnswer(string uid, int eid, int num, string json);

        /// <summary>
        /// Gets the exam answer of one question, used by both exam takers and exam creator.
        /// Exam taker can only call this method during the exam session.
        /// </summary>
        /// <param name="currentUid">The current user ID, for role verification</param>
        /// <param name="uid">The user ID of the user whose answer will be returned, will be ignored when
        /// the current user is an exam taker.</param>
        /// <param name="eid">The exam ID</param>
        /// <param name="num">The question number</param>
        /// <param name="json">Returns the answer in json format, null if failed</param>
        /// <param name="time">Return the time when the answer was submitted, <see cref="DateTime.MinValue"/>
        /// if failed</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int GetAnswer(string currentUid, string uid, int eid, int num, out string json, out DateTime time);

        /// <summary>
        /// Gets the question count of a exam
        /// </summary>
        /// <param name="examId"></param>
        /// <returns>The question count</returns>
        int GetQuestionCount(int examId);
        
        /// <summary>
        /// Gets a list of exam created by the current user.
        /// </summary>
        /// <param name="uid">Current user ID</param>
        /// <returns>List of exam created by the current user.</returns>
        IList<ExamDetails> GetCreatedExams(string uid);
        
        /// <summary>
        /// Bans a exam taker, should be called by proctors
        /// </summary>
        /// <param name="eid">The exam ID</param>
        /// <param name="uid">The current user Id, for role verification</param>
        /// <param name="takerUid">The user Id of the exam taker to be banned</param>
        /// <param name="reason">Specify the reason why the exam taker should be banned.</param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int BanExamTaker(int eid, string uid, string takerUid, string reason);
        
        /// <summary>
        /// Adds a event message into the database.
        /// <remarks>
        /// This will not send the message, this method should be called by
        /// <see cref="SmartProctor.Server.Controllers.Exam.SendEventController"/> which sends the message with SignalR.
        /// </remarks>
        /// </summary>
        /// <param name="eid">The exam ID</param>
        /// <param name="senderUid">The user Id of the sender</param>
        /// <param name="receiptUid">The user ID of the receipt, null for broadcast message</param>
        /// <param name="type">Message type, see <see cref="Consts"/></param>
        /// <param name="message">The message text</param>
        /// <param name="attachment">The attachment image path, currently only used with <see cref="Consts.MessageTypeWarning"/></param>
        /// <returns><see cref="ErrorCodes.Success"/> if succeed, error code if fails</returns>
        int AddEvent(int eid, string senderUid, string receiptUid, int type, string message, string attachment = null);
        
        /// <summary>
        /// Gets a list of events associated with the current user in a specific exam.
        /// </summary>
        /// <param name="uid">The user ID</param>
        /// <param name="eid">The exam ID</param>
        /// <param name="type">Message type, see <see cref="Consts"/></param>
        /// <returns>A list of event messages, null if failed</returns>
        IList<EventItem> GetEvents(string uid, int eid, int type);
    }
    
    /// <summary>
    /// Implementation of exam related business logic defined in <see cref="IExamServices"/>
    /// </summary>
    public class ExamServices : BaseServices<Exam>, IExamServices
    {
        private IExamUserRepository _examUserRepo;
        private IQuestionRepository _questionRepo;
        private IAnswerRepository _answerRepo;
        private IEventRepository _eventRepo;
        private IUserServices _userServices;

        private ILogger<ExamServices> _logger;
        
        public ExamServices(IExamRepository repo, IExamUserRepository examUserRepo, 
            IQuestionRepository questionRepo, IAnswerRepository answerRepo, IEventRepository eventRepo, IUserServices userServices, ILogger<ExamServices> logger) : base(repo)
        {
            _examUserRepo = examUserRepo;
            _questionRepo = questionRepo;
            _answerRepo = answerRepo;
            _eventRepo = eventRepo;
            _userServices = userServices;

            _logger = logger;
        }

        public int Attempt(int eid, string uid, out string banReason)
        {
            _logger.LogDebug($"Attempt(eid: {eid}, uid: \"{uid}\")");
            banReason = null;
            try
            {
                // Exam ID not found
                if (GetObject(eid) == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                // Gets the exam-user relationship
                var q = _examUserRepo.GetFirstOrDefaultObject(
                    x => x.ExamId == eid && x.UserId == uid && x.UserRole == Consts.UserTypeTaker);
                // The exam taker not joined the exam
                if (q == null)
                {
                    return ErrorCodes.ExamNotPermitToTake;
                }

                // The exam was banned
                if (q.BanReason != null)
                {
                    banReason = q.BanReason;
                    return ErrorCodes.ExamTakerBanned;
                }

                var e = GetObject(eid);

                // Exam takers can enter the exam 5 minutes before the exam begin (but cannot view or answer questions)
                if (e.StartTime > DateTime.Now.AddMinutes(5))
                {
                    return ErrorCodes.ExamNotBegin;
                }

                // TODO: Add late-entry check
                if (e.StartTime.AddSeconds(e.Duration) < DateTime.Now)
                {
                    return ErrorCodes.ExamExpired;
                }

                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public int EnterProctor(int eid, string uid)
        {
            _logger.LogInformation($"EnterProctor(eid: {eid}, uid: \"{uid}\")");
            try
            {
                if (GetObject(eid) == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var q = _examUserRepo.GetFirstOrDefaultObject(
                    x => x.ExamId == eid && x.UserId == uid && x.UserRole == Consts.UserTypeProctor);
                if (q == null)
                {
                    return ErrorCodes.ExamNotPermitToProctor;
                }

                var e = GetObject(eid);

                // Proctors can enter the exam 15 minutes before the exam begin 
                if (e.StartTime > DateTime.Now.AddMinutes(15))
                {
                    return ErrorCodes.ExamNotBegin;
                }

                if (e.StartTime.AddSeconds(e.Duration) < DateTime.Now)
                {
                    return ErrorCodes.ExamExpired;
                }

                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public IList<(string, string)> GetExamTakers(int eid)
        {
            _logger.LogInformation($"GetExamTaker(eid: {eid})");
            if (GetObject(eid) == null)
            {
                return null;
            }

            var q = _examUserRepo.GetObjectList(x => x.ExamId == eid && x.UserRole == Consts.UserTypeTaker, x => x.UserId,
                OrderingType.Ascending);

            var ret = new List<(string, string)>();
            foreach (var x in q)
            {
                ret.Add((x.UserId, x.BanReason));
            }

            return ret;
        }

        public int JoinExam(string uid, int eid, out string banReason)
        {
            _logger.LogInformation($"JoinExam(uid: \"{uid}\", eid: {eid}, out banReason)");
            banReason = null;
            try
            {
                var exam = GetObject(eid);

                if (exam == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                if (exam.StartTime.AddSeconds(exam.Duration) < DateTime.Now)
                {
                    return ErrorCodes.ExamExpired;
                }

                var count = _examUserRepo.ObjectCount(x => x.ExamId == eid && x.UserRole == Consts.UserTypeTaker && x.BanReason == null);

                if (count >= exam.MaximumTakersNum)
                {
                    return ErrorCodes.ExamMaxTakerReached;
                }

                var eu = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid);

                if (eu != null)
                {
                    if (eu.UserRole != Consts.UserTypeTaker)
                        return ErrorCodes.ExamAlreadyProctored;

                    banReason = eu.BanReason;
                    return eu.BanReason == null ? ErrorCodes.ExamAlreadyJoined : ErrorCodes.ExamTakerBanned;
                }

                eu = new ExamUser()
                {
                    ExamId = eid,
                    UserId = uid,
                    UserRole = 1,
                    BanReason = null
                };

                _examUserRepo.Add(eu);

                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public IList<string> GetProctors(int eid)
        {
            if (GetObject(eid) == null)
            {
                return null;
            }

            var q = _examUserRepo.GetObjectList(x => x.ExamId == eid && x.UserRole == Consts.UserTypeProctor, x => x.UserId,
                OrderingType.Ascending);

            var ret = new List<string>();
            foreach (var x in q)
            {
                ret.Add(x.UserId);
            }

            return ret;
        }

        public int AddProctor(string currentUid, string uid, int eid)
        {
            _logger.LogInformation($"AddProctor(currentUid: \"{currentUid}\", uid: \"{uid}\", eid: {eid})");
            try
            {
                var exam = GetObject(eid);
                if (exam == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var user = _userServices.GetObject(uid);
                if (user == null)
                {
                    return ErrorCodes.UserNotExists;
                }

                if (exam.Creator != currentUid)
                {
                    return ErrorCodes.ExamNotPermitToEdit;
                }

                var q = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid);

                if (q != null)
                {
                    if (q.UserRole == Consts.UserTypeTaker)
                    {
                        return ErrorCodes.ExamAlreadyJoined;
                    }
                    else if (q.UserRole == Consts.UserTypeProctor)
                    {
                        return ErrorCodes.ExamAlreadyProctored;
                    }
                }
                else
                {
                    _examUserRepo.Save(new ExamUser()
                    {
                        ExamId = eid,
                        UserId = uid,
                        UserRole = Consts.UserTypeProctor
                    });
                }

                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public IList<ExamDetails> GetExamsForUser(string uid, int role)
        {
            _logger.LogInformation($"GetExamsForUser(uid: \"{uid}\", role: {role})");
            try
            {
                var q = _examUserRepo.GetObjectList(x => x.UserId == uid && x.UserRole == role,
                    x => x.ExamId, OrderingType.Ascending);

                return (from x in q
                    select (GetObject(x.ExamId), x)
                    into ex
                    where ex.Item1 != null
                    select new ExamDetails()
                    {
                        Id = ex.Item1.Id,
                        Name = ex.Item1.Name,
                        Description = ex.Item1.Description,
                        Duration = ex.Item1.Duration,
                        StartTime = ex.Item1.StartTime,
                        OpenBook = ex.Item1.OpenBook,
                        BanReason = ex.Item2?.BanReason
                    }).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return null;
            }
        }
        public int GetPaper(string uid, int eid, out IList<string> questions)
        {
            _logger.LogInformation($"GetPaper(uid: \"{uid}\", eid: {eid})");
            questions = null;
            try
            {
                var exam = GetObject(eid);
                if (exam == null)
                {
                    return ErrorCodes.ExamNotExist;
                }
                
                var role = GetUserRoleInExam(uid, eid);
                if (role == Consts.UserTypeTaker)
                {
                    var err = CheckEarlyOrLate(eid, true);

                    if (err != ErrorCodes.Success)
                    {
                        return err;
                    }
                }
                else if (role == Consts.UserTypeBanned)
                {
                    return ErrorCodes.ExamTakerBanned;
                }
                else if (exam.Creator != uid)
                {
                    return ErrorCodes.ExamNotPermitToTake;
                }

                var q = _questionRepo.GetObjectList(x => x.ExamId == eid, x => x.Number, OrderingType.Ascending);

                questions = q.Select(x => x.QuestionJson).ToList();
                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }


        public int EditPaper(string uid, int eid, IList<string> questions)
        {
            _logger.LogInformation($"EditPaper(uid: \"{uid}\", eid: {eid}, questions: [...])");
            try
            {
                var exam = GetObject(eid);
                if (exam.Creator != uid)
                {
                    return ErrorCodes.ExamNotPermitToEdit;
                }

                var answerCount = _answerRepo.ObjectCount(x => x.ExamId == eid);
                if (answerCount > 0)
                {
                    return ErrorCodes.ExamAlreadyAnswered;
                }
                
                _questionRepo.Delete(x => x.ExamId == eid);

                for (var i = 0; i < questions.Count; i++)
                {
                    _questionRepo.Add(new Question()
                    {
                        ExamId = eid,
                        Number = i + 1,
                        QuestionJson = questions[i]
                    });
                }

                _questionRepo.SaveChanges();

                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public int SubmitAnswer(string uid, int eid, int num, string json)
        {
            _logger.LogInformation($"SubmitAnswer(uid: \"{uid}\", eid: {eid}, num: {num}, json: \"{json}\")");
            try
            {
                if (GetUserRoleInExam(uid, eid) != Consts.UserTypeTaker)
                {
                    return ErrorCodes.ExamNotPermitToTake;
                }

                var err = CheckEarlyOrLate(eid, true);
                if (err != ErrorCodes.Success)
                {
                    return err;
                }

                var q = _answerRepo.GetFirstOrDefaultObject(x =>
                    x.UserId == uid && x.ExamId == eid && x.QuestionNum == num);

                if (q == null)
                {
                    q = new Answer()
                    {
                        ExamId = eid,
                        QuestionNum = num,
                        AnswerTime = DateTime.Now,
                        UserId = uid,
                        AnswerJson = json
                    };
                    _answerRepo.Add(q);
                }
                else
                {
                    q.AnswerJson = json;
                    q.AnswerTime = DateTime.Now;
                }

                _answerRepo.SaveChanges();

                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public int GetAnswer(string currentUid, string uid, int eid, int num, out string json, out DateTime time)
        {
            json = null;
            time = DateTime.MinValue;
            _logger.LogInformation($"GetAnswer(currentUid: \"{currentUid}\", uid: \"{uid}\", eid: {eid}, num: {num})");
            try
            {
                var exam = GetObject(eid);

                if (exam == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var role = GetUserRoleInExam(currentUid, eid);
                if (role == Consts.UserTypeTaker)
                {
                    var ret = CheckEarlyOrLate(eid, true);
                    if (ret == ErrorCodes.Success)
                    {
                        var answer =
                            _answerRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == currentUid && x.QuestionNum == num);
                        if (answer == null)
                        {
                            return ErrorCodes.QuestionNotAnswered;
                        }
                        json = answer.AnswerJson;
                        time = answer.AnswerTime ?? DateTime.MinValue;
                    }

                    return ret;
                }
                else if (exam.Creator == currentUid)
                {
                    var answer =
                        _answerRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid && x.QuestionNum == num);
                    if (answer == null)
                    {
                        return ErrorCodes.QuestionNotAnswered;
                    }
                    json = answer.AnswerJson;
                    time = answer.AnswerTime ?? DateTime.MinValue;

                    return ErrorCodes.Success;
                }
                else
                {
                    return ErrorCodes.ExamNotPermitToTake;
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public int GetQuestionCount(int examId)
        {
            return _questionRepo.ObjectCount(x => x.ExamId == examId);
        }

        public IList<ExamDetails> GetCreatedExams(string uid)
        {
            _logger.LogInformation($"GetCreatedExams(uid: \"{uid}\")");
            try
            {
                var q = GetObjectList(x => x.Creator == uid, x => x.Id, OrderingType.Ascending);

                return (from x in q
                    select x
                    into x
                    where x != null
                    select new ExamDetails()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Duration = x.Duration,
                        StartTime = x.StartTime,
                        OpenBook = x.OpenBook,
                    }).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return null;
            }
        }

        public int BanExamTaker(int eid, string uid, string takerUid, string reason)
        {
            _logger.LogInformation($"BanExamTaker(eid: {eid}, uid: \"{uid}\", takerUid: \"{takerUid}\", reason: \"{reason}\"");
            try
            {
                var e = GetObject(eid);

                if (e == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var q1 = _examUserRepo.GetFirstOrDefaultObject(x =>
                    x.ExamId == eid && x.UserId == uid && x.UserRole == 2);
                if (q1 == null && e.Creator != uid)
                {
                    return ErrorCodes.ExamNotPermitToProctor;
                }

                var q2 = _examUserRepo.GetFirstOrDefaultObject(x =>
                    x.ExamId == eid && x.UserId == takerUid && x.UserRole == 1);

                if (q2 != null)
                {
                    q2.BanReason = reason;
                    _examUserRepo.Save(q2);
                }

                return ErrorCodes.Success;
            }
            catch
            {
                _logger.LogError(eid.ToString());
                return ErrorCodes.UnknownError;
            }
        }
        
        public int AddEvent(int eid, string senderUid, string receiptUid, int type, string message, string attachment = null)
        {
            _logger.LogInformation($"AddEvent(eid: {eid}, senderUid: \"{senderUid}\", receiptUid: \"{receiptUid}\"," +
                             $" type: {type}, message: \"{message}\", attachment: {attachment}");
            try
            {
                var e = GetObject(eid);
                if (e == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var ev = new Event()
                {
                    ExamId = eid,
                    Sender = senderUid,
                    Receipt = receiptUid,
                    Message = message,
                    Attachment = attachment,
                    Time = DateTime.Now,
                    Type = type
                };

                _eventRepo.Save(ev);

                return ErrorCodes.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public IList<EventItem> GetEvents(string uid, int eid, int type)
        {
            _logger.LogInformation($"GetEvents(uid: \"{uid}\", eid: {eid}, type: {type})");
            try
            {
                IList<Event> q;
                
                q = _eventRepo.GetObjectList(x => x.ExamId == eid && x.Type == type &&
                                                  (x.Sender == uid || x.Receipt == uid || x.Receipt == null),
                        x => x.Time, OrderingType.Ascending);
                

                return q.Select(x => new EventItem()
                {
                    Time = x.Time,
                    Message = x.Message,
                    Receipt = x.Receipt,
                    Sender = x.Sender,
                    Attachment = x.Attachment,
                    Type = x.Type
                }).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Check whether the current user is early or late for the exam
        /// </summary>
        /// <param name="eid">Exam ID</param>
        /// <param name="viewQuestions">Questions should only be viewed after the exam begins, true if this method
        /// is called for checking before getting questions</param>
        /// <returns><see cref="ErrorCodes.Success"/> if the exam taker is neither late or early</returns>
        private int CheckEarlyOrLate(int eid, bool viewQuestions)
        {
            var e = GetObject(eid);

            if (e == null)
            {
                return ErrorCodes.ExamNotExist;
            }

            if (DateTime.Now < (viewQuestions ? e.StartTime : e.StartTime.AddMinutes(-15)))
            {
                return ErrorCodes.ExamNotBegin;
            }

            if (DateTime.Now > e.StartTime.AddSeconds(e.Duration))
            {
                return ErrorCodes.ExamExpired;
            }

            return ErrorCodes.Success;
        }
        
        /// <summary>
        /// Gets the role of the user in the exam.
        /// </summary>
        /// <param name="uid">The user ID</param>
        /// <param name="eid">The exam Id</param>
        /// <returns></returns>
        private int GetUserRoleInExam(string uid, int eid)
        {
            var e = GetObject(eid);

            var q = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid);

            if (q == null)
            {
                return Consts.UserTypeNoPermission;
            }

            if (q.BanReason != null)
            {
                return Consts.UserTypeBanned;
            }

            return q.UserRole ?? 0;
        }
    }
} 