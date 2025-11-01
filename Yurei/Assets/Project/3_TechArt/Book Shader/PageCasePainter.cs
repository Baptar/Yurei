using UnityEngine;

[ExecuteAlways]
public class PageCasePainter : MonoBehaviour
{
    [System.Serializable]
    public class CaseData
    {
        public string name;
        public Texture texture;
        public float x = 0f;       // left (0..1)
        public float y = 0f;       // bottom (0..1)
        public float width = 0.3f; // (0..1)
        public float height = 0.3f;// (0..1)
        public float alpha = 1f;
    }

    public RenderTexture targetRender;
    public Color backgroundColor = Color.clear; // si transparent -> Color.clear
    public CaseData[] cases;
    public bool update = true;

    // Material qui contient le shader Hidden/BlitAdd (ou tout shader unlit avec alpha)
    [Tooltip("Material must use an alpha-blending shader (SrcAlpha OneMinusSrcAlpha).")]
    public Material blitMaterial;

    void OnEnable()
    {
        if (blitMaterial == null)
        {
            Shader s = Shader.Find("Hidden/BlitAdd");
            if (s != null) blitMaterial = new Material(s);
        }
    }

    void Update()
    {
        if (!update) return;
        
        if (targetRender == null) return;
        if (blitMaterial == null)
        {
            Debug.LogWarning("No blitMaterial assigned or shader Hidden/BlitAdd not found.");
            return;
        }

        int w = targetRender.width;
        int h = targetRender.height;

        // Bind RT and clear (avec alpha)
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = targetRender;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, w, 0, h); // espace pixel dans la RT

        // Clear: utilise GL.Clear pour fixer le fond
        GL.Clear(true, true, backgroundColor);

        // Draw chaque case avec DrawTexture (position en pixels)
        foreach (var c in cases)
        {
            if (c == null || c.texture == null) continue;

            // Calcul de la rectangle en pixels (y zéro = bottom)
            float px = c.x * w;
            float py = c.y * h;
            float pw = Mathf.Max(1, c.width * w);
            float ph = Mathf.Max(1, c.height * h);

            Rect dst = new Rect(px, py, pw, ph);

            // Paramètre alpha si nécessaire (assure-toi que blitMaterial utilise _MainTex)
            blitMaterial.SetFloat("_Alpha", c.alpha);

            // DrawTexture utilise le material si fourni - il doit sampler _MainTex
            Graphics.DrawTexture(dst, c.texture, blitMaterial);
        }

        GL.PopMatrix();
        RenderTexture.active = prev;
    }
}
