Shader "Hidden/BlitAdd"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Alpha;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                c.a *= _Alpha;
                return c;
            }
            ENDCG
        }
    }
}
