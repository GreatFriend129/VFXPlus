sampler2D uImage0 : register(s0);

float progress = 0.0;
float zoom1 = 1.0;
float zoom2 = 1.0;
float zoom3 = 1.0;

float2 offset;

float colorIntensity1 = 1.0;
float colorIntensity3 = 1.0;
float colorIntensity2 = 1.0;

float scanlineIntensity = 1.0;

float curvePower = 1.0;

int scanLineCount = 400;

bool reverse = false;

texture Texture1;
sampler tex1Sampler = sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    Texture = (Texture1);
    AddressU = Wrap;
    AddressV = Wrap;
};

texture Texture2;
sampler tex2Sampler = sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    Texture = (Texture2);
    AddressU = Wrap;
    AddressV = Wrap;
};

texture Texture3;
sampler tex3Sampler = sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    Texture = (Texture3);
    AddressU = Wrap;
    AddressV = Wrap;
};

texture Texture4;
sampler tex4Sampler = sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    Texture = (Texture4);
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 PixelShaderFunction(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 baseUV = screenspace.xy;
    float4 baseCol = tex2D(uImage0, baseUV);
    
    float2 curvedUV = baseUV - 0.25;
    curvedUV.x *= 1.7778;

    
    if (reverse)
        curvedUV = normalize(curvedUV) * tan(asin(length(curvedUV) * curvePower));
	else
        curvedUV = normalize(curvedUV) * atan(sin(length(curvedUV) * curvePower));
    
    //curvedUV.x += progress * 0.1;
    
    float2 curvedUV1 = curvedUV + float2(sin(progress * 0.1), -cos(progress * 0.04));
    float2 curvedUV2 = curvedUV + float2(0.2, 0.7) + float2(sin(progress * 0.06), -cos(progress * 0.075));
    float2 curvedUV3 = curvedUV + float2(0.3, 0.4) + float2(sin(progress * 0.08), -cos(progress * 0.03));
    float2 curvedUV4 = curvedUV + float2(0.5, 0.8) + float2(sin(progress * 0.04), -cos(progress * 0.035));

    float4 toReturn = tex2D(tex1Sampler, curvedUV1 * zoom1) * colorIntensity1;
    toReturn += tex2D(tex2Sampler, curvedUV2 * zoom2) * colorIntensity2;
    toReturn += tex2D(tex3Sampler, curvedUV3 * zoom3) * colorIntensity3;
    //toReturn += tex2D(tex4Sampler, curvedUV4 * zoom3) * colorIntensity3; //Both star layers should use the same zoom and intensity



    float scanline = fmod(floor(baseUV.y * scanLineCount), 2.0);
    toReturn.rgb -= scanline * scanlineIntensity;
    
    return float4(toReturn.rgb, 1.0) * baseCol.a;
    
    //return float4(toReturn.rgb * colorIntensity, 1.0);

}

technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
