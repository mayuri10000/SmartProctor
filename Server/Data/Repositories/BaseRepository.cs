using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartProctor.Server.Data.Entities;
using SmartProctor.Shared;

namespace SmartProctor.Server.Data.Repositories
{
    public interface IBaseRepository<T> where T : class, new()
    {
        SmartProctorDbContext DbContext { get; set; }
        bool IsInsert(T obj);
        T GetObjectByKeys(params object[] keys);

        IList<T> GetObjectList<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy,
            OrderingType orderingType, string[] includes = null);

        T GetFirstOrDefaultObject(Expression<Func<T, bool>> where, string[] includes = null);

        T GetFirstOrDefaultObject<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy,
            OrderingType orderingType, string[] includes = null);

        int ObjectCount(Expression<Func<T, bool>> where, string[] includes = null);

        decimal GetSum(Expression<Func<T, bool>> where, Func<T, decimal> sum, string[] includes = null);

        void Add(T obj);
        void Update(T obj);
        void Save(T obj);

        void Delete(T obj);

        void Delete(Expression<Func<T, bool>> where);
        void SaveChanges();
    }

    public class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {
        // public BaseRepository() : this(null)
        // {
        // }

        public BaseRepository(SmartProctorDbContext db)
        {
            DbContext = db;
        }

        //public BaseRepository() { }
        public SmartProctorDbContext DbContext { get; set; }

        public virtual bool IsInsert(T obj)
        {
            var entry = DbContext.Entry(obj);
            return entry.State == EntityState.Added || entry.State == EntityState.Detached; 
            //return obj.EntityKey == null || obj.EntityKey.EntityKeyValues == null;
        }

        public virtual IList<T> GetObjectList<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy,
            OrderingType orderingType, string[] includes = null)
        {
            List<T> result = null;
            IQueryable<T> resultList = DbContext.Set<T>()
                .Includes(includes)
                .Where(where)
                .OrderBy(orderBy, orderingType); //.Includes(includes);
            result = resultList.ToList();
            
            return result;
        }

        public virtual T GetFirstOrDefaultObject(Expression<Func<T, bool>> where, string[] includes = null)
        {
            return DbContext.Set<T>()
                //.CreateQuery<T>(sql)
                .Includes(includes).FirstOrDefault(where);
        }

        public virtual T GetFirstOrDefaultObject<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy,
            OrderingType orderingType, string[] includes = null)
        {
            return DbContext.Set<T>()
                .Includes(includes).Where(where).OrderBy(orderBy, orderingType).FirstOrDefault();
        }

        public virtual T GetObjectByKeys(params object[] keys)
        {
            T obj = DbContext.Set<T>().Find(keys);
            return obj;
        }

        //public virtual T GetObjectByGuid(Guid guid, string[] includes = null)
        //{
        //    string sql = string.Format("SELECT VALUE c FROM {0} AS c ", _entitySetName);
        //    T obj = BaseDB.BaseDataContext
        //         .Set<T>()
        //        //.CreateQuery<T>(sql)
        //         .Includes(includes).Where("it.Guid = @guid", new ObjectParameter("guid", guid)).FirstOrDefault();
        //    return obj;
        //}

        public virtual int ObjectCount(Expression<Func<T, bool>> where, string[] includes = null)
        {
            int count = 0;
            var query = DbContext.Set<T>()
                .Includes(includes);
            
            count = query.Count(where);
            
            return count;
        }

        public virtual decimal GetSum(Expression<Func<T, bool>> where, Func<T, decimal> sum, string[] includes = null)
        {
            var result = DbContext.Set<T>()
                .Includes(includes).Where(where).Sum(sum);
            return result;
        }

        //public virtual object ObjectSum(Expression<Func<T, bool>> where, Func<T,object> sumBy, string[] includes)
        //{
        //    string sql = string.Format("SELECT VALUE c FROM {0} AS c ", _entitySetName);
        //    object result= DbContext.CreateQuery<T>(sql).Includes(includes).Where(where).Sum(sumBy);
        //    return result;
        //}


        public virtual void Add(T obj)
        {
            DbContext.Set<T>().Add(obj);
            SaveChanges();
        }

        public virtual void Update(T obj)
        {
            // DbContext.ApplyPropertyChanges(_entitySetName, obj);
            SaveChanges();
        }

        public virtual void Save(T obj)
        {
            if (IsInsert(obj))
            {
                Add(obj);
            }
            else
            {
                Update(obj);
            }
        }

        public virtual void SaveChanges()
        {
            DbContext.SaveChanges(); //TODO: SaveOptions.
        }

        public virtual void Delete(T obj)
        {
            DbContext.Set<T>().Remove(obj);
            SaveChanges();
        }

        public void Delete(Expression<Func<T, bool>> where)
        {
            var q = DbContext.Set<T>().Where(where);
            DbContext.Set<T>().RemoveRange(q);
            SaveChanges();
        }
    }
}
