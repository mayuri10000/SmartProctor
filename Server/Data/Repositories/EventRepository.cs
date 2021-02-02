using SmartProctor.Server.Data.Entities;

namespace SmartProctor.Server.Data.Repositories
{
    public interface IEventRepository : IBaseRepository<Event>
    {
    }
    
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        public EventRepository(SmartProctorDbContext db) : base(db)
        {
        }
    }
}