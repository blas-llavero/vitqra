using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using D=Vitqra.VitqraCommunityPolicy;

namespace Vitqra
{
    // Fixed fictional aggregate release demonstration. No patient rows, therapies,
    // real clinical endpoints, real identities, arbitrary filters, imports or network.
    // The policy and suppression here are NOT a production privacy boundary.
    public static class VitqraAggregateCollaboration
    {
        public const string Protocol="synthetic-aggregate-v1";
        public const int MinimumGroup=20, MinimumCell=5;
        public enum Group { A,B,C,D }
        public enum Endpoint { BenefitProxy,HarmProxy,FunctionProxy }
        public enum State { Denied, Visible, Withheld }
        private sealed class Fixture
        {
            public readonly Group group;
            public readonly int n,benefit,harm,function;
            public Fixture(Group g,int n,int b,int h,int f)
            {group=g;this.n=n;benefit=b;harm=h;function=f;}
        }
        // Predefined disjoint fictional groups, one fixed snapshot, no grand total.
        // These counts are invented, not measured outcomes.
        private static readonly Fixture[] Fixtures={
            new Fixture(Group.A,60,36,18,30),
            new Fixture(Group.B,60,34,10,33),
            new Fixture(Group.C,60,39,2,36),
            new Fixture(Group.D,12,7,5,6)
        };
        public sealed class Cell
        {
            public readonly Endpoint endpoint;
            public readonly int count;
            internal Cell(Endpoint e,int c){endpoint=e;count=c;}
        }
        public sealed class Row
        {
            public readonly Group group;
            public readonly State state;
            public readonly int? denominator;
            public readonly ReadOnlyCollection<Cell> cells;
            internal Row(Group g,State s,int? n,Cell[] cells)
            {group=g;state=s;denominator=n;this.cells=Array.AsReadOnly(cells);}
        }
        public sealed class Result
        {
            public readonly State state;
            public readonly ReadOnlyCollection<Row> rows;
            public readonly string digest;
            internal Result(State s,Row[] r,string d){state=s;rows=Array.AsReadOnly(r);digest=d;}
        }
        private static string N(int n)=>n.ToString(CultureInfo.InvariantCulture);
        private static bool Safe(Fixture f)
        {
            if(f.n<MinimumGroup)return false;
            foreach(int c in new[]{f.benefit,f.harm,f.function})
                if(c<MinimumCell||c>f.n-MinimumCell)return false;
            return true;
        }
        // All-or-nothing suppression per group, including denominator and other
        // endpoints, even if only one binary cell or its complement is small.
        private static Row Publish(Fixture f)
        {
            if(!Safe(f))return new Row(f.group,State.Withheld,null,new Cell[0]);
            return new Row(f.group,State.Visible,f.n,new[]{
                new Cell(Endpoint.BenefitProxy,f.benefit),new Cell(Endpoint.HarmProxy,f.harm),
                new Cell(Endpoint.FunctionProxy,f.function)});
        }
        private static Result Denied()=>new Result(State.Denied,new Row[0],null);
        public static Result Open(D.Actor actor,D.Resource resource,D.Purpose purpose,int day)
        {
            if(resource!=D.Resource.AggregateA||
                !((actor==D.Actor.Researcher&&purpose==D.Purpose.Research)||
                  (actor==D.Actor.Industry&&purpose==D.Purpose.Collaboration)))return Denied();
            var decision=D.Evaluate(actor,resource,purpose,D.Action.Read,day);
            if(decision.outcome!=D.Outcome.AllowSimulation)return Denied();
            var rows=Fixtures.Select(Publish).ToArray();
            // Digest covers only the released representation, never hidden counts.
            var canonical=new StringBuilder(Protocol+"\nAggregateA\n");
            foreach(var r in rows)
            {
                canonical.Append(r.group).Append('|').Append(r.state);
                if(r.denominator.HasValue)canonical.Append('|').Append(N(r.denominator.Value));
                foreach(var c in r.cells)canonical.Append('|').Append(c.endpoint).Append('=').Append(N(c.count));
                canonical.Append('\n');
            }
            return new Result(State.Visible,rows,VitqraLongitudinal.Hash(canonical.ToString()));
        }
        public static bool ClinicalRelease=>false;
        public static bool ExportEnabled=>false;
        public static bool NetworkEnabled=>false;
        // Pure synthetic boundary fixture, no arbitrary query against stored data.
        public static bool BoundaryChecks()
        {
            var cases=new[]{new Fixture(Group.A,19,9,9,9),new Fixture(Group.A,20,4,10,10),
                new Fixture(Group.A,20,16,10,10),new Fixture(Group.A,20,5,15,10),
                new Fixture(Group.A,20,-1,10,10),new Fixture(Group.A,20,21,10,10),
                new Fixture(Group.A,20,0,10,10),new Fixture(Group.A,20,20,10,10)};
            for(int i=0;i<cases.Length;i++)
            {
                var r=Publish(cases[i]);bool visible=i==3;
                if((r.state==State.Visible)!=visible)return false;
                if(!visible&&(r.denominator.HasValue||r.cells.Count!=0))return false;
            }
            return true;
        }
    }
}
