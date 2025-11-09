using EditorAttributes;
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
        [Range(0.0f, 1.0f)] public float alpha = 1f;
        
        [Space(10)]
        [Header("Contour")]
        public bool drawOutline = false;
        [Range(0.5f, 50f)] public float outlineThickness = 2f;
        public Color outlineColor = Color.white;

        [Space(10)] 
        [Header("Crop")]
        [Tooltip("pixels to cut at left")]
        public int tl = 0;
        [Tooltip("pixels to cut at right")]
        public int tr = 0;
        [Tooltip("pixels to cut at top")]
        public int tt = 0;
        [Tooltip("pixels to cut at bottom")]
        public int tb = 0;
        
        // intern cache (not serialized)
        // like that it's not stored in the scene, 
        [System.NonSerialized] public bool needsRecalc = true;
        [System.NonSerialized] public Rect cachedSrcRect;
        [System.NonSerialized] public float cachedCropW, cachedCropH, cachedOffsetX, cachedOffsetY;
    }

    [SerializeField] private RenderTexture targetRender;
    [SerializeField] private Color backgroundColor = Color.clear; 
    [SerializeField] private CaseData[] cases;
    
    [Header("Perf")]
    [SerializeField] private bool autoUpdate = true;
    [Tooltip("enable culling for outscreen textures")]
    [SerializeField] private bool enableCulling = true;
    
    private Material blitMaterial;
    private bool isDirty = true;
    private static Material lineMat;

    void OnEnable()
    {
        if (blitMaterial == null)
        {
            Shader s = Shader.Find("Hidden/BlitAdd");
            if (s != null) blitMaterial = new Material(s);
        }
        MarkDirty();
    }

    // called when a value is change inn inspector
    private void OnValidate()
    {
        MarkDirty();
    }
    
    private void Update()
    {
        if (!autoUpdate && !isDirty) return;
        RenderInternal();
        isDirty = false;
    }

    /// <summary>
    /// say if texture needs update
    /// </summary>
    private void MarkDirty()
    {
        isDirty = true;
        if (cases != null)
        {
            foreach (var c in cases)
            {
                if (c != null) c.needsRecalc = true;
            }
        }
    }

    /// <summary>
    /// force an immediate render
    /// </summary>
    public void Render()
    {
        RenderInternal();
    }

    void RenderInternal()
    {
        if (targetRender == null) return;
        if (blitMaterial == null)
        {
            Debug.LogWarning("No blitMaterial assigned or shader Hidden/BlitAdd not found.");
            return;
        }
        if (cases == null || cases.Length == 0) return;

        int w = targetRender.width;
        int h = targetRender.height;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = targetRender;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, w, 0, h);

        GL.Clear(true, true, backgroundColor);

        // bounds for culling
        Rect screenBounds = new Rect(0, 0, w, h);

        foreach (var c in cases)
        {
            if (c == null || c.texture == null) continue;

            // calculate crop data if necessary
            if (c.needsRecalc)
            {
                CalculateSourceRect(c);
                c.needsRecalc = false;
            }

            // base position 
            float baseX = c.x * w;
            float baseY = c.y * h;
            float basePW = Mathf.Max(1, c.width * w);
            float basePH = Mathf.Max(1, c.height * h);

            // ajuts size and position with crop
            float finalWidth = basePW * c.cachedCropW;
            float finalHeight = basePH * c.cachedCropH;
            float px = baseX + (basePW * c.cachedOffsetX);
            float py = baseY + (basePH * c.cachedOffsetY);

            Rect dst = new Rect(px, py, finalWidth, finalHeight);

            // culling : skip si hors écran
            if (enableCulling && !dst.Overlaps(screenBounds))
                continue;

            blitMaterial.SetFloat("_Alpha", c.alpha);
            Graphics.DrawTexture(dst, c.texture, c.cachedSrcRect, 0, 0, 0, 0, blitMaterial);
            
            if (c.drawOutline && c.outlineThickness > 0f)
                DrawRectOutline(dst, c.outlineColor, c.outlineThickness);
        }

        GL.PopMatrix();
        RenderTexture.active = prev;
    }
    
    /// <summary>
    /// calculate and cache source rectangle and crop parameters
    /// </summary>
    void CalculateSourceRect(CaseData c)
    {
        if (c.texture == null)
        {
            c.cachedSrcRect = new Rect(0, 0, 1, 1);
            c.cachedCropW = 1f;
            c.cachedCropH = 1f;
            c.cachedOffsetX = 0f;
            c.cachedOffsetY = 0f;
            return;
        }

        float texWidth = c.texture.width;
        float texHeight = c.texture.height;

        // convert pixels in normalised coordinates
        float left = c.tl / texWidth;
        float right = c.tr / texWidth;
        float top = c.tt / texHeight;
        float bottom = c.tb / texHeight;

        // Clamp to avoid invalids values
        left = Mathf.Clamp(left, 0f, 0.99f);
        right = Mathf.Clamp(right, 0f, 0.99f);
        top = Mathf.Clamp(top, 0f, 0.99f);
        bottom = Mathf.Clamp(bottom, 0f, 0.99f);

        // test left + right < 1 and top + bottom < 1
        if (left + right >= 1f)
        {
            float scale = 0.99f / (left + right);
            left *= scale;
            right *= scale;
        }
        if (top + bottom >= 1f)
        {
            float scale = 0.99f / (top + bottom);
            top *= scale;
            bottom *= scale;
        }

        // stock in cache
        c.cachedCropW = 1f - left - right;
        c.cachedCropH = 1f - top - bottom;
        c.cachedOffsetX = left;
        c.cachedOffsetY = top;
        c.cachedSrcRect = new Rect(left, bottom, c.cachedCropW, c.cachedCropH);
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

        float maxThick = Mathf.Min(thickness, Mathf.Min(rect.width, rect.height) * 0.5f);

        // top
        GL.Vertex3(xMin - maxThick, yMax, 0);
        GL.Vertex3(xMax + maxThick, yMax, 0);
        GL.Vertex3(xMax + maxThick, yMax + maxThick, 0);
        GL.Vertex3(xMin - maxThick, yMax + maxThick, 0);

        // bottom
        GL.Vertex3(xMin - maxThick, yMin - maxThick, 0);
        GL.Vertex3(xMax + maxThick, yMin - maxThick, 0);
        GL.Vertex3(xMax + maxThick, yMin, 0);
        GL.Vertex3(xMin - maxThick, yMin, 0);

        // left
        GL.Vertex3(xMin - maxThick, yMin, 0);
        GL.Vertex3(xMin, yMin, 0);
        GL.Vertex3(xMin, yMax, 0);
        GL.Vertex3(xMin - maxThick, yMax, 0);

        // right
        GL.Vertex3(xMax, yMin, 0);
        GL.Vertex3(xMax + maxThick, yMin, 0);
        GL.Vertex3(xMax + maxThick, yMax, 0);
        GL.Vertex3(xMax, yMax, 0);

        GL.End();
    }

    Material GetLineMaterial()
    {
        if (lineMat == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZWrite", 0);
        }
        return lineMat;
    }
}