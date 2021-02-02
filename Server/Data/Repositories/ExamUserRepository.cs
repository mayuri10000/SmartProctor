using SmartProctor.Server.Data.Entities;

namespace SmartProctor.Server.Data.Repositories
{
    public interface IExamUserRepository : IBaseRepository<ExamUser>
    {
    }
    
    public class ExamUserRepository : BaseRepository<ExamUser>, IExamUserRepository
    {
        public ExamUserRepository(SmartProctorDbContext db) : base(db)
        {
        }
    }
}