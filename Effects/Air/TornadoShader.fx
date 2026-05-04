sampler uImage0 : register(s0);
float progress;
matrix WorldViewProjection;

float2 tex1FlowSpeed = float2(1.0, 1.0);
float2 tex1Zoom = float2(1.0, 1.0);
float3 tex1Color;
float tex1ColorMirrorMult = 0.3;

float2 tex2FlowSpeed = float2(1.0, 1.0);;
float2 tex2Zoom = float2(1.0, 1.0);
float3 tex2Color;
float tex2ColorMirrorMult = 0.3;

float fadeWidth = 0.25f;

float sineTimeSpeed = 1.0;
float sineMult = 0.15f;
float startingCurveAmount = 0.0;

struct VertexShaderInput
{
    float3 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float3 TextureCoordinates : TEXCOORD0; //float 3 b/c z is going to be the trail width 
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

texture TrailTexture1;
sampler tex1Sampler = sampler_state
{
    Texture = (TrailTexture1);
    AddressU = Wrap;
    AddressV = Wrap;
};

texture TrailTexture2;
sampler tex2Sampler = sampler_state
{
    Texture = (TrailTexture2);
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

float easeCirc(float prog)
{
    return 1.0 - sqrt(1.0 - prog * prog);
}

float4 White(VertexShaderOutput input) : COLOR0
{    
    
    //return float4(1.0, 0.0, 1.0, 1.0);
    float2 coords = input.TextureCoordinates;
    
    //Fix trail appearing jaggy 
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;

    ///Curve the shader up near the vertical edges
    float2 curvedUV = coords;
    float2 curvedUVOpposite = coords;
   
    
    float inCircMult = startingCurveAmount + (sin(progress * sineTimeSpeed) * sineMult); //0.5
    float outCircMult = startingCurveAmount + (cos(progress * sineTimeSpeed) * sineMult); // 0.5
    
    curvedUV.x += (1.0 - easeCirc(coords.y)) * 0.15 * inCircMult;
    curvedUV.x -= easeCirc(1.0 - coords.y) * 0.15 * outCircMult;

    //Curve the copy in the opposite direction
    curvedUVOpposite.x -= (1.0 - easeCirc(coords.y)) * 0.15 * outCircMult;
    curvedUVOpposite.x += easeCirc(1.0 - coords.y) * 0.15 * inCircMult;
    
    //float2 tex1UV = float2(tex1ZoomedUV.x + progress * tex1FlowSpeed.x, tex1ZoomedUV.y + progress * tex1FlowSpeed.y);
    float2 tex1UV = curvedUV * tex1Zoom + (progress * tex1FlowSpeed);
    float3 tex1Col = tex2D(tex1Sampler, tex1UV).rgb * tex1Color;
    
    //float2 tex2UV = float2(tex2ZoomedUV.x + progress * tex2FlowSpeed.x, tex2ZoomedUV.y + progress * tex2FlowSpeed.y);
    float2 tex2UV = curvedUV * tex2Zoom + (progress * tex2FlowSpeed);
    float3 tex2Col = tex2D(tex2Sampler, tex2UV).rgb * tex2Color;
    
    //Mirrored
    float2 tex1UVMirror = curvedUVOpposite * tex1Zoom + (progress * tex1FlowSpeed);
    float3 tex1ColMirror = tex2D(tex1Sampler, tex1UVMirror).rgb * tex1Color * tex1ColorMirrorMult;

    float2 tex2UVMirror = curvedUVOpposite * tex2Zoom + (progress * tex2FlowSpeed);
    float3 tex2ColMirror = tex2D(tex2Sampler, tex2UVMirror).rgb * tex2Color * tex2ColorMirrorMult;

    float3 finalCol = tex1Col + tex2Col + tex1ColMirror + tex2ColMirror;
    
    //Fade the shader in from the top and out from the bottom
    float fadeInOut = smoothstep(0.0, fadeWidth, coords.y) - smoothstep(1.0 - fadeWidth, 1.0, coords.y);

    //Remove black from result
    float finalColAVG = length(finalCol) * 1.0; //(finalCol.r + finalCol.g + finalCol.b) / 3.0;
    
    finalColAVG = smoothstep(0.0, 1.0, finalColAVG);
    
    return float4(finalCol * fadeInOut, fadeInOut) * input.Color * finalColAVG;
}

technique BasicColorDrawing
{
    pass DefaultPass
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 White();
    }
    pass AltPass
    {
        PixelShader = compile ps_3_0 White();
    }
};