using SmartProctor.Server.Data.Entities;

namespace SmartProctor.Server.Data.Repositories
{
    public interface IQuestionRepository : IBaseRepository<Question>
    {
    }
    
    public class QuestionRepository : BaseRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(SmartProctorDbContext db) : base(db)
        {
        }
    }
}