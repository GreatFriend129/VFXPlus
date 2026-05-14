matrix WorldViewProjection;

float progress;

//How many times the texture repeats
float reps = 1.0;

float posterizationSteps = 0.0;

float finalColMult = 1.0;
float totalMult = 1.0;

float2 noiseScale;
float noiseIntensity = 1.0;

float flowSpeed = 1.0;
float flowYOffset = 0.0;
float2 flowScale;

//Adds a flat amount to the flowTexture's brightness
float flowGammaBoost = 0.0;

//The colors of the gradient
float3 gradColors[10];

//The number of colors in the gradient
float numberOfColors = 5;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0; //float 3 b/c z is going to be the trail width 
};

texture TrailTexture;
sampler trailTex = sampler_state
{
    texture = <TrailTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

texture NoiseTexture;
sampler noiseTex = sampler_state
{
    texture = <NoiseTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

texture FlowTexture;
sampler flowTex = sampler_state
{
    texture = <FlowTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

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

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, WorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;

    output.TextureCoordinates = input.TextureCoordinates;

    return output;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{    
    float2 baseCoords = input.TextureCoordinates.xy;
    
    //Fix trail appearing jaggy if the width changes with trail progress
    baseCoords.y = (baseCoords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float yOffset = tex2D(noiseTex, float2((baseCoords.x * noiseScale.x) + progress, baseCoords.y * noiseScale.y)).r;
        
    float2 chainCoords = float2(baseCoords.x * reps, baseCoords.y - (yOffset * 0.03 * noiseIntensity));
        
    float4 in_color = tex2D(trailTex, chainCoords);
    float4 flowScroll = tex2D(flowTex, float2((baseCoords.x * flowScale.x) + progress * flowSpeed, flowYOffset + (baseCoords.y * flowScale.y)));
    flowScroll.rgb = saturate(flowScroll.rgb + float3(flowGammaBoost, flowGammaBoost, flowGammaBoost));
    
    //Remove black from result
    float finalColAVG = length(in_color.rgb);
    finalColAVG = smoothstep(0.0, 1.0, finalColAVG);
    

    float4 toPosterize = in_color;
    //if (chainCoords.y < 0.01 && length(toPosterize.rgb != 0.0))
    //    toPosterize.rgb = float3(1.0, 1.0, 1.0);
    
    toPosterize.rgb = Posterize(toPosterize.rgb);
    
    toPosterize.rgb = blendColors(toPosterize.r);
    
    float alpha = 1.0;
    
    if (length(toPosterize.rgb) == 0.0)
        alpha = 0.0;
    
    if (posterizationSteps <= 0.0)
        return float4(toPosterize.rgb * flowScroll.rgb * finalColMult, alpha * length(toPosterize.rgb)) * totalMult;
    else
        return float4(toPosterize.rgb * flowScroll.rgb * finalColMult, alpha * input.Color.a) * totalMult;
    
}

technique BasicColorDrawing
{
    pass DefaultPass
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};