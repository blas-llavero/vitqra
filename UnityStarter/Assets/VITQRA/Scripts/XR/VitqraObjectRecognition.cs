using System;
using System.Collections.Generic;

namespace Vitqra
{
    // Closed local geometry exercise. No input from camera, files or sensors,
    // no actual object identity, model downloads, clinical use or network.
    public sealed class VitqraObjectRecognition
    {
        public const int Size=24, MinimumScore=750, MinimumMargin=80;
        public enum Shape { Circle, Square, Triangle, Blank, Line, Ring, Pair }
        public enum Label { Circle, Square, Triangle, Unknown }
        public sealed class Match
        {
            public readonly Label label;
            public readonly int score,margin;
            internal Match(Label label,int score,int margin){this.label=label;this.score=score;this.margin=margin;}
        }
        private static readonly bool[][][] Templates=BuildTemplates();
        private bool enabled;
        private bool[] current;
        private Match match;
        public bool Enabled=>enabled;
        public void SetEnabled(bool value)
        {
            if(enabled!=value){current=null;match=null;}
            enabled=value;
        }
        public Match Current=>enabled?match:null;
        public bool[] Preview()=>enabled&&current!=null?(bool[])current.Clone():null;
        private static bool[] Raster(Shape shape,int radius,int dx,int dy)
        {
            var a=new bool[Size*Size];
            for(int j=0;j<a.Length;j++)
            {
                int x=j%Size-12-dx,y=j/Size-12-dy,d=x*x+y*y;
                switch(shape)
                {
                    case Shape.Circle:a[j]=d<=radius*radius;break;
                    case Shape.Square:a[j]=Math.Abs(x)<=radius&&Math.Abs(y)<=radius;break;
                    case Shape.Triangle:a[j]=y>=-radius&&y<=radius&&Math.Abs(x)<=(y+radius)/2;break;
                    case Shape.Blank:a[j]=false;break;
                    case Shape.Line:a[j]=Math.Abs(y)<=1&&Math.Abs(x)<=9;break;
                    case Shape.Ring:a[j]=d<=64&&d>=36;break;
                    case Shape.Pair:a[j]=(x-6)*(x-6)+y*y<=9||(x+6)*(x+6)+y*y<=9;break;
                }
            }
            return a;
        }
        private static bool[][][] BuildTemplates()
        {
            var all=new bool[3][][];
            for(int k=0;k<3;k++)
            {
                var list=new List<bool[]>();
                for(int r=5;r<=8;r++)for(int x=-3;x<=3;x++)for(int y=-3;y<=3;y++)
                    list.Add(Raster((Shape)k,r,x,y));
                all[k]=list.ToArray();
            }
            return all;
        }
        private static Match Classify(bool[] a)
        {
            int best=-1,second=-1,bestLabel=-1;
            for(int k=0;k<3;k++)
            {
                int score=0;
                foreach(var t in Templates[k])
                {
                    int i=0,u=0;
                    for(int j=0;j<a.Length;j++){if(a[j]&&t[j])i++;if(a[j]||t[j])u++;}
                    score=Math.Max(score,u==0?0:1000*i/u);
                }
                if(score>best){second=best;best=score;bestLabel=k;}
                else if(score>second)second=score;
            }
            return new Match(best>=MinimumScore&&best-second>=MinimumMargin?(Label)bestLabel:Label.Unknown,best,best-second);
        }
        // Explicit synthetic fixture only. Reject invalid parameters and clear old result.
        // Seeded flips can revisit pixels; number of attempts is not a unique pixel count.
        public Match RunFixture(Shape shape,int radius,int dx,int dy,int flipAttempts)
        {
            current=null;match=null;
            if(!enabled||!Enum.IsDefined(typeof(Shape),shape)||radius<5||radius>8||
                dx< -3||dx>3||dy< -3||dy>3||flipAttempts<0||flipAttempts>45)return null;
            current=Raster(shape,radius,dx,dy);
            uint seed=(uint)(((int)shape+1)*987+radius*33+dx*11+dy);
            for(int j=0;j<flipAttempts;j++)
            {
                seed=unchecked(seed*1664525u+1013904223u);
                int p=(int)(seed%(uint)current.Length);current[p]=!current[p];
            }
            match=Classify(current);
            return match;
        }
        public bool CameraEnabled=>false;
        public bool NetworkEnabled=>false;
        public bool ClinicalRelease=>false;
    }
}
