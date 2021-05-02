using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartProctor.Server.Data.Entities;
using SmartProctor.Shared;

namespace SmartProctor.Server.Data.Repositories
{
    /// <summary>
    /// The base interface of all the repository tier classes, offers basic
    /// database operations.
    /// </summary>
    /// <typeparam name="T">Entity type, should be one of the entities of the database context</typeparam>
    public interface IBaseRepository<T> where T : class, new()
    {
        /// <summary>
        /// The database context
        /// </summary>
        SmartProctorDbContext DbContext { get; set; }
        
        /// <summary>
        /// Return whether the entity object should be inserted (i.e. not currently
        /// present in the database)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool IsInsert(T obj);
        
        /// <summary>
        /// Obtains the entity object from the database with a set of primary keys
        /// </summary>
        /// <param name="keys">The primary keys of the objects</param>
        /// <returns></returns>
        T GetObjectByKeys(params object[] keys);

        /// <summary>
        /// Gets a list of objects from the database table that meets a specific rule. The result will be ordered
        /// </summary>
        /// <param name="where">The expression of the rule that the results should meet</param>
        /// <param name="orderBy">The expression returns the field used for sorting the results</param>
        /// <param name="orderingType">The sorting type of the result, ascending or descending</param>
        /// <param name="includes"></param>
        /// <typeparam name="TK">The type of the field used for sorting, not needed to manually specify</typeparam>
        /// <returns></returns>
        IList<T> GetObjectList<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy,
            OrderingType orderingType, string[] includes = null);

        /// <summary>
        /// Gets the first object in the database table that meets a specific rule, if there's no object matches,
        /// null will be returned.
        /// </summary>
        /// <param name="where">The expression of the rule that the results should meet</param>
        /// <param name="includes"></param>
        /// <returns>The first object matches the rule or null if no object matches</returns>
        T GetFirstOrDefaultObject(Expression<Func<T, bool>> where, string[] includes = null);

        /// <summary>
        /// Gets the first object in the database table that meets a specific rule, if there's no object matches,
        /// null will be returned. The results will be first ordered then the first object in the results will be returned.
        /// </summary>
        /// <param name="where">The expression of the rule that the results should meet</param>
        /// <param name="orderBy">The expression returns the field used for sorting the results</param>
        /// <param name="orderingType">The sorting type of the result, ascending or descending</param>
        /// <param name="includes"></param>
        /// <typeparam name="TK">The type of the field used for sorting, not needed to manually specify</typeparam>
        /// <returns></returns>
        T GetFirstOrDefaultObject<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy,
            OrderingType orderingType, string[] includes = null);

        /// <summary>
        /// Gets the count of objects in the database table that meets a specific rule.
        /// </summary>
        /// <param name="where">The expression of the rule that the results should meet</param>
        /// <param name="includes"></param>
        /// <returns></returns>
        int ObjectCount(Expression<Func<T, bool>> where, string[] includes = null);

        /// <summary>
        /// Adds a new object into the database table
        /// </summary>
        /// <param name="obj"></param>
        void Add(T obj);
        
        /// <summary>
        /// Updates a existing object in the database table
        /// </summary>
        /// <param name="obj"></param>
        void Update(T obj);
        
        /// <summary>
        /// Saves a object into the database table, if the object not exists in the database,
        /// it will be added, otherwise it will be updated.
        /// </summary>
        /// <param name="obj"></param>
        void Save(T obj);

        /// <summary>
        /// Deletes a object in the database table
        /// </summary>
        /// <param name="obj">The expression of the rule that the deleted objects should meet</param>
        void Delete(T obj);

        /// <summary>
        /// Delete all objects in the database table that meets a specific rule,
        /// </summary>
        /// <param name="where"></param>
        void Delete(Expression<Func<T, bool>> where);
        
        /// <summary>
        /// Saves the changes made in the database, will be automatically called by the server tier.
        /// </summary>
        void SaveChanges();
    }

    /// <summary>
    /// Implementation of interface <see cref="IBaseRepository{T}"/>
    /// </summary>
    /// <typeparam name="T"> Entity type, should be one of the entities of the database context</typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {
        // public BaseRepository() : this(null)
        // {
        // }

        /// <summary>
        /// Constructor, the database context will be passed with dependency injection
        /// </summary>
        /// <param name="db">The database context</param>
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

        public virtual int ObjectCount(Expression<Func<T, bool>> where, string[] includes = null)
        {
            int count = 0;
            var query = DbContext.Set<T>()
                .Includes(includes);

            count = query.Count(where);

            return count;
        }


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
