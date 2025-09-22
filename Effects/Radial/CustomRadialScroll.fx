// Shader based off of: https://nekotoarts.github.io/blog/Polar-Water-Breakdown
const float TAU = 6.283185;

sampler2D uImage0 : register(s0);
float uTime : register(c0);

float flowSpeed = 0.7;
float distortStrength = 0.05;
float colorIntensity = 1.0;

float3 gradColors[10];
float gradPowers[10];
float numberOfColors = 5;

//Higher number = more zoomed out
float zoom = 1.0;

texture causticTexture;
sampler2D causticTex = sampler_state
{
    texture = <causticTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    //AddressU = wrap;
    //AddressV = wrap;
};

texture gradientTexture;
sampler2D gradientTex = sampler_state
{
    texture = <gradientTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture distortTexture;
sampler2D distortTex = sampler_state
{
    texture = <distortTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap; //
    AddressV = wrap; //
};

//GLSLs mod function is different than HLSL's fmod function so we have to remake it
float2 glsl_mod(float2 ab, float y)
{
    float xA = ab.x - y * floor(ab.x / y);
    float yB = ab.y - y * floor(ab.y / y);
    return float2(xA, yB);
}

float2 polar_coordinates(float2 uv, float2 center, float zoom, float repeat)
{
    float2 dir = uv - center;
    float radius = length(dir) * 2.0;
    float angle = atan2(dir.y, dir.x) / TAU; //atan --> atan2 
    return glsl_mod(float2(radius * zoom, angle * repeat), 1.0);
}

float3 get_gradient_color(float x)
{   
    int numberOfLerps = numberOfColors - 1;
    
    int colIndexA = floor(x * numberOfLerps);
    
    float colPercent = (x * numberOfLerps) - colIndexA;
    
    float3 toReturn = lerp(gradColors[colIndexA], gradColors[colIndexA + 1], pow(colPercent, gradPowers[colIndexA]));
    
    return toReturn;
}

float4 main(float4 screenspace : TEXCOORD0) : COLOR0
{
    //Our normal cartesian coordinates
    float2 baseUV = screenspace.xy;

    //Starting color of texture shader is being applied to
    float4 baseCol = tex2D(uImage0, baseUV);
    
    //Only draw where the input texture is not black (or not close to black) | Assumes tex is greyscale
    baseCol.a = baseCol.r * baseCol.r;
    
    //Distort our starting UVs using the noise texture
    float distortAmount = tex2D(distortTex, screenspace.xy + uTime * 0.1).r;
    float adjustedUV = baseUV;
    adjustedUV += distortAmount * distortStrength;
    adjustedUV -= distortStrength / 2.0;
    adjustedUV *= zoom;
    
    //Convert the distorted UVs into polar coordinates (r, theta)
    float2 polarUV = polar_coordinates(adjustedUV, float2(0.5, 0.5) * zoom, 1.0, 1.0);
    polarUV.x -= uTime * flowSpeed * zoom;
    
    //Get the color of the caustic texture at the polar coords (All input textures should be grayscale so rgb are all equal)
    float caus = tex2D(causticTex, polarUV).r;
    
    //Color the caustic texture using the gradient
    float3 causColor = get_gradient_color(caus);
    
    float4 toReturn = float4(causColor.rgb * colorIntensity, baseCol.a);
    
    //Remove black from result
    toReturn.a *= (toReturn.r + toReturn.g + toReturn.b);
    
    return toReturn;
}

technique Technique1
{
    pass Aura
    {
        PixelShader = compile ps_3_0 main();
    }
}
