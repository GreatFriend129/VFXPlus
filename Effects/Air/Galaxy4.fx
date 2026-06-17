sampler uImage0 : register(s0);

float progress = 0.0;
float posterizationSteps = 4.0;
float zoom;

float screenWidth;
float screenHeight;
float width;
float height;
float2 offset;

texture ScrollTexture1;
sampler tex1Sampler = sampler_state
{
    Texture = (ScrollTexture1);
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ScrollTexture2;
sampler tex2Sampler = sampler_state
{
    Texture = (ScrollTexture2);
    AddressU = Wrap;
    AddressV = Wrap;
};

const float TAU = 6.28318;
const float PI = 3.141592;
// All components are in the range [0…1], including hue.
float3 rgb2hsv(float3 color)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(color.bg, K.wz), float4(color.gb, K.xy), step(color.b, color.g));
    float4 q = lerp(float4(p.xyw, color.r), float4(color.r, p.yzx), step(p.x, color.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
// All components are in the range [0…1], including hue.
float3 hsv2rgb(float3 color)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(color.xxx + K.xyz) * 6.0 - K.www);
    return color.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), color.y);
}

float3 Posterize(float3 inputCol)
{
    //If statement bad but w/e
    if (posterizationSteps <= 0.0)
        return inputCol;
    
    //Convert color to hsv to allow for nicer looking posterization
    float3 hsvCol = rgb2hsv(inputCol);
    float3 hsvColCopy = rgb2hsv(inputCol);
    
    //Posterize the value
    hsvCol.z = round(hsvCol.z * posterizationSteps) / posterizationSteps;

    //Convert back to rgb then return
    return hsv2rgb(hsvCol);
}

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 uv = screenSpace.xy;
    uv.x *= 1.875;
    uv.y *= 1.055;
    //uv.x *= screenWidth / 1024;
    //uv.y *= screenHeight / 1024;
    
    float2 newOffset = offset;

    newOffset.x /= 1024;
    newOffset.y /= 1024;
    newOffset += float2(cos(progress * 0.05), sin(progress * 0.05)) * 0.25;
    
    uv = uv + newOffset;
    uv = uv % 1;
    
    float4 baseCol = tex2D(uImage0, screenSpace.xy);
    	
    float2 M = float2(0.0, 0.0);
    M -= float2(M.x + sin(progress * 0.22), M.y - cos(progress * 0.22));
    M *= 0;
    
    float4 dustCol1 = tex2D(tex1Sampler, (uv * zoom) + M) * 0.75;
    float val1 = (dustCol1.rgb) / 3.0;
    float3 dustCol1bright = dustCol1.rgb + (val1 > 0.4 ? ((val1 - 0.4) * 2.5) : float3(0, 0, 0));

    
    
    float4 dustCol2 = tex2D(tex2Sampler, (uv * zoom) + M * 0.5);
    float val2 = (dustCol2.rgb) / 3.0;
    float3 dustCol2bright = dustCol2.rgb + (val2 > 0.4 ? ((val2 - 0.4) * 2.5) : float3(0, 0, 0));
    
    float3 combined = dustCol1 * 2.0;//    +dustCol2bright;
    //combined = Posterize(combined);
    
    return float4(combined, 1.0) * baseCol.a;

}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}