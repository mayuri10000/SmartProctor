using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartProctor.Server.Data;
using SmartProctor.Server.Data.Repositories;
using SmartProctor.Shared;

namespace SmartProctor.Server.Services
{
    /// <summary>
    /// The base interface of all the service tier (business logic) methods.
    /// The methods in the interface simply invokes the underlying repository tier methods.
    /// </summary>
    /// <typeparam name="T">Entity type associated with the base methods</typeparam>
    public interface IBaseServices<T> where T : class,new()
    {
        IBaseRepository<T> BaseRepository { get; set; }
        bool IsInsert(T obj);
        T GetObject(params object[] keys);
        T GetObject(Expression<Func<T, bool>> where, string[] includes = null);
        T GetObject<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null);
        IList<T> GetObjectList<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null);
        int GetCount(Expression<Func<T, bool>> where, string[] includes = null);
        void TryDetectChange(T obj);
        void SaveObject(T obj);
        void DeleteObject(long id);
        void DeleteObject(Guid guid);
        void DeleteObject(T obj);
        void DeleteAll(IEnumerable<T> objects);
    }
    
    /// <summary>
    /// Implementation of <see cref="IBaseServices{T}"/>
    /// </summary>
    /// <typeparam name="T">Entity type associated with the base methods</typeparam>
    public class BaseServices<T> : IBaseServices<T> where T : class, new()
    {
        public IBaseRepository<T> BaseRepository { get; set; }

        public BaseServices(IBaseRepository<T> repo)
        {
            BaseRepository = repo;
        }

        public virtual bool IsInsert(T obj)
        {
            return BaseRepository.IsInsert(obj);
        }

        public virtual T GetObject(params object[] keys)
        {
            return BaseRepository.GetObjectByKeys(keys);
        }

        public T GetObject(Expression<Func<T, bool>> where, string[] includes = null)
        {
            return BaseRepository.GetFirstOrDefaultObject(where, includes);
        }

        public T GetObject<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null)
        {
            return BaseRepository.GetFirstOrDefaultObject(where, orderBy, orderingType, includes);
        }
        
        public virtual IList<T> GetObjectList<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null)
        {
            return BaseRepository.GetObjectList(where, orderBy, orderingType, includes);
        }

        public virtual int GetCount(Expression<Func<T, bool>> where, string[] includes = null)
        {
            return BaseRepository.ObjectCount(where, includes);
        }

        public virtual void TryDetectChange(T obj)
        {
            if (!IsInsert(obj))
            {
                BaseRepository.DbContext.Entry(obj).State = EntityState.Modified;
            }
        }

        public virtual void SaveObject(T obj)
        {
            BaseRepository.Save(obj);
        }

        public virtual void DeleteObject(long id)
        {
            T obj = this.GetObject(id, null);
            this.DeleteObject(obj);
        }

        public virtual void DeleteObject(Guid guid)
        {
            T obj = this.GetObject(guid, null);
            this.DeleteObject(obj);
        }

        public virtual void DeleteObject(T obj)
        {
            BaseRepository.Delete(obj);
        }

        public virtual void DeleteAll(IEnumerable<T> objects)
        {
            var list = objects.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                this.DeleteObject(list[i]);
            }
        }
    }
}