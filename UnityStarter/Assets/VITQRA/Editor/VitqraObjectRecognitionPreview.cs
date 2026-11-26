using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using R=Vitqra.VitqraObjectRecognition;

namespace Vitqra
{
    public sealed class VitqraObjectRecognitionPreview:EditorWindow
    {
        private R exercise;
        private int shape,radius=6,dx,dy,noise;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Synthetic object recognition","Generated shapes only. No camera or medical identification.","Enable local exercise","Shape","Circle","Square","Triangle","Blank","Line","Ring","Pair","Size","Horizontal offset","Vertical offset","Pixel flip attempts","Recognize","Unknown","Similarity / margin","No result","Change inputs and run again"}},
            {"ca",new[]{"Reconeixement sintètic d'objectes","Només formes generades. Sense càmera ni identificació mèdica.","Activa la prova local","Forma","Cercle","Quadrat","Triangle","Buit","Línia","Anell","Parella","Mida","Desplaçament horitzontal","Desplaçament vertical","Intents d'alteració de píxels","Reconeix","Desconegut","Similitud / marge","Cap resultat","Canvia els paràmetres i torna a executar"}},
            {"es",new[]{"Reconocimiento sintético de objetos","Solo formas generadas. Sin cámara ni identificación médica.","Activar prueba local","Forma","Círculo","Cuadrado","Triángulo","Vacío","Línea","Anillo","Pareja","Tamaño","Desplazamiento horizontal","Desplazamiento vertical","Intentos de alterar píxeles","Reconocer","Desconocido","Similitud / margen","Sin resultado","Cambiar parámetros y ejecutar de nuevo"}},
            {"zh",new[]{"合成物体识别","仅生成形状。无相机或医疗识别。","启用本地练习","形状","圆形","正方形","三角形","空白","线条","圆环","一对","大小","水平偏移","垂直偏移","像素翻转次数","识别","未知","相似度 / 差值","无结果","修改参数并重新运行"}},
            {"hi",new[]{"कृत्रिम वस्तु पहचान","केवल निर्मित आकृतियाँ। कैमरा या चिकित्सा पहचान नहीं।","स्थानीय अभ्यास चालू करें","आकृति","वृत्त","वर्ग","त्रिभुज","खाली","रेखा","वलय","जोड़ी","आकार","क्षैतिज विस्थापन","ऊर्ध्व विस्थापन","पिक्सेल पलटने के प्रयास","पहचानें","अज्ञात","समानता / अंतर","कोई परिणाम नहीं","मान बदलें और फिर चलाएँ"}},
            {"fr",new[]{"Reconnaissance synthétique d’objets","Formes générées uniquement. Ni caméra ni identification médicale.","Activer le test local","Forme","Cercle","Carré","Triangle","Vide","Ligne","Anneau","Paire","Taille","Décalage horizontal","Décalage vertical","Tentatives d’inversion de pixels","Reconnaître","Inconnu","Similarité / écart","Aucun résultat","Modifier les paramètres et relancer"}},
            {"de",new[]{"Synthetische Objekterkennung","Nur erzeugte Formen. Keine Kamera oder medizinische Identifikation.","Lokalen Test aktivieren","Form","Kreis","Quadrat","Dreieck","Leer","Linie","Ring","Paar","Größe","Horizontaler Versatz","Vertikaler Versatz","Pixeländerungsversuche","Erkennen","Unbekannt","Ähnlichkeit / Abstand","Kein Ergebnis","Parameter ändern und erneut starten"}},
            {"pt",new[]{"Reconhecimento sintético de objetos","Apenas formas geradas. Sem câmara ou identificação médica.","Ativar teste local","Forma","Círculo","Quadrado","Triângulo","Vazio","Linha","Anel","Par","Tamanho","Deslocamento horizontal","Deslocamento vertical","Tentativas de inverter píxeis","Reconhecer","Desconhecido","Semelhança / margem","Sem resultado","Alterar parâmetros e executar novamente"}},
            {"ar",new[]{"تعرف اصطناعي على الأجسام","أشكال مولدة فقط. لا كاميرا أو تعريف طبي.","تفعيل الاختبار المحلي","الشكل","دائرة","مربع","مثلث","فارغ","خط","حلقة","زوج","الحجم","إزاحة أفقية","إزاحة عمودية","محاولات قلب البكسلات","تعرف","غير معروف","التشابه / الفرق","لا نتيجة","غيّر المعلمات وأعد التشغيل"}},
            {"ja",new[]{"合成物体認識","生成図形のみ。カメラや医療識別なし。","ローカルテストを有効化","形状","円","正方形","三角形","空白","線","輪","ペア","大きさ","水平オフセット","垂直オフセット","ピクセル反転試行回数","認識","不明","類似度 / 差","結果なし","パラメータを変更して再実行"}},
        };
        [MenuItem("VITQRA/Synthetic Object Recognition Preview")]
        private static void Open()=>GetWindow<VitqraObjectRecognitionPreview>();
        private void OnGUI()
        {
            string[] l;if(!Labels.TryGetValue(VitqraStrings.DeviceLanguage,out l))l=Labels["en"];
            titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            if(exercise==null)exercise=new R();
            exercise.SetEnabled(EditorGUILayout.Toggle(l[2],exercise.Enabled));
            if(!exercise.Enabled)return;
            EditorGUI.BeginChangeCheck();
            shape=EditorGUILayout.Popup(l[3],shape,new[]{l[4],l[5],l[6],l[7],l[8],l[9],l[10]});
            radius=EditorGUILayout.IntSlider(l[11],radius,5,8);
            dx=EditorGUILayout.IntSlider(l[12],dx,-3,3);dy=EditorGUILayout.IntSlider(l[13],dy,-3,3);
            noise=EditorGUILayout.IntSlider(l[14],noise,0,45);
            if(EditorGUI.EndChangeCheck()){exercise.SetEnabled(false);exercise.SetEnabled(true);}
            if(GUILayout.Button(l[15]))exercise.RunFixture((R.Shape)shape,radius,dx,dy,noise);
            var m=exercise.Current;var a=exercise.Preview();
            if(m==null||a==null){EditorGUILayout.LabelField(l[18]);return;}
            Rect rect=GUILayoutUtility.GetRect(160,200,GUILayout.ExpandWidth(true));
            float cell=Mathf.Min(rect.width,rect.height)/R.Size;
            float left=rect.center.x-cell*R.Size/2;
            for(int j=0;j<a.Length;j++)
                EditorGUI.DrawRect(new Rect(left+j%R.Size*cell,rect.y+j/R.Size*cell,cell,cell),
                    a[j]?Color.cyan:new Color(.08f,.13f,.19f));
            EditorGUILayout.LabelField(m.label==R.Label.Unknown?l[16]:l[4+(int)m.label],EditorStyles.boldLabel);
            EditorGUILayout.LabelField(l[17],(m.score/10f).ToString("F1",CultureInfo.InvariantCulture)+"% / "+
                (m.margin/10f).ToString("F1",CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField(l[19],EditorStyles.wordWrappedLabel);
        }
        private void OnDisable(){if(exercise!=null)exercise.SetEnabled(false);exercise=null;}
    }
}
