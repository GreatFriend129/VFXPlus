sampler uImage0 : register(s0);

float time = 0.0;

float zoom = 1.0;
float layers = 2.0;

float4 Colors[10];

float random(in float2 st)
{
    return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
}

float mynoise(in float2 st)
{
    // Splited integer and float values.
    float2 i = floor(st + float2(-time, time * 0.5));
    float2 f = frac(st + float2(-time, time * 0.5));
    
    float a = random(i + float2(0.0, 0.0));
    float b = random(i + float2(1.0, 0.0));
    float c = random(i + float2(0.0, 1.0));
    float d = random(i + float2(1.0, 1.0));
    
    // -2.0f^3 + 3.0f^2
    float2 u = f * f * (3.0 - 2.0 * f);
    
    return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// fractional brown motion
float fbm(in float2 st)
{
    float v = 0.0;
    float a = 0.5;
    
    for (int i = 0; i < 5; i++)
    {
        v += a * mynoise(st);
        st = st * 2.0;
        a *= 0.5;
    }
    
    return v;
}

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    //Gets the screen coord
    float2 st = screenSpace.xy;
    float4 baseCol = tex2D(uImage0, st);
    

   
    float4 combinedColor = float4(0.0, 0.0, 0.0, 0.0);
    for (int i = 0; i < layers; i++)
    {
        float2 adjustedUV = st;
        adjustedUV *= zoom + i;
        
        float2 q = float2(0.0, 0.0);
        q.x = fbm(adjustedUV + float2(0.0, 0.0));
        q.y = fbm(adjustedUV + float2(1.0, 1.0));
        
        float2 r = float2(0.0, 0.0);
        r.x = fbm(adjustedUV + (1.0 * q) + float2(1.7, 9.2) + ((0.15 * i) * time));
        r.y = fbm(adjustedUV + (1.0 * q) + float2(8.3, 2.8) + ((0.12 * i) * time));
        
        float f = fbm(adjustedUV + r);
        float coef = (f * f * f + (0.6 * f * f) + (0.5 * f));
        combinedColor += float4(Colors[i].rgb * coef, Colors[i].a);
    }
    
    return combinedColor * baseCol.a;
    
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}