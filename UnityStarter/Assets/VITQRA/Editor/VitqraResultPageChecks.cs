using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraResultPageChecks
    {
        [MenuItem("VITQRA/Run Result Page Checks")]
        public static void Run()
        {
            Check(VitqraResultPages.PageCount(0)==0,"Empty");
            Check(VitqraResultPages.PageCount(6)==1,"Full single page");
            Check(VitqraResultPages.PageCount(7)==2,"Partial last page");
            Check(VitqraResultPages.Move(0,12,1)==6,"Next page");
            Check(VitqraResultPages.Move(6,12,1)==6,"Last page boundary");
            Check(VitqraResultPages.Move(6,13,1)==12,"Third page with one item");
            Check(VitqraResultPages.Move(12,13,-1)==6,"Previous page");
            Check(VitqraResultPages.Move(0,13,-1)==0,"First page boundary");
            Check(VitqraResultPages.ClampFirst(12,7)==6,"Shrinking results");
            Check(VitqraResultPages.ClampFirst(6,20)==6,"Incoming results preserve page");
            Check(VitqraResultPages.PageNumber(6,12)==2,"Page indicator");
            Check(VitqraResultPages.PageNumber(0,0)==0,"Empty indicator");
            Debug.Log("VITQRA result page checks passed.");
        }
        private static void Check(bool condition,string name)
        {if(!condition)throw new InvalidOperationException(name);}
    }
}
