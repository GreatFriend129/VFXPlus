sampler uImage0 : register(s0);
float uTime : register(c0);

//How zoomed into our texture the effect should be. Should generally be constant
float zoom = 1.0;

//How fast our effect should flow outward. (Negative values flow inward)
float flowSpeed = 0.0;

//Should probably always be between 0 and 1
float radius = 1.0;

//How much the edge of the circle should fade out. (0 = no fade)
float edgeBlendDist = 0.15;

//How much the inside of the circle should fade in (hole in center).
float insideBlendDist = 0.07;

//How much the effect should be distorted by distortTex
float distortIntensity = 0.07;

float3 inputColor;

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

const float TwoPi = 6.283185;
float4 RadialScrollOneCol(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 baseUV = screenspace.xy;
    
    ///Center our UVs and zoom them;
    float2 centeredUV = baseUV - 0.5;
    centeredUV *= zoom;
   
  	///Convert UVs to polar coordinates
  	//Find the angle and distance of our UVs from center
    float angle = atan2(centeredUV.y, centeredUV.x) / TwoPi;
    float dist = length(centeredUV);
    
    float2 polarCoords = float2(dist, angle);
    
    //Flow the effect by increasing the radius
    polarCoords.x -= flowSpeed * uTime * 0.1;
    
    
    ///Distort our new polar UVs
    float distortedUV = polarCoords + float2(uTime * 0.1, 0.0);
    
    float distortAmount = tex2D(distortTex, polarCoords + float2(uTime * -0.1, 0.0)).r;
    float2 adjustedUV = polarCoords;
    adjustedUV += distortAmount * distortIntensity * 0.5;
    adjustedUV -= distortIntensity / 2.0;
    
    
    //Sample our texture at the distorted polar coords
    float3 col = 1.0 * tex2D(causticTex, adjustedUV);
        
    float endAlpha = 1.0;
    
    //Limit our shader to a certain radius and blend the edge of it
    float zoomedRadius = radius * zoom * 0.5;
    endAlpha *= 1.0 - smoothstep(zoomedRadius, zoomedRadius + edgeBlendDist * zoom, dist);
    
    //Fade in at the center
    endAlpha *= smoothstep(0, insideBlendDist * zoom, dist);
    
    
    ///Make result depend on the alpha of the texture the shader is drawing on
    //Starting color of texture shader is being applied to
    float4 baseCol = tex2D(uImage0, baseUV);
        
    float4 toReturn = float4(col * endAlpha * inputColor * baseCol.a, endAlpha * baseCol.a);
    return toReturn;
}

technique Technique1
{
    pass Aura 
    {
        PixelShader = compile ps_3_0 RadialScrollOneCol();
    }
}