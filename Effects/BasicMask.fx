sampler uImage0 : register(s0);

texture Mask;
sampler maskTex = sampler_state
{
    texture = <Mask>;
};

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 st = screenSpace.xy;
    float4 baseCol = tex2D(uImage0, st);
    float4 maskCol = tex2D(maskTex, st);
    baseCol *= maskCol.a;
    return baseCol;
    
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}