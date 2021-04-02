using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
        IList<string> GetExamTakers(int eid);
        
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

        int GetQuestion(string uid, int eid, int num, out Question question);

        int EditQuestion(string uid, int eid, int num, string json);

        int SubmitAnswer(string uid, int eid, int num, string json);

        int GetAnswer(string currentUid, string uid, int eid, int num, out string json);

        int GetQuestionCount(int examId);
        IList<ExamDetails> GetCreatedExams(string uid);
    }
    
    public class ExamServices : BaseServices<Exam>, IExamServices
    {
        private IExamUserRepository _examUserRepo;
        private IQuestionRepository _questionRepo;
        private IAnswerRepository _answerRepo;
        
        public ExamServices(IExamRepository repo, IExamUserRepository examUserRepo, 
            IQuestionRepository questionRepo, IAnswerRepository answerRepo) : base(repo)
        {
            _examUserRepo = examUserRepo;
            _questionRepo = questionRepo;
            _answerRepo = answerRepo;
        }

        public int Attempt(int eid, string uid, out string banReason)
        {
            banReason = null;
            if (GetObject(eid) == null)
            {
                return ErrorCodes.ExamNotExist;
            }
            
            var q = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid && x.UserRole == 1);
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

        public int EnterProctor(int eid, string uid)
        {
            if (GetObject(eid) == null)
            {
                return ErrorCodes.ExamNotExist;
            }
            
            var q = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid && x.UserRole == 2);
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

        public IList<string> GetExamTakers(int eid)
        {
            if (GetObject(eid) == null)
            {
                return null;
            }

            var q = _examUserRepo.GetObjectList(x => x.ExamId == eid && x.UserRole == 1, x => x.UserId,
                OrderingType.Ascending);

            var ret = new List<string>();
            foreach (var x in q)
            {
                ret.Add(x.UserId);
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
            catch
            {
                return null;
            }
        }

        public int GetQuestion(string uid, int eid, int num, out Question question)
        {
            question = null;
            try
            {
                var role = GetUserRoleInExam(uid, eid);
                if (role == 1)
                {
                    var err = CheckEarlyOrLate(eid, true);

                    if (err != ErrorCodes.Success)
                    {
                        return err;
                    }
                }
                else if (role != 3)
                {
                    return ErrorCodes.ExamNotPermitToTake;
                }

                var q = _questionRepo.GetFirstOrDefaultObject(x => x.Number == num && x.ExamId == eid);

                if (q == null)
                {
                    return ErrorCodes.QuestionNotExist;
                }

                question = q;
                return ErrorCodes.Success;
            }
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }


        public int EditQuestion(string uid, int eid, int num, string json)
        {
            try
            {
                if (GetUserRoleInExam(uid, eid) != 3)
                {
                    return ErrorCodes.ExamNotPermitToEdit;
                }
                
                var q = _questionRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.Number == num);

                if (q == null)
                {
                    q = new Question()
                    {
                        ExamId = eid,
                        Number = num,
                        QuestionJson = json
                    };
                    _questionRepo.Add(q);
                }
                else
                {
                    q.QuestionJson = json;
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

        public int GetAnswer(string currentUid, string uid, int eid, int num, out string json)
        {
            throw new NotImplementedException();
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

            return q.UserRole ?? 0;
        }
    }
} 