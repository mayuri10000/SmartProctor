using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SmartProctor.Server.Data;
using SmartProctor.Server.Data.Entities;
using SmartProctor.Server.Data.Repositories;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Services
{
    public interface IExamServices : IBaseServices<Exam>
    {
        int Attempt(int eid, string uid);
        int EnterProctor(int eid, string uid);
        IList<string> GetExamTakers(int eid);
        IList<string> GetProctors(int eid);

        IList<ExamDetails> GetExamsForUser(string uid, int role);
    }
    
    public class ExamServices : BaseServices<Exam>, IExamServices
    {
        private IExamUserRepository _examUserRepo;
        
        public ExamServices(IExamRepository repo, IExamUserRepository examUserRepo) : base(repo)
        {
            _examUserRepo = examUserRepo;
        }

        public int Attempt(int eid, string uid)
        {
            if (GetObject(eid) == null)
            {
                return ErrorCodes.ExamNotExist;
            }
            
            var q = _examUserRepo.GetFirstOrDefaultObject(x => x.ExamId == eid && x.UserId == uid && x.UserRole == 1);
            if (q == null)
            {
                return ErrorCodes.ExamNotPermitToTake;
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
                    select GetObject(x.ExamId)
                    into ex
                    where ex != null
                    select new ExamDetails()
                    {
                        Id = ex.Id,
                        Name = ex.Name,
                        Description = ex.Description,
                        Duration = ex.Duration,
                        StartTime = ex.StartTime
                    }).ToList();
            }
            catch
            {
                return null;
            }
        }
    }
}