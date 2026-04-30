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

//How much to fade from the veritcal edges
float fadeWidth = 0.25f;

//How fast the curve should shift up and down
float sineTimeSpeed = 1.0;

//How much it should curve up and down
float sineMult = 0.15f;

float startingCurveAmount = 0.0;

//The number of steps for Posterization. Set to 0 for now Posterization
float posterizationSteps = 6.0;

//How much to hue shift, if at all. Should be quite low
float hueShiftIntensity = 0.0;

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

float4 White(VertexShaderOutput input) : COLOR0
{    
    //This shader flows perpendicular to the direction of the vertex strip
    //It curves the flow near the top edges of the trail then draws a dimmer copy curved in the opposite direction to give the illusion of 3D
    
    float2 coords = input.TextureCoordinates;
    
    //Fix trail appearing when its width decreases
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;

    ///Curve the shader up near the vertical edges
    float2 curvedUV = coords;
    float2 curvedUVOpposite = coords;
    
    float inCircMult = startingCurveAmount + (sin(progress * sineTimeSpeed) * sineMult); //0.5
    float outCircMult = startingCurveAmount + (cos(progress * sineTimeSpeed) * sineMult); // 0.5
    
    curvedUV.x += (1.0 - easeCirc(coords.y)) * 0.15 * inCircMult;
    curvedUV.x -= easeCirc(1.0 - coords.y) * 0.15 * outCircMult;

    ///Curve the copy in the opposite direction
    curvedUVOpposite.x -= (1.0 - easeCirc(coords.y)) * 0.15 * outCircMult;
    curvedUVOpposite.x += easeCirc(1.0 - coords.y) * 0.15 * inCircMult;
    
    //Top layer
    float2 tex1UV = curvedUV * tex1Zoom + (progress * tex1FlowSpeed);
    float3 tex1Col = tex2D(tex1Sampler, tex1UV).rgb * tex1Color;
    
    float2 tex2UV = curvedUV * tex2Zoom + (progress * tex2FlowSpeed);
    float3 tex2Col = tex2D(tex2Sampler, tex2UV).rgb * tex2Color;
    
    //Mirrored layer
    float2 tex1UVMirror = curvedUVOpposite * tex1Zoom + (progress * tex1FlowSpeed);
    float3 tex1ColMirror = tex2D(tex1Sampler, tex1UVMirror).rgb * tex1Color * tex1ColorMirrorMult;

    float2 tex2UVMirror = curvedUVOpposite * tex2Zoom + (progress * tex2FlowSpeed);
    float3 tex2ColMirror = tex2D(tex2Sampler, tex2UVMirror).rgb * tex2Color * tex2ColorMirrorMult;

    float3 finalCol = tex1Col + tex2Col + tex1ColMirror + tex2ColMirror;

    //Posterize
    finalCol = Posterize(finalCol);
    
    ///Hue shift
    float3 hsvFinalCol = rgb2hsv(finalCol);

	//Lerp towards yellow and purple depending on brightness
    hsvFinalCol.r = lerp(hsvFinalCol.r, 0.167, hsvFinalCol.b * hueShiftIntensity);
    hsvFinalCol.r = lerp(hsvFinalCol.r, 0.75, (1.0 - hsvFinalCol.b) * hueShiftIntensity);
    
    finalCol = hsv2rgb(hsvFinalCol);
    
    ///Fade the shader in from the top and out from the bottom
    float fadeInOut = smoothstep(0.0, fadeWidth, coords.y) - smoothstep(1.0 - fadeWidth, 1.0, coords.y);

    //Remove black from result
    float finalColAVG = length(finalCol) * 1.0;
    
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