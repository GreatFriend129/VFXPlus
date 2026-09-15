sampler2D uImage0 : register(s0);
float uTime : register(c0);

texture maskTexture;
sampler2D maskTex = sampler_state
{
    texture = <maskTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    //AddressU = wrap;
    //AddressV = wrap;
};

float3 color;

float glowThreshold = 0.4;
float glowPower = 2.5;
float fadeProgress = 1.0;

float endAlpha = 1.0;

float blackRemoveThreshold = 0.1;

float4 main(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 baseUV = screenspace.xy;

    //Starting color of texture shader is being applied to
    float4 baseCol = tex2D(uImage0, baseUV);
    
    float3 brighten = baseCol.rgb * baseCol.a * color + (baseCol.a > glowThreshold ? ((baseCol.a - glowThreshold) * glowPower) : float3(0, 0, 0));
    
    
    //Subtract using the mask based on fadeProgress 
    float4 maskCol = tex2D(maskTex, baseUV);
    float3 toRet = baseCol.rgb -= (maskCol.rgb * fadeProgress);
    
    float3 finalCol = toRet * brighten * (endAlpha * (1.0 - fadeProgress));
    
    //Remove black from result
    float finalColAVG = length(finalCol) * 1.0;
    
    if (finalColAVG < blackRemoveThreshold)
        finalColAVG = 0.0;
    
    //finalColAVG = lerp(0.0, 1.0, finalColAVG);
    
    float4 toReturn = float4(finalCol, 1.0) * finalColAVG;
    return toReturn;
    
    //Remove all color under threshold
    //float Todelete = 1.0;
    //if (length(toReturn.rgb) < blackRemoveThreshold)
    //    Todelete = 0.0;
    
    //return toReturn * Todelete;
    
    //float4 toReturn = 
    //return toReturn;
}

technique Technique1
{
    pass Aura
    {
        PixelShader = compile ps_3_0 main();
    }
}
