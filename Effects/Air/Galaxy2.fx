sampler uImage0 : register(s0);

float progress = 0.0;

float NUM_LAYERS = 8.0;

float Velocity = 0.025; //modified value to increse or decrease speed, negative value travel backwards
float StarGlow = 0.025;
float Zoom = 20.0;

float4 color1;
float4 color2;

float4 scrollColor1;
float4 scrollColor2;


texture ScrollTexture1;
sampler tex1Sampler = sampler_state
{
    Texture = (ScrollTexture1);
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ScrollTexture2;
sampler tex2Sampler = sampler_state
{
    Texture = (ScrollTexture2);
    AddressU = Wrap;
    AddressV = Wrap;
};

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
            float3 color = sin(float3(color1.r, color1.g, color1.b) * frac(n * 2345.2) * TAU) * .25 + .75;
            color = color * float3(color2.r, color2.g, color2.b + size); //float3(.9,.59,.9+size);
            star *= sin(progress * .6 + n * TAU) * .5 + .5;
            col += star * size * color;
        }
    }
    return col;
}

//Outline
// returns 1 if input > 0, else 0
float gtz(float input)
{
    return max(0, sign(input));
}

float check(float2 from)
{
    float2 DIRECTIONS[8];
    DIRECTIONS[0] = float2(1.0, 0.0);
    DIRECTIONS[1] = float2(0.0, 1.0);
    DIRECTIONS[2] = float2(-1.0, 0.0);
    DIRECTIONS[3] = float2(0.0, -1.0);
    DIRECTIONS[4] = float2(1.0, 1.0);
    DIRECTIONS[5] = float2(-1.0, -1.0);
    DIRECTIONS[6] = float2(-1.0, 1.0);
    DIRECTIONS[7] = float2(1.0, -1.0);
    
    float result = 0.0;
    for (int i = 0; i < 8; i++)
    {
        result += tex2D(uImage0, from + DIRECTIONS[i] * 160.0).a;
    }
    return gtz(result);
}


float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{

    
    float2 uv = screenSpace.xy;
    float4 baseCol = tex2D(uImage0, uv);
    	
    float2 M = float2(0.0, 0.0);
    M -= float2(M.x + sin(progress * 0.22), M.y - cos(progress * 0.22));
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
	
    float4 dustCol1 = tex2D(tex1Sampler, uv + M);
    float4 dustCol2 = tex2D(tex2Sampler, uv + M * 0.5);
    

    toReturn += dustCol1 * scrollColor1;
    toReturn += dustCol2 * scrollColor2;
	
    toReturn *= pow(baseCol.a, 1.0);
    
    //toReturn = lerp(toReturn, float4(1.0, 1.0, 1.0, 1.0), check(uv) * (1.0 - gtz(baseCol.a)));
    
    float width = 0.003;
    float4 outline_color = float4(1.0, 1.0, 1.0, 1.0);
    
    float4 line_neg_y = float4(outline_color.rgb, tex2D(uImage0, float2(uv.x, uv.y - width)).a);
    float4 line_pos_y = float4(outline_color.rgb, tex2D(uImage0, float2(uv.x, uv.y + width)).a);
    float4 line_neg_x = float4(outline_color.rgb, tex2D(uImage0, float2(uv.x - width, uv.y)).a);
    float4 line_pos_x = float4(outline_color.rgb, tex2D(uImage0, float2(uv.x + width, uv.y)).a);
    float4 outline_y = lerp(line_neg_y, line_pos_y, line_pos_y);
    float4 outline_x = lerp(line_neg_x, line_pos_x, line_pos_x);
    float4 outline = lerp(outline_y, outline_x, outline_x);
    
    toReturn = lerp(toReturn, outline, (1.0 - baseCol.a));
    
    return toReturn;
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}