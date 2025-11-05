sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float progress;
float3 ColorOne;
matrix WorldViewProjection;
float4 uShaderSpecificData;

float3 gradColors[10];
float numberOfColors = 5;

float colorIntensity = 1.0;

float reps = 1.0;

//1 = lerp
int easeType = 0;

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinates : TEXCOORD0; 
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};
texture TrailTexture;
sampler tent = sampler_state
{
    Texture = (TrailTexture);
    AddressU = Wrap;
    AddressV = Wrap;
};

float3 sineMix(float3 a, float3 b, float x, int type)
{
    float PI = 3.14159;
    return lerp(a, b, sin((x * PI) / 2.0));
}


//Maps the inputed color to the gradient
float3 blendColors(float gray)
{
    float segment = 1.0 / float(numberOfColors - 1);
    float scaledGray = gray / segment;
    
    int i = int(floor(scaledGray));
    i = clamp(i, 0, numberOfColors - 2);
    
    float t = (gray - float(i) * segment) / segment;

    
	//return sineMix(rgb[i], rgb[i + 1], t, 1); //2 = easeOut
	//return smoothMix(rgb[i], rgb[i + 1], t);
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

float4 White(VertexShaderOutput input) : COLOR0
{    
    float2 coords = input.TextureCoordinates.xy;
    
    //Get the coordinates (adjusted by reps and progress)
    float x = ((coords.x * reps) + progress) % 1; //1
    float2 noisecoords = float2(x, coords.y);
    
    //Get color of trail texture at this point
    float4 in_color = tex2D(tent, noisecoords);
    
    //Take the average of the rgb values to use for gradient map
    //Doing this instead of in_color.r just incase the input texture isn't grayscale
    float avg = (in_color.r + in_color.g + in_color.b) / 3.0;
    
    //Map the color to the gradient
    float3 gradBlend = blendColors(avg);
    
    
    return float4(gradBlend * in_color.rgb * in_color.a, in_color.a * avg) * avg * input.Color.a;
    
    
    //Brightening part based off of SLR GlowingDust.fx
    
    //Brighten color if above certain threshold
    //float3 better_color = in_color.rgb * in_color.a * ColorOne + (in_color.a > glowThreshold ? ((in_color.a - glowThreshold) * glowIntensity) : float3(0, 0, 0));

    //Get average of color
    //float average = ((in_color.x + in_color.y + in_color.z) / 3.0);
    
    //float input_alpha = input.Color.a;
    
    //return float4(better_color, in_color.a * average) * average * input_alpha;
}

technique BasicColorDrawing
{
    pass DefaultPass
    {
        VertexShader = compile vs_3_0 MainVS();
    }
    pass MainPS
    {
        PixelShader = compile ps_3_0 White();
    }
};