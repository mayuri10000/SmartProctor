using System;
using System.Collections.Generic;

namespace SmartProctor.Shared
{
    public class PagedList<T> : List<T> where T : class /*,new()*/
    {
        public PagedList(List<T> list, int pageIndex, int pageCount, int totalCount)
            : this(list, pageIndex, pageCount, totalCount, null)
        {
        }

        public PagedList(List<T> list, int pageIndex, int pageCount, int totalCount, int? skipCount = null)
        {
            this.AddRange(list);
            PageIndex = pageIndex;
            PageCount = pageCount;
            TotalCount = totalCount < 0 ? list.Count : totalCount;
            SkipCount = skipCount ?? GetSkipRecord(pageIndex, pageCount);
        }
        
        public static int GetSkipRecord(int pageIndex, int pageCount)
        {
            return (pageIndex - 1) * pageCount;
        }

        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
        public int SkipCount { get; set; }

        public int TotalPageNumber => Convert.ToInt32((TotalCount - 1) / PageCount) + 1;
    }
}