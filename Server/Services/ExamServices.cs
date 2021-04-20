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
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Services
{
    public interface IExamServices : IBaseServices<Exam>
    {
        /// <summary>
        /// Called when entering the exam, verify the exam is eligible for
        /// current user to take
        /// </summary>
        /// <param name="eid">Exam Id</param>
        /// <param name="uid">User Id</param>
        /// <param name="banReason">If the user was banned, outputs the reason</param>
        /// <returns>Error code, <code>ErrorCodes.Success</code> if no error</returns>
        int Attempt(int eid, string uid, out string banReason);
        
        /// <summary>
        /// Verify the exam is eligible for current user to proctor
        /// </summary>
        /// <param name="eid">Exam Id</param>
        /// <param name="uid">User Id</param>
        /// <returns>Error code, <code>ErrorCodes.Success</code> if no error</returns>
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
        /// <returns>List of user ids of the test proctors, <code>null</code> if error occurs</returns>
        IList<string> GetProctors(int eid);

        int JoinExam(string uid, int eid, out string banReason);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        IList<ExamDetails> GetExamsForUser(string uid, int role);

        int GetPaper(string uid, int eid, out IList<string> questions);

        int EditPaper(string uid, int eid, IList<string> questions);

        int SubmitAnswer(string uid, int eid, int num, string json);

        int GetAnswer(string currentUid, string uid, int eid, int num, out string json, out DateTime time);

        int GetQuestionCount(int examId);
        IList<ExamDetails> GetCreatedExams(string uid);
        
        int BanExamTaker(int eid, string uid, string takerUid, string reason);
    }
    
    public class ExamServices : BaseServices<Exam>, IExamServices
    {
        private IExamUserRepository _examUserRepo;
        private IQuestionRepository _questionRepo;
        private IAnswerRepository _answerRepo;

        private ILogger<ExamServices> _logger;
        
        public ExamServices(IExamRepository repo, IExamUserRepository examUserRepo, 
            IQuestionRepository questionRepo, IAnswerRepository answerRepo, ILogger<ExamServices> logger) : base(repo)
        {
            _examUserRepo = examUserRepo;
            _questionRepo = questionRepo;
            _answerRepo = answerRepo;

            _logger = logger;
        }

        public int Attempt(int eid, string uid, out string banReason)
        {
            _logger.LogInformation($"Attempt eid = {eid}, uid = {uid}");
            banReason = null;
            try
            {
                if (GetObject(eid) == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var q = _examUserRepo.GetFirstOrDefaultObject(
                    x => x.ExamId == eid && x.UserId == uid && x.UserRole == 1);
                if (q == null)
                {
                    return ErrorCodes.ExamNotPermitToTake;
                }

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
            _logger.LogInformation($"EnterProctor eid = {eid}, uid = {uid}");
            try
            {
                if (GetObject(eid) == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var q = _examUserRepo.GetFirstOrDefaultObject(
                    x => x.ExamId == eid && x.UserId == uid && x.UserRole == 2);
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
            _logger.LogInformation($"GetExamTaker eid = {eid}");
            if (GetObject(eid) == null)
            {
                return null;
            }

            var q = _examUserRepo.GetObjectList(x => x.ExamId == eid && x.UserRole == 1, x => x.UserId,
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
            banReason = null;
            var exam = GetObject(eid);

            if (exam == null)
            {
                return ErrorCodes.ExamNotExist;
            }

            if (exam.StartTime.AddSeconds(exam.Duration) < DateTime.Now)
            {
                return ErrorCodes.ExamExpired;
            }

            var eu = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid);

            if (eu != null)
            {
                if (eu.UserRole != 1) 
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

        public IList<string> GetProctors(int eid)
        {
            if (GetObject(eid) == null)
            {
                return null;
            }

            var q = _examUserRepo.GetObjectList(x => x.ExamId == eid && x.UserRole == 2, x => x.UserId,
                OrderingType.Ascending);

            var ret = new List<string>();
            foreach (var x in q)
            {
                ret.Add(x.UserId);
            }

            return ret;
        }

        public IList<ExamDetails> GetExamsForUser(string uid, int role)
        {
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
            _logger.LogInformation($"GetPaper, uid = {uid}, eid = {eid}");
            questions = null;
            try
            {
                if (GetObject(eid) == null)
                {
                    return ErrorCodes.ExamNotExist;
                }
                
                var role = GetUserRoleInExam(uid, eid);
                if (role == 1)
                {
                    var err = CheckEarlyOrLate(eid, true);

                    if (err != ErrorCodes.Success)
                    {
                        return err;
                    }
                }
                else if (role == -1)
                {
                    return ErrorCodes.ExamTakerBanned;
                }
                else if (role != 3)
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
            try
            {
                if (GetUserRoleInExam(uid, eid) != 3)
                {
                    return ErrorCodes.ExamNotPermitToEdit;
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
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        public int SubmitAnswer(string uid, int eid, int num, string json)
        {
            try
            {
                if (GetUserRoleInExam(uid, eid) != 1)
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
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        public int GetAnswer(string currentUid, string uid, int eid, int num, out string json, out DateTime time)
        {
            json = null;
            time = DateTime.MinValue;
            try
            {
                var exam = GetObject(eid);

                if (exam == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var role = GetUserRoleInExam(currentUid, eid);
                if (role == 1)
                {
                    var ret = CheckEarlyOrLate(eid, true);
                    if (ret == ErrorCodes.Success)
                    {
                        var answer =
                            _answerRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == currentUid && x.QuestionNum == num);
                        json = answer.AnswerJson;
                        time = answer.AnswerTime ?? DateTime.MinValue;
                    }

                    return ret;
                }
                else if (role == 3)
                {
                    var answer =
                        _answerRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid && x.QuestionNum == num);
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
                return ErrorCodes.UnknownError;
            }
        }

        public int GetQuestionCount(int examId)
        {
            return _questionRepo.ObjectCount(x => x.ExamId == examId);
        }

        public IList<ExamDetails> GetCreatedExams(string uid)
        {
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
            catch
            {
                return null;
            }
        }

        public int BanExamTaker(int eid, string uid, string takerUid, string reason)
        {
            try
            {
                var e = GetObject(eid);

                if (e == null)
                {
                    return ErrorCodes.ExamNotExist;
                }

                var q1 = _examUserRepo.GetFirstOrDefaultObject(x =>
                    x.ExamId == eid && x.UserId == uid && x.UserRole == 2);
                if (q1 == null)
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
                return ErrorCodes.UnknownError;
            }
        }

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

        private int GetUserRoleInExam(string uid, int eid)
        {
            var e = GetObject(eid);

            if (e.Creator == uid)
            {
                return 3;
            }

            var q = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid);

            if (q == null)
            {
                return 0;
            }

            if (q.BanReason != null)
            {
                return -1;
            }

            return q.UserRole ?? 0;
        }
    }
} 