//Borrowed from SotS WaterTrail

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float progress;
matrix WorldViewProjection;
float4 uShaderSpecificData;
float fadeAmount = 0.0;
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
sampler tent = sampler_state
{
    Texture = (TrailTexture);
    AddressU = Wrap;
    AddressV = Wrap;
};
VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, WorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;

    output.TextureCoordinates = input.TextureCoordinates;

    return output;
};

float4 White(VertexShaderOutput input) : COLOR0
{
    float2 baseCoords = input.TextureCoordinates.xy;
    
    //Fix trail appearing jaggy if the width changes with trail progress
    baseCoords.y = (baseCoords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float x = (baseCoords.x + progress) % 1;
    float2 noisecoords = float2(x, baseCoords.y);
    float brightness = tex2D(tent, noisecoords).r;
    float4 color = input.Color;
    color *= sqrt(brightness);
    return color * sqrt(input.TextureCoordinates.x);
}

technique BasicColorDrawing
{
    pass MainPS
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 White();
    }
};