using SmartProctor.Server.Data.Entities;

namespace SmartProctor.Server.Data.Repositories
{
    public interface IExamRepository : IBaseRepository<Exam>
    {
    } 
    
    public class ExamRepository : BaseRepository<Exam>, IExamRepository
    {
        public ExamRepository(SmartProctorDbContext db) : base(db)
        {
        }
    }
}