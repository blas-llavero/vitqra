"""Independent Python reproduction of the closed C# synthetic protocols."""
import hashlib
import json
from pathlib import Path

def snapshot(p, cutoff):
    if not 0 <= p < 3 or not 0 <= cutoff <= 180:
        return None
    rows = []
    for i in range(7):
        d = i*30
        rows += [(p,d,d+(12 if i==2 else 0),'A',None if i==3 and p==1 else 20+p*4+i*(p+1)*3,i!=4),
                 (p,d,d+(5 if i==1 else 0),'B',None if i==2 else 70-p*3-i*2,True)]
    return [r for r in rows if r[1]<=cutoff and r[2]<=cutoff and r[5]]

def score(rows):
    visible=sorted((day,v) for day,available,v in rows if day<=60 and available<=60 and v is not None)
    return visible[-1][1]-visible[0][1] if len(visible)>=2 else None

def outcome(event,followup):
    return True if event is not None and 60<event<=120 else None if followup<120 else False

def counts(examples, threshold):
    m=dict(TP=0,FP=0,TN=0,FN=0)
    for _,s,y in examples:
        positive=threshold is not None and s>=threshold
        m['TP' if y and positive else 'FN' if y else 'FP' if positive else 'TN']+=1
    return m

def rates(m):
    tp,fp,tn,fn=(m[k] for k in ('TP','FP','TN','FN'))
    def div(a,b): return a/b if b else None
    se,sp=div(tp,tp+fn),div(tn,tn+fp)
    return dict(**m,n=tp+fp+tn+fn,sensitivity=se,specificity=sp,ppv=div(tp,tp+fp),npv=div(tn,tn+fn),balanced_accuracy=(se+sp)/2 if se is not None and sp is not None else None)

def run():
    state=20261126
    def nextint(n):
        nonlocal state
        state=(state*1664525+1013904223)&0xffffffff
        return (state>>8)%n
    groups={'development':[],'holdout':[]}
    excluded={g:dict(assigned=0,censored=0,missing=0) for g in groups}
    canonical='synthetic-retrospective-v1\nseed=20261126\ncutoff=60\nhorizon=120\n'
    perturbations=0
    for i in range(120):
        latent=nextint(11)-5; start=40+nextint(11)-5
        mid=start+latent+nextint(7)-3; end=start+2*latent+nextint(9)-4
        delay=7 if nextint(4)==0 else 0
        missing_start=nextint(9)==0; missing_mid=nextint(7)==0; missing_end=nextint(8)==0
        has_event=nextint(100)<40+latent*6
        event_day=75+nextint(46); followup=90 if nextint(9)==0 else 120
        event=event_day if has_event and event_day<=followup else None
        rows=[(0,0,None if missing_start else start),(30,30,None if missing_mid else mid),(60,60+delay,None if missing_end else end),(100,100,90 if has_event else 10)]
        group='holdout' if i%3==0 else 'development'; excluded[group]['assigned']+=1
        canonical+=f'{i}|{group}|{followup}|{event if event is not None else "none"}\n'
        for day,avail,val in rows: canonical+=f'{day}|{avail}|{val if val is not None else "missing"}\n'
        s=score(rows); y=outcome(event,followup)
        altered=[(d,a,99999) if d>60 or a>60 else (d,a,v) for d,a,v in rows]
        assert score(altered)==s; perturbations+=1
        if y is None: excluded[group]['censored']+=1
        elif s is None: excluded[group]['missing']+=1
        else: groups[group].append((i,s,y))
    train,test=groups.values()
    assert not ({r[0] for r in train}&{r[0] for r in test})
    p=sum(r[2] for r in train); n=len(train)-p
    assert p and n
    def utility(t):
        m=counts(train,t); return m['TP']*n+m['TN']*p
    threshold=max(range(-20,22),key=lambda t:(utility(t),t))
    for g,rows in groups.items(): assert len(rows)+excluded[g]['censored']+excluded[g]['missing']==excluded[g]['assigned']
    for p in range(3):
        for d in range(181): assert all(r[0]==p and r[1]<=d and r[2]<=d and r[5] for r in snapshot(p,d))
    assert len(snapshot(0,30))==3 and len(snapshot(0,35))==4
    assert len(snapshot(1,180))==13 and any(r[4] is None for r in snapshot(1,180))
    assert outcome(None,90) is None and score([(0,0,None),(30,30,1)]) is None
    assert snapshot(-1,0) is None and snapshot(0,181) is None
    return dict(scope='synthetic only; no clinical validation',cutoff_checks=543,future_perturbation_checks=perturbations,threshold=threshold,split=excluded,development=rates(counts(train,threshold)),holdout=rates(counts(test,threshold)),always_negative_baseline=rates(counts(test,None)),cohort_sha256=hashlib.sha256(canonical.encode()).hexdigest())

if __name__=='__main__':
    result=run(); assert result==run()
    print(json.dumps(result,indent=2))
