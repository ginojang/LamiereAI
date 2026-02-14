// MatCap Shader, (c) 2015-2019 Jean Moreno

Shader "MatCap/Vertex/Textured_Multiply_Bump"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		
		[Header(Normal)]
		_BumpMap ("Normal Map", 2D) = "bump" {}
		[Toggle(MATCAP_ACCURATE)] _MatCapAccurate ("Accurate Calculation", Int) = 0

		[Header(MatCap)]
		_MatCap("MatCap (RGB)", 2D) = "white" {}

		[Header(Eradication)]
		[Toggle] _UsingEradication("Using Eradication", Float) = 0
		_EradicationIntensity("Eradication Intensity", Range(1.0,10.0)) = 1.0

		[Header(Dissolve)]
		[Toggle(DISSOLVE)] _UsingDissolve("Using Dissolve", Float) = 0
		_DissolveTex("Dissolve Texture", 2D) = "white" {}
		_DissolveEdge("Dissolve Edge",Range(0.01,0.5)) = 0.01
		_DissolveProgress("Dissolve Progress",Range(0,1)) = 0
		[Enum(Off,0,Front,1,Back,2)] _CullMode("Culling Mode", int) = 0
		[Enum(Off,0,On,1)] _ZWrite("ZWrite", int) = 0

		[HideIfDisabled(DISSOLVE)][NoScaleOffset] _EdgeAroundRamp("Edge Ramp", 2D) = "white" {}
		[HideIfDisabled(DISSOLVE)]_EdgeAround("Edge Color Range",Range(0,0.5)) = 0.099
		[HideIfDisabled(DISSOLVE)]_EdgeAroundPower("Edge Color Power",Range(1,5)) = 3
		[HideIfDisabled(DISSOLVE)]_EdgeAroundHDR("Edge Color HDR",Range(1,3)) = 1.58
		[HideIfDisabled(DISSOLVE)]_EdgeDistortion("Edge Distortion",Range(0,1)) = 0.33
	}

	Subshader
	{
		//텍스쳐 랩핑
		Tags { "RenderType" = "Opaque" }
		usePass "ShaderFunction/MatCapTexture/MATCAPTEXTURE"

		//발광
		usePass "ShaderFunction/Radiation/RADIATION"

		//디졸브
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		usePass "ShaderFunction/Dissolve/DISSOLVE"
	}
	Fallback "VertexLit"
}