using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using D=Vitqra.VitqraCommunityPolicy;

namespace Vitqra
{
    // Closed fictional previsit fixture. Not an EHR, clinical assessment,
    // real authentication, medical image reader or patient intake system.
    public static class VitqraPrevisit
    {
        public const string Protocol="synthetic-previsit-v1";
        public enum Kind { NumericA, NumericB, SymptomMarker, ImagePlaceholder, VisitQuestion }
        public enum Provenance { FictionalSelfReport, FictionalDevice, FictionalDocument }
        public enum Status { Available, ExplicitlyMissing, PlaceholderOnly }
        public sealed class Observation
        {
            public readonly int id,observed,available;
            public readonly Kind kind;
            public readonly Provenance source;
            public readonly Status status;
            public readonly int? value;
            internal Observation(int id,int observed,int available,Kind kind,Provenance source,Status status,int? value)
            {this.id=id;this.observed=observed;this.available=available;this.kind=kind;this.source=source;this.status=status;this.value=value;}
        }
        public sealed class Summary
        {
            public readonly D.Resource resource;
            public readonly int cutoff,visitDay,knownMissing,placeholders;
            public readonly int? changeA,changeB;
            public readonly ReadOnlyCollection<Observation> observations;
            public readonly string digest;
            internal Summary(D.Resource resource,int cutoff,int visitDay,Observation[] rows)
            {
                this.resource=resource;this.cutoff=cutoff;this.visitDay=visitDay;
                observations=Array.AsReadOnly(rows);
                knownMissing=rows.Count(r=>r.status==Status.ExplicitlyMissing);
                placeholders=rows.Count(r=>r.status==Status.PlaceholderOnly);
                changeA=Change(rows,Kind.NumericA);changeB=Change(rows,Kind.NumericB);
                var b=new StringBuilder(Protocol+"\n"+resource+"|"+N(cutoff)+"|"+N(visitDay)+"\n");
                foreach(var r in rows)b.Append(N(r.id)).Append('|').Append(N(r.observed)).Append('|').Append(N(r.available))
                    .Append('|').Append(r.kind).Append('|').Append(r.source).Append('|').Append(r.status)
                    .Append('|').Append(r.value.HasValue?N(r.value.Value):"none").Append('\n');
                digest=VitqraLongitudinal.Hash(b.ToString());
            }
        }
        public sealed class Result
        {
            public readonly Summary summary;
            public readonly D.Reason reason;
            internal Result(Summary s,D.Reason r){summary=s;reason=r;}
        }
        private static string N(int n)=>n.ToString(CultureInfo.InvariantCulture);
        private static int? Change(IEnumerable<Observation> rows,Kind k)
        {
            var v=rows.Where(r=>r.kind==k&&r.status==Status.Available&&r.value.HasValue).ToArray();
            return v.Length<2?(int?)null:v[v.Length-1].value.Value-v[0].value.Value;
        }
        private static IEnumerable<Observation> Generate(D.Resource resource)
        {
            int shift=resource==D.Resource.RecordB?10:0;
            yield return new Observation(1,5,5,Kind.NumericA,Provenance.FictionalDevice,Status.Available,20+shift);
            yield return new Observation(2,5,5,Kind.NumericB,Provenance.FictionalDevice,Status.Available,70-shift);
            yield return new Observation(3,10,10,Kind.SymptomMarker,Provenance.FictionalSelfReport,Status.Available,2);
            yield return new Observation(4,15,15,Kind.NumericA,Provenance.FictionalDevice,Status.Available,24+shift);
            yield return new Observation(5,15,18,Kind.NumericB,Provenance.FictionalDevice,Status.ExplicitlyMissing,null);
            yield return new Observation(6,20,20,Kind.ImagePlaceholder,Provenance.FictionalDocument,Status.PlaceholderOnly,null);
            yield return new Observation(7,20,20,Kind.VisitQuestion,Provenance.FictionalSelfReport,Status.PlaceholderOnly,null);
            yield return new Observation(8,23,28,Kind.NumericA,Provenance.FictionalDevice,Status.Available,27+shift);
            yield return new Observation(9,24,24,Kind.NumericB,Provenance.FictionalDevice,Status.Available,66-shift);
            yield return new Observation(10,30,30,Kind.NumericA,Provenance.FictionalDevice,Status.Available,99);
        }
        // Check permission at current access day, not historical cutoff.
        // A separate cutoff determines eligible data. No historical grant replay.
        public static Result Open(D.Actor actor,D.Resource resource,int accessDay,int cutoff)
        {
            if(resource!=D.Resource.RecordA&&resource!=D.Resource.RecordB)return new Result(null,D.Reason.Invalid);
            const int visitDay=30;
            if(accessDay<0||accessDay>180||cutoff<0||cutoff>accessDay||cutoff>=visitDay)
                return new Result(null,D.Reason.Invalid);
            var decision=D.Evaluate(actor,resource,D.Purpose.Care,D.Action.Read,accessDay);
            if(decision.outcome!=D.Outcome.AllowSimulation)return new Result(null,decision.reason);
            var rows=Generate(resource).Where(r=>r.observed<=cutoff&&r.available<=cutoff)
                .OrderBy(r=>r.observed).ThenBy(r=>r.id).ToArray();
            return new Result(new Summary(resource,cutoff,visitDay,rows),D.Reason.Granted);
        }
        public static bool ClinicalRelease=>false;
        public static bool ExportEnabled=>false;
    }
}
