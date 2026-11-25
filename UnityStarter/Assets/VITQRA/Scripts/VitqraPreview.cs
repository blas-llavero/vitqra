using System;
using UnityEngine;

namespace Vitqra
{
    public sealed class VitqraPreview : MonoBehaviour
    {
        private VitqraSettings settings;
        private string query = "", note = "";
        private bool searched;
        private Vector2 scroll;
        private const string NoteKey = "vitqra.preview.note";
        private readonly string[] providers = { "local", "OpenAI", "Anthropic", "Gemini", "Mistral", "DeepSeek" };
        private void Awake() { settings = VitqraSettings.Load(); note = PlayerPrefs.GetString(NoteKey, ""); }
        private void OnGUI()
        {
            if (settings == null) return;
            var l = VitqraStrings.Get(settings.language);
            var previous = GUI.color;
            GUI.color = new Color(.4f, 1f, .95f);
            GUILayout.BeginArea(new Rect(12,12,Mathf.Max(100,Screen.width-24),Mathf.Max(100,Screen.height-24)), GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("VITQRA — Science without barriers", GUI.skin.box);
            GUI.color = previous;
            GUILayout.Label(l[0]);
            GUILayout.Label(l[1]);
            int lang = Mathf.Max(0,Array.IndexOf(VitqraStrings.Languages,settings.language));
            int nextLang = GUILayout.SelectionGrid(lang,VitqraStrings.Languages,6);
            settings.language = VitqraStrings.Languages[nextLang];
            settings.automaticResearch = GUILayout.Toggle(settings.automaticResearch,l[2]);
            settings.automaticTranslation = GUILayout.Toggle(settings.automaticTranslation,l[3]);
            GUILayout.Label(l[4]);
            int p = Mathf.Max(0,Array.IndexOf(providers,settings.provider));
            settings.provider = providers[GUILayout.SelectionGrid(p,providers,3)];
            GUILayout.Label(l[5]);
            if (GUILayout.Button(l[6])) settings.Save();
            GUILayout.Space(12);
            GUILayout.Label(l[7]);
            for (int i = 14; i < 19; i++) if (GUILayout.Button(l[i])) { query = l[i]; searched = true; }
            string nextQuery = GUILayout.TextField(query, 300);
            if (nextQuery != query) { query = nextQuery; searched = false; }
            if (GUILayout.Button(l[8])) searched = true;
            if (searched) GUILayout.Label(l[9]);
            GUILayout.Space(12);
            GUILayout.Label(l[10]);
            note = GUILayout.TextArea(note, 10000, GUILayout.MinHeight(70));
            if (GUILayout.Button(l[11])) { PlayerPrefs.SetString(NoteKey,note); PlayerPrefs.Save(); }
            if (GUILayout.Button(l[12])) note = PlayerPrefs.GetString(NoteKey, "");
            if (GUILayout.Button(l[13])) { note = ""; PlayerPrefs.DeleteKey(NoteKey); PlayerPrefs.Save(); }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
