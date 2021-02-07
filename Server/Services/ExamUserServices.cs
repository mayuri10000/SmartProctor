using System.Linq;
using System.Collections.Generic;
using SmartProctor.Server.Data;
using SmartProctor.Server.Data.Entities;
using SmartProctor.Server.Data.Repositories;

namespace SmartProctor.Server.Services
{
    public interface IExamUserServices : IBaseServices<ExamUser>
    {
        IList<int> GetProctoredExamIds(string uid);
        IList<int> GetTakeExamIds(string uid);
        bool AddUserForExam(int eid, string uid, bool isProctor);
    }
    
    public class ExamUserServices : BaseServices<ExamUser>, IExamUserServices
    {
        public ExamUserServices(IExamUserRepository repo) : base(repo)
        {
        }

        public IList<int> GetProctoredExamIds(string uid)
        {
            var q = BaseRepository.GetObjectList(x => x.UserId == uid && x.UserRole == 2, x => x.ExamId,
                OrderingType.Descending);
            return (from x in q select x.ExamId).ToList();
        }
        
        public IList<int> GetTakeExamIds(string uid)
        {
            var q = BaseRepository.GetObjectList(x => x.UserId == uid && x.UserRole == 1, x => x.ExamId,
                OrderingType.Descending);
            return (from x in q select x.ExamId).ToList();
        }

        public bool AddUserForExam(int eid, string uid, bool isProctor)
        {
            var x = new ExamUser()
            {
                ExamId = eid,
                UserId = uid,
                UserRole = isProctor ? 2 : 1,
            };

            try
            {
                SaveObject(x);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}