matrix WorldViewProjection;

float progress;

//How many times the texture repeats
float tex1reps = 1.0;
float tex2reps = 1.0;

float bodyPower = 4.0;
float bodyIntensity = 1.0;
float tex1Intensity = 1.0;
float tex2Intensity = 1.0;

float whiteGlowSize = 1.0;
float whiteGlowPower = 2.0;

float posterizationSteps = 0.0;

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

texture TrailTexture1;
sampler trailTex1 = sampler_state
{
    texture = <TrailTexture1>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

texture TrailTexture2;
sampler trailTex2 = sampler_state
{
    texture = <TrailTexture2>;
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
    
    float4 finalCol = float4(0.0, 0.0, 0.0, 0.0);
    
    //0 if on center line, 0.5 if as far away as possible
    float verticalDistFromCenter = abs(baseCoords.y - 0.5);
    
    //The closer to the center, the more white it is
    float whiteIntensity = pow((0.05 * whiteGlowSize) / (verticalDistFromCenter * 1.5), whiteGlowPower);

    float3 beamCol = input.Color.rgb;
    beamCol += whiteIntensity * length(beamCol);
    
    beamCol *= pow(smoothstep(0.5, 0.0, verticalDistFromCenter), bodyPower); //4
    
    float3 trail1 = tex2D(trailTex1, float2((baseCoords.x * tex1reps) - progress, baseCoords.y)).rgb * beamCol;
    float3 trail2 = tex2D(trailTex2, float2((baseCoords.x * tex2reps) - progress, baseCoords.y)).rgb * beamCol;
    
    float3 combinedTrail = (trail1 * tex1Intensity) + (trail2 * tex2Intensity);

    float3 toPosterize = (beamCol.rgb * bodyIntensity) + combinedTrail.rgb;
    toPosterize = Posterize(toPosterize);
    
   
    float brightness = dot(toPosterize.rgb, float3(0.2126, 0.7152, 0.0722));
    brightness = smoothstep(0.0, 1.0, brightness);
    
    float4 toReturn = float4(toPosterize, input.Color.a) * brightness;
    return toReturn;
    
    /*
    float2 baseCoords = input.TextureCoordinates.xy;
    
    //Fix trail appearing jaggy if the width changes with trail progress
    baseCoords.y = (baseCoords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    //0 if on center line, 0.5 if as far away as possible
    float verticalDistFromCenter = abs(baseCoords.y - 0.5);
    
    //The closer to the center, the more white it is
    float whiteIntensity = pow((0.05 * whiteGlowSize) / (verticalDistFromCenter * 1.5), whiteGlowPower);

    float4 beamCol = input.Color;
    beamCol += whiteIntensity * length(beamCol);
    
    beamCol *= pow(smoothstep(0.5, 0.0, verticalDistFromCenter), bodyPower); //4
    
    float4 trail1 = tex2D(trailTex1, float2((baseCoords.x * tex1reps) - progress, baseCoords.y)) * beamCol;
    float4 trail2 = tex2D(trailTex2, float2((baseCoords.x * tex2reps) - progress, baseCoords.y)) * beamCol;
    
    float4 combinedTrail = (trail1 * tex1Intensity) + (trail2 * tex2Intensity);

    float3 toPosterize = (beamCol.rgb * bodyIntensity) + combinedTrail.rgb;
    toPosterize = Posterize(toPosterize);
    
    
    //Remove black from result
    float finalColAVG = length(toPosterize) / 3.0;
    
    //if (finalColAVG < 0.75)
    //    finalColAVG = 0.0;
    
    float4 toReturn = float4(toPosterize, input.Color.a) * finalColAVG;
    return toReturn;
    */
}

technique BasicColorDrawing
{
    pass DefaultPass
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};