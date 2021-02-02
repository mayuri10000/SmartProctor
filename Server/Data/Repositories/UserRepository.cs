using SmartProctor.Server.Data.Entities;

namespace SmartProctor.Server.Data.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
    }
    
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(SmartProctorDbContext db) : base(db)
        {
        }
    }
}