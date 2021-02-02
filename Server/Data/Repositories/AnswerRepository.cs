using SmartProctor.Server.Data.Entities;

namespace SmartProctor.Server.Data.Repositories
{
    public interface IAnswerRepository : IBaseRepository<Answer>
    {
    }
    
    public class AnswerRepository : BaseRepository<Answer>, IAnswerRepository
    {
        public AnswerRepository(SmartProctorDbContext db) : base(db)
        {
        }
    }
}