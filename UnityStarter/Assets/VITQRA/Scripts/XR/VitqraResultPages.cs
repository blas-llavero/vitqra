using System;

namespace Vitqra
{
    // Page offsets remain aligned even when the last page is incomplete.
    public static class VitqraResultPages
    {
        public const int PageSize=6;
        public static int PageCount(int count)=>count<=0?0:1+(count-1)/PageSize;
        public static int ClampFirst(int first,int count)
        {
            if(count<=0)return 0;
            return Math.Min(Math.Max(0,first)/PageSize,PageCount(count)-1)*PageSize;
        }
        public static int PageNumber(int first,int count)=>count<=0?0:ClampFirst(first,count)/PageSize+1;
        public static int Move(int first,int count,int direction)
        {
            if(count<=0)return 0;
            int page=ClampFirst(first,count)/PageSize;
            return Math.Min(Math.Max(0,page+Math.Sign(direction)),PageCount(count)-1)*PageSize;
        }
    }
}
