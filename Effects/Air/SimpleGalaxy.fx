sampler2D uImage0 : register(s0);

float progress = 0.0;
float zoom1;

float2 offset;

texture ScrollTexture1;
sampler2D tex1Sampler = sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    Texture = <ScrollTexture1>;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ScrollTexture2;
sampler2D tex2Sampler = sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    Texture = <ScrollTexture2>;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ScrollTexture3;
sampler2D tex3Sampler = sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    Texture = <ScrollTexture3>;
    AddressU = Wrap;
    AddressV = Wrap;
};
float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 uv = screenSpace.xy;
    
    float2 actualOffset = offset / 1080;
    
    float4 baseCol = tex2D(uImage0, screenSpace.xy);

    float2 UnderUV = float2(uv.x + sin(progress * 0.02), uv.y - cos(progress * 0.02)); 
    //float2 UnderUV = float2(uv.x + progress * 0.12, uv.y);
    float4 UnderCol = tex2D(tex1Sampler, (UnderUV * zoom1) + actualOffset * 0.5);

    float2 MiddleUV = float2(uv.x + sin(progress * 0.05), uv.y - cos(progress * 0.05));
    float4 MiddleCol = tex2D(tex3Sampler, (MiddleUV * zoom1) + actualOffset * 0.7);
    
    float2 OverUV = float2(uv.x + sin(progress * 0.05), uv.y - cos(progress * 0.05)); 
    float4 OverCol = tex2D(tex3Sampler, (OverUV * zoom1) + actualOffset);

    float2 OverUV3 = float2(uv.x + sin(progress * 0.05), uv.y - cos(progress * 0.05));
    float4 OverCol3 = tex2D(tex3Sampler, (OverUV3 * zoom1) + actualOffset * 1.15);
    
    float4 trueCol = (UnderCol * 0.0) + OverCol + MiddleCol + OverCol3;
    
    //if (OverCol.a == 0)
    //    trueCol += OverCol;
    
    return float4(trueCol.rgb, 1.0) * baseCol.a;

    
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}