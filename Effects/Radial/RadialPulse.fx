sampler uImage0 : register(s0);
float uTime : register(c0);

//The progress of the effect (0 to 1)
float progress = 0.0;

////Ring shape values
// Radius of the ring at begining and end
float ringRadiusStart = 0.0;

//How thick the ring is at the start 
float ringThicknessStart = 0.1;

//How soft the ring is (bigger == sharper cutoff)
float ringPower = 1.0;

//Intensity of the ring
float ringMult = 1.0;

//Speed of the ring's wave
float ringWaveSpeed = 0.2;
float ringWaveStrength = 1.0;
float ringWaveLength = 21; //30


float fadeStrength = 0.0;

////Caustic Effect
//How zoomed into our texture the effect should be. Should generally be constant
float zoom = 1.0;

//How fast our effect should flow outward. (Negative values flow inward)
float flowSpeed = 0.0;

//The colors of the gradient
float3 gradColors[10];

//The number of colors in the gradient
float numberOfColors = 5;

//Intensity of the end result
float finalColIntensity = 1.0;

//Total alpha of the effect
float totalAlpha = 1.0;


float posterizationSteps = 0.0;

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


const float TwoPi = 6.283185;

float EaseOutQuadratic(float x)
{
    return 1.0 - pow(1.0 - x, 2.0);
}

float EaseOutCubic(float x)
{
    return 1.0 - pow(1.0 - x, 3.0);
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

float4 RadialScrollGradient(float4 screenspace : TEXCOORD0) : COLOR0
{   
    float2 baseUV = screenspace.xy;
    
    float fade = EaseOutQuadratic(progress);
    
    
    ///Ring Shape
    //Increase radius and decrease thickness as effect progresses
    float ringRadius = lerp(ringRadiusStart, 0.4, EaseOutCubic(progress));
    float ringThickness = lerp(ringThicknessStart, 0, EaseOutQuadratic(progress));
    
    float ringDist = distance(baseUV, float2(0.5, 0.5));
    
    //Distort the ring
    float2 ringUV = baseUV + float2(0.0, uTime) * ringWaveSpeed;
    ringDist += (0.01 * ringWaveStrength) * (cos(ringUV.x * ringWaveLength) + sin(ringUV.y * ringWaveLength));
    
    float ring = 1.0 - pow(saturate(abs(ringDist - ringRadius) / ringThickness), ringPower);
    
    ring *= ringMult;
    
    ///Polar coord flow
    ///Center our UVs and zoom them;
    float2 centeredUV = baseUV - 0.5;
    centeredUV *= zoom;
   
  	///Convert UVs to polar coordinates
  	//Find the angle and distance of our UVs from center
    float angle = atan2(centeredUV.y, centeredUV.x) / TwoPi;
    float dist = length(centeredUV);
    
    //(r, theta)
    float2 polarCoords = float2(dist - ringRadius, angle);
    
    ///Flow the effect by increasing the radius
    polarCoords.x -= flowSpeed * uTime * 0.1;
    polarCoords.x -= ringRadius * zoom;
    
    //Sample caustic texture at the polar coords
    float3 causCol = tex2D(causticTex, polarCoords);
    
    //Limit effect to only be on ring
    causCol *= ring;
    
    ///Get gradient color
    //Grayscale value
    float avg = (causCol.r + causCol.g + causCol.b) / 3.0;
    float3 gradCol = blendColors(avg * totalAlpha);
    
    gradCol = Posterize(gradCol);
    
    
    float zFade = 1.0 - clamp(baseUV.x * fadeStrength, 0.0, 1.0);
    
    
    float finalColAVG = length(gradCol);
    finalColAVG = smoothstep(0.0, 1.0, finalColAVG);

    
    //return float4(gradCol * finalColIntensity, totalAlpha * smoothstep(0.0, 1.0, length(causCol))) * zFade;
    
    return float4(gradCol * finalColIntensity, totalAlpha * finalColAVG) * zFade;
}

technique Technique1
{
    pass Aura 
    {
        PixelShader = compile ps_3_0 RadialScrollGradient();
    }
}