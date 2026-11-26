using UnityEngine;

namespace Vitqra
{
    // Procedural placeholder; no imported model, texture, audio or animation assets.
    public sealed class VitqraAvatarPreview : MonoBehaviour
    {
        private Transform bust;
        private Renderer head, torso, visor;
        private Material material;
        private MaterialPropertyBlock properties;
        public void SetProfile(int profile)
        {
            if (bust == null) Create();
            bust.gameObject.SetActive(profile >= 0 && profile <= 3);
            if (!bust.gameObject.activeSelf) return;
            bool younger = profile >= 2;
            bool alternate = profile % 2 == 1;
            bust.localScale = Vector3.one * (younger ? .82f : 1f);
            head.transform.localScale = new Vector3(.11f, .12f, .085f);
            torso.transform.localScale = new Vector3(alternate ? .145f : .16f, .115f, .08f);
            Tint(head, alternate ? new Color(.79f,.65f,1f) : new Color(.4f,.94f,.94f));
            Tint(torso, alternate ? new Color(.35f,.24f,.63f) : new Color(.08f,.40f,.48f));
            Tint(visor, new Color(.05f,.10f,.18f));
        }

        private void Create()
        {
            bust = new GameObject("Procedural avatar bust").transform;
            bust.SetParent(transform, false);
            bust.localPosition = new Vector3(-.19f,.01f,-.065f);
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader != null) material = new Material(shader);
            properties = new MaterialPropertyBlock();
            head = Part("Head", PrimitiveType.Sphere, new Vector3(0,.045f,0), new Vector3(.11f,.12f,.085f));
            torso = Part("Torso", PrimitiveType.Capsule, new Vector3(0,-.057f,0), new Vector3(.16f,.115f,.08f));
            visor = Part("Visor", PrimitiveType.Cube, new Vector3(0,.05f,-.042f), new Vector3(.082f,.022f,.012f));
        }

        private Renderer Part(string name, PrimitiveType kind, Vector3 position, Vector3 scale)
        {
            var obj = GameObject.CreatePrimitive(kind);
            obj.name = name;
            obj.transform.SetParent(bust,false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            var collider = obj.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; Destroy(collider); }
            var renderer = obj.GetComponent<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            return renderer;
        }

        private void Tint(Renderer renderer, Color color)
        {
            properties.Clear();
            properties.SetColor("_BaseColor",color);
            properties.SetColor("_Color",color);
            renderer.SetPropertyBlock(properties);
        }

        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
