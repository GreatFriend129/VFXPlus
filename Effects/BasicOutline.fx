sampler uImage0 : register(s0);

float4 outlineColor;
float outlineThickness;

// returns 1 if input > 0, else 0
float gtz(float input)
{
    return max(0, sign(input));
}


float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{   
    float2 uv = screenSpace.xy;
    float4 baseCol = tex2D(uImage0, uv);
    
    float thickness = 0.0005 * outlineThickness;
    
    float right = tex2D(uImage0, uv + float2(thickness, 0.0)).a;
    float left = tex2D(uImage0, uv + float2(-thickness, 0.0)).a;
    float up = tex2D(uImage0, uv + float2(0.0, -thickness)).a;
    float down = tex2D(uImage0, uv + float2(0.0, thickness)).a;
    
    float upRight = tex2D(uImage0, uv + float2(thickness, -thickness)).a;
    float upLeft = tex2D(uImage0, uv + float2(-thickness, -thickness)).a;
    float downRight = tex2D(uImage0, uv + float2(thickness, thickness)).a;
    float downLeft = tex2D(uImage0, uv + float2(-thickness, thickness)).a;
    
    float valid = gtz(baseCol.a) * (1 - gtz(right));    
    valid = max(valid, gtz(baseCol.a) * (1 - gtz(left)));
    valid = max(valid, gtz(baseCol.a) * (1 - gtz(up)));
    valid = max(valid, gtz(baseCol.a) * (1 - gtz(down)));
    
    valid = max(valid, gtz(baseCol.a) * (1 - gtz(upRight)));
    valid = max(valid, gtz(baseCol.a) * (1 - gtz(upLeft)));
    valid = max(valid, gtz(baseCol.a) * (1 - gtz(downRight)));
    valid = max(valid, gtz(baseCol.a) * (1 - gtz(downLeft)));
    
    return baseCol + (baseCol * outlineColor * valid);
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}