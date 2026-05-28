sampler uImage0 : register(s0);

float progress = 0.0;

float NUM_LAYERS = 8.0;

float Velocity = 0.025; //modified value to increse or decrease speed, negative value travel backwards
float StarGlow = 0.025;
float StarSize = 2.0;
float Zoom = 20.0;

float3 color1;
float3 color2;

const float TAU = 6.28318;
const float PI = 3.141592;
float Star(float2 uv, float flare)
{
    float d = length(uv);
    float m = sin(StarGlow * 1.2) / d;
    float rays = max(0., .5 - abs(uv.x * uv.y * 1000.));
    m += (rays * flare) * 2.;
    m *= smoothstep(1., .1, d);
    return m;
}

float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}


float3 StarLayer(float2 uv)
{
    float3 col = float3(0.0, 0.0, 0.0);
    float2 gv = frac(uv);
    float2 id = floor(uv);
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 offs = float2(x, y);
            float n = Hash21(id + offs);
            float size = frac(n);
            float star = Star(gv - offs - float2(n, frac(n * 34.)) + .5, smoothstep(.1, .9, size) * .46);
            float3 color = sin(float3(.2, .3, .9) * frac(n * 2345.2) * TAU) * .25 + .75;
            color = color * float3(color2.r, color2.g, color2.b + size); //float3(.9,.59,.9+size);
            star *= sin(progress * .6 + n * TAU) * .5 + .5;
            col += star * size * color;
        }
    }
    return col;
}

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 uv = screenSpace.xy;
    float4 baseCol = tex2D(uImage0, uv);
    	
    float2 M = float2(0.0, 0.0);
    M -= float2(M.x + sin(progress * 0.22), M.y - cos(progress * 0.22));
    //M +=(iMouse.xy-iResolution.xy*.5)/iResolution.y;
    float t = progress * Velocity;
    float3 col = float3(0.0, 0.0, 0.0);
    for (float i = 0.; i < 1.; i += 1. / NUM_LAYERS)
    {
        float depth = frac(i + t);
        float scale = lerp(Zoom, .5, depth);
        float fade = depth * smoothstep(1., .9, depth);
        col += StarLayer(uv * scale + i * 453.2 - progress * .05 + M) * fade;
    }
    float4 toReturn = float4(col, 1.0);
	
	
    return toReturn * baseCol.a;
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}