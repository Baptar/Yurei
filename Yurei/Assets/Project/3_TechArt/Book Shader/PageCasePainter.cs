using UnityEngine;

[ExecuteAlways]
public class PageCasePainter : MonoBehaviour
{
    [System.Serializable]
    public class CaseData
    {
        public string name;
        public Texture texture;
        public float x = 0f;       
        public float y = 0f;       
        public float width = 0.3f; 
        public float height = 0.3f;
        public float alpha = 1f;
        
        [Header("Contour")]
        public bool drawOutline = false;
        [Range(0.5f, 50f)] public float outlineThickness = 2f;
        public Color outlineColor = Color.white;
    }

    public RenderTexture targetRender;
    public Color backgroundColor = Color.clear; 
    public CaseData[] cases;
    public bool update = true;
    
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

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = targetRender;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, w, 0, h);

        GL.Clear(true, true, backgroundColor);

        foreach (var c in cases)
        {
            if (c == null || c.texture == null) continue;

            float px = c.x * w;
            float py = c.y * h;
            float pw = Mathf.Max(1, c.width * w);
            float ph = Mathf.Max(1, c.height * h);

            Rect dst = new Rect(px, py, pw, ph);

            blitMaterial.SetFloat("_Alpha", c.alpha);

            Graphics.DrawTexture(dst, c.texture, blitMaterial);
            
            if (c.drawOutline && c.outlineThickness > 0f)
                DrawRectOutline(dst, c.outlineColor, c.outlineThickness);
        }

        GL.PopMatrix();
        RenderTexture.active = prev;
    }
    
    void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        Material lineMat = GetLineMaterial();
        lineMat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(color);

        float xMin = rect.xMin;
        float xMax = rect.xMax;
        float yMin = rect.yMin;
        float yMax = rect.yMax;

        float maxThick = Mathf.Min(thickness, Mathf.Min(rect.width, rect.height));

        // Haut
        GL.Vertex3(xMin - maxThick, yMax, 0);
        GL.Vertex3(xMax + maxThick, yMax, 0);
        GL.Vertex3(xMax + maxThick, yMax + maxThick, 0);
        GL.Vertex3(xMin - maxThick, yMax + maxThick, 0);

        // Bas
        GL.Vertex3(xMin - maxThick, yMin - maxThick, 0);
        GL.Vertex3(xMax + maxThick, yMin - maxThick, 0);
        GL.Vertex3(xMax + maxThick, yMin, 0);
        GL.Vertex3(xMin - maxThick, yMin, 0);

        // Gauche
        GL.Vertex3(xMin - maxThick, yMin, 0);
        GL.Vertex3(xMin, yMin, 0);
        GL.Vertex3(xMin, yMax, 0);
        GL.Vertex3(xMin - maxThick, yMax, 0);

        // Droite
        GL.Vertex3(xMax, yMin, 0);
        GL.Vertex3(xMax + maxThick, yMin, 0);
        GL.Vertex3(xMax + maxThick, yMax, 0);
        GL.Vertex3(xMax, yMax, 0);

        GL.End();
    }

    static Material _lineMat;
    Material GetLineMaterial()
    {
        if (_lineMat == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _lineMat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _lineMat.SetInt("_ZWrite", 0);
        }
        return _lineMat;
    }
}
