sampler uImage0 : register(s0);

float progress;

float posterizationSteps = 0.0;

//The colors of the gradient
float3 gradColors[10];

//The number of colors in the gradient
float numberOfColors = 5;

//Higher number = more zoomed out
float zoom = 1.0;

float screenWidth;
float screenHeight;
float width;
float height;
float2 offset;

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

////Posterization
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

//Converts grayscale value to respective gradient color
float3 blendColors(float gray)
{
    float segment = 1.0 / float(numberOfColors - 1);
    float scaledGray = gray / segment;
    
    int i = int(floor(scaledGray));
    i = clamp(i, 0, numberOfColors - 2);
    
    float t = (gray - float(i) * segment) / segment;

    return lerp(gradColors[i], gradColors[i + 1], t);
}

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 baseCoords = screenSpace.xy;
    float4 baseCol = tex2D(uImage0, baseCoords);
    
    float2 adjustedUV = baseCoords - float2(sin(progress * 0.15), -cos(progress * 0.15));
    adjustedUV *= zoom;
    
    float2 q = float2(0.0, 0.0);
    q.x = fbm(adjustedUV + float2(0.0, 0.0));
    q.y = fbm(adjustedUV + float2(1.0, 1.0));
    
    // These numbers(such as 1.7, 9.2, etc.) are not special meaning.
    float2 r = float2(0.0, 0.0);
    r.x = fbm(adjustedUV + (1.0 * q) + float2(1.7, 9.2) + (0.15 * progress));
    r.y = fbm(adjustedUV + (1.0 * q) + float2(8.3, 2.8) + (0.12 * progress));
    
    // Calculate 'r' is that getting domain warping.
    float f = fbm(adjustedUV + r);
    
    // f^3 + 0.6f^2 + 0.5f
    float coef = (f * f * f + (0.6 * f * f) + (0.5 * f));
    
    float4 toPosterize = float4(float3(1.0, 1.0, 1.0) * coef, 1.0);
    
    toPosterize.rgb = Posterize(toPosterize.rgb);
    
    toPosterize.rgb = blendColors(toPosterize.r);
    
    float alpha = 1.0;
    
    if (posterizationSteps <= 0.0)
        return float4(toPosterize.rgb, alpha * length(toPosterize.rgb)) * baseCol.a;
    else
        return float4(toPosterize.rgb, alpha) * baseCol.a;
    
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}