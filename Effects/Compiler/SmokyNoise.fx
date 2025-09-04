// Shader largely based off of: https://www.shadertoy.com/view/4tcBDn
sampler2D uImage0 : register(s0);
float uTime : register(c0);

float3 color;

//Higher number = more zoomed out
float zoom = 1.0;

// Get random value
float random(in float2 st)
{
    return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
}

// Get noise
float mynoise(in float2 st)
{
    // Splited integer and float values.
    float2 i = floor(st);
    float2 f = frac(st);
    
    float a = random(i + float2(0.0, 0.0));
    float b = random(i + float2(1.0, 0.0));
    float c = random(i + float2(0.0, 1.0));
    float d = random(i + float2(1.0, 1.0));
    
    // -2.0f^3 + 3.0f^2
    float2 u = f * f * (3.0 - 2.0 * f);
    
    return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// fractional brown motion
// Reduce amplitude multiplied by 0.5, and frequency multiplied by 2.
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

float4 main(float4 screenspace : TEXCOORD0) : COLOR0
{
    //Our normal cartesian coordinates
    float2 baseUV = screenspace.xy;

    //Starting color of texture shader is being applied to
    float4 baseCol = tex2D(uImage0, baseUV);
    
    float2 adjustedUV = baseUV;
    adjustedUV *= zoom;
    
    float2 q = float2(0.0, 0.0);
    q.x = fbm(adjustedUV + float2(0.0, 0.0));
    q.y = fbm(adjustedUV + float2(1.0, 1.0));
    
    // These numbers(such as 1.7, 9.2, etc.) are not special meaning.
    float2 r = float2(0.0, 0.0);
    r.x = fbm(adjustedUV + (1.0 * q) + float2(1.7, 9.2) + (0.15 * uTime));
    r.y = fbm(adjustedUV + (1.0 * q) + float2(8.3, 2.8) + (0.12 * uTime));
    
    // Calculate 'r' is that getting domain warping.
    float f = fbm(adjustedUV + r);
    
    // f^3 + 0.6f^2 + 0.5f
    float coef = (f * f * f + (0.6 * f * f) + (0.5 * f));
    
    float4 toReturn = float4(color * coef, baseCol.a);
    return toReturn;
}

technique Technique1
{
    pass Aura
    {
        PixelShader = compile ps_3_0 main();
    }
}
