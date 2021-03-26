Shader "Custom/MultiSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Weight ("Weight", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _Weight;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c1 = tex2D (_MainTex, IN.uv_MainTex + _Time.y) * _Color * _Weight;
            fixed4 c2 = tex2D (_MainTex, IN.uv_MainTex + _Time.x *1.01) * _Color * _Weight;
            fixed4 c3 = tex2D (_MainTex, IN.uv_MainTex + _Time.x * 1.011) * _Color * _Weight;
            fixed4 c4 = tex2D (_MainTex, IN.uv_MainTex + _Time.x * 1.014) * _Color * _Weight;
            //o.Albedo = max( max( c.r , c.g ), max( c.b , c.a ) );
            o.Albedo = ( c1.r + c2.g + c3.b + c4.a ) / 4;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;//c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
