using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraAggregateCollaboration;

namespace Vitqra
{
    public sealed class VitqraAggregateCollaborationPreview:EditorWindow
    {
        private D.Actor actor=D.Actor.Industry;
        private D.Resource resource=D.Resource.AggregateA;
        private D.Purpose purpose=D.Purpose.Collaboration;
        private int day=25;
        private Vector2 scroll;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Synthetic aggregate collaboration","Invented counts only. No treatment evidence or real patients.","Actor","Resource","Purpose","Access day","Access denied","Withheld","Group","Benefit proxy","Harm proxy","Function proxy","Released snapshot digest"}},
            {"ca",new[]{"Col·laboració agregada sintètica","Només recomptes inventats. Sense evidència terapèutica ni pacients reals.","Participant","Recurs","Finalitat","Dia d'accés","Accés denegat","No divulgat","Grup","Indicador de benefici","Indicador de dany","Indicador funcional","Empremta de la versió divulgada"}},
            {"es",new[]{"Colaboración agregada sintética","Solo recuentos inventados. Sin evidencia terapéutica ni pacientes reales.","Participante","Recurso","Finalidad","Día de acceso","Acceso denegado","No divulgado","Grupo","Indicador de beneficio","Indicador de daño","Indicador funcional","Huella de la versión divulgada"}},
            {"zh",new[]{"合成汇总协作","仅虚构计数。无治疗证据或真实患者。","参与者","资源","用途","访问日","拒绝访问","不披露","组","获益代理","伤害代理","功能代理","发布快照摘要"}},
            {"hi",new[]{"कृत्रिम समेकित सहयोग","केवल काल्पनिक गणना। उपचार प्रमाण या वास्तविक रोगी नहीं।","भागीदार","संसाधन","उद्देश्य","पहुँच दिन","पहुँच अस्वीकृत","प्रकाशित नहीं","समूह","लाभ संकेतक","हानि संकेतक","कार्य संकेतक","जारी संस्करण का हैश"}},
            {"fr",new[]{"Collaboration agrégée synthétique","Comptages fictifs uniquement. Ni preuve thérapeutique ni patients réels.","Participant","Ressource","Finalité","Jour d’accès","Accès refusé","Non divulgué","Groupe","Indicateur de bénéfice","Indicateur de dommage","Indicateur fonctionnel","Empreinte de la version divulguée"}},
            {"de",new[]{"Synthetische aggregierte Zusammenarbeit","Nur fiktive Zahlen. Keine Therapieevidenz oder echten Patienten.","Teilnehmer","Ressource","Zweck","Zugriffstag","Zugriff verweigert","Nicht offengelegt","Gruppe","Nutzenindikator","Schadensindikator","Funktionsindikator","Hash der freigegebenen Version"}},
            {"pt",new[]{"Colaboração agregada sintética","Apenas contagens fictícias. Sem evidência terapêutica ou pacientes reais.","Participante","Recurso","Finalidade","Dia de acesso","Acesso negado","Não divulgado","Grupo","Indicador de benefício","Indicador de dano","Indicador funcional","Hash da versão divulgada"}},
            {"ar",new[]{"تعاون تجميعي اصطناعي","أعداد خيالية فقط. لا دليل علاجي أو مرضى حقيقيون.","المشارك","المورد","الغرض","يوم الوصول","رفض الوصول","غير مفصح عنه","المجموعة","مؤشر الفائدة","مؤشر الضرر","مؤشر الوظيفة","بصمة النسخة المنشورة"}},
            {"ja",new[]{"合成集計コラボレーション","架空の件数のみ。治療の根拠や実患者なし。","参加者","リソース","目的","アクセス日","アクセス拒否","非開示","グループ","利益指標","害の指標","機能指標","開示版のハッシュ"}},
        };
        [MenuItem("VITQRA/Synthetic Aggregate Collaboration Preview")]
        private static void Open()=>GetWindow<VitqraAggregateCollaborationPreview>();
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            actor=(D.Actor)EditorGUILayout.EnumPopup(l[2],actor);
            resource=(D.Resource)EditorGUILayout.EnumPopup(l[3],resource);
            purpose=(D.Purpose)EditorGUILayout.EnumPopup(l[4],purpose);
            day=EditorGUILayout.IntSlider(l[5],day,0,180);
            var result=M.Open(actor,resource,purpose,day);
            if(result.state==M.State.Denied){EditorGUILayout.HelpBox(l[6],MessageType.Info);return;}
            scroll=EditorGUILayout.BeginScrollView(scroll);
            foreach(var row in result.rows)
            {
                EditorGUILayout.LabelField(l[8]+" "+row.group,EditorStyles.boldLabel);
                if(row.state==M.State.Withheld){EditorGUILayout.LabelField(l[7]);continue;}
                foreach(var cell in row.cells)
                {
                    double rate=(double)cell.count/row.denominator.Value;
                    EditorGUILayout.LabelField(l[9+(int)cell.endpoint],cell.count+" / "+row.denominator.Value+" ("+rate.ToString("P1",CultureInfo.InvariantCulture)+")");
                    Rect rect=GUILayoutUtility.GetRect(80,10,GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(rect,new Color(.12f,.18f,.22f));
                    EditorGUI.DrawRect(new Rect(rect.x,rect.y,rect.width*(float)rate,rect.height),Color.cyan);
                }
            }
            EditorGUILayout.LabelField(l[12]);
            EditorGUILayout.SelectableLabel(result.digest,EditorStyles.textField,GUILayout.Height(36));
            EditorGUILayout.EndScrollView();
        }
    }
}
