Shader "Unlit/VertexSnapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Colour("Color", Color) = (0.5, 0.5, 0.5, 1)
		_VertexSnap("Vertex Snapping", Float) = 40
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Colour;
			float _VertexSnap;

            v2f vert (appdata v)
            {
                v2f o;
				float3 wp = UnityObjectToViewPos(v.vertex.xyz);
				wp.xyz = floor(wp.xyz * _VertexSnap) / _VertexSnap;
				o.vertex = mul(UNITY_MATRIX_P, float4(wp.xyz, 1.0f));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Colour;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
