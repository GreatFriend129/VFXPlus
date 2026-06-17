sampler2D uImage0 : register(s0);
float uTime : register(c0);


float3 color;

float glowThreshold = 0.4;
float glowPower = 2.5;


float progress = 0.0;

float3 innerCol;
float3 outerCol;

//Cant use GetDimension on something that is not a texture2D (which we would need ps_4_0 for)
//So instead we need to pass in the dimensions of the main texture
//float2 mainTexDimensions;
float mainTexWidth;
float mainTexHeight;

texture maskTexture;
sampler2D maskTex = sampler_state
{
    texture = <maskTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float4 main(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 baseUV = screenspace.xy;

    //Starting color of texture shader is being applied to
    float4 baseCol = tex2D(uImage0, baseUV);
    
    float3 brighten = baseCol.rgb * baseCol.a * color + (baseCol.a > glowThreshold ? ((baseCol.a - glowThreshold) * glowPower) : float3(0, 0, 0));
    
    float4 newBaseCol = float4(baseCol.rgb * brighten, baseCol.a);
    
    ////
    float2 dissolveUV = float2(floor(baseUV.x * mainTexWidth) / mainTexWidth, floor(baseUV.y * mainTexHeight) / mainTexHeight);
    
    //Fix bizzare rounding error
    dissolveUV.y += 0.01;
    
    float4 noiseCol = tex2D(maskTex, dissolveUV);
		
    float adjustedFadeProg = progress * 1.2;
	
    float3 erosion = smoothstep(adjustedFadeProg - 0.2, adjustedFadeProg, noiseCol.rgb);
    
    float3 border = smoothstep(0., .1, erosion) - smoothstep(.1, 1., erosion);
        
    float3 col = (erosion) * newBaseCol.rgb;
     
    float3 fire = lerp(innerCol, outerCol, smoothstep(0.8, 1., border)) * 2.;
    
    col += border * fire * 2.0;
	
    return float4(col * newBaseCol.a, newBaseCol.a * erosion.r * 4.0) * newBaseCol; 
    
}

technique Technique1
{
    pass Aura
    {
        PixelShader = compile ps_3_0 main();
    }
}
