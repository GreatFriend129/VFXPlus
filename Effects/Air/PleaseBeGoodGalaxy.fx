sampler2D uImage0 : register(s0);

float uTime = 0.0;
float zoom = 1.0;

float density = 1.3;


const float4 background_color = float4(0.02, 0.04, 0.12, 1.0);
const float aspect_ratio = 1.0;
const int layer_parascale = 4;
const float2 star_speed = float2(0.0, 0.0);
const float2 star_wave = float2(0.0, 0.0);
const float star_size = 3.0;
const float star_rotate_speed = 0.5;
const float twinkle_effect = 0.6;
const float twinkle_speed = 0.3;
const bool pixelate_enabled = false;
const float pixelate_count = 1000.0;

const float PI = 3.141592;
float one_div_x(float x)
{
    return (abs(x) < 0.0001) ? 1.0 : (1.0 / x);
}

float one_div_x2(float x)
{
    return one_div_x(x) * one_div_x(x);
}

float get_beta_w(float x, float f, float size)
{
    return size * x * PI / f;
}

float get_beta_h(float y, float f, float size)
{
    return size * y * PI / f;
}

float get_i(float2 uv, float f, float2 size)
{
    return one_div_x2(get_beta_w(uv.x, f, size.x)) * one_div_x2(get_beta_h(uv.y, f, size.y));
}

float random(float2 st)
{
    return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43759.5453123);
}

float2 get_snowflake_world_center(float2 grid_id, float2 time_offset, float2 snow_offset, float layer_scale)
{
    return (grid_id + snow_offset + time_offset) / layer_scale;
}

float2 rotate(float2 uv, float add_theta)
{
    float theta = atan2(uv.y, uv.x) + add_theta;
    float r = length(uv);
    return float2(r * cos(theta), r * sin(theta));
}

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 uv = screenSpace.xy;    
    float4 baseCol = tex2D(uImage0, screenSpace.xy);

    
    uv *= zoom;
    uv.x *= aspect_ratio;
    
    float4 storedBaseCol = baseCol;
    
    float2 cuv = (uv - 0.5) * 2.0;
	
    for (int layer = 0; layer < layer_parascale; layer++)
    {
        float layer_scale = exp(float(layer + 1) * density);
        float2 layer_speed = float2(star_speed.x, star_speed.y) * (1.0 + float(layer) * 0.3);
        float layer_size = star_size * (1.0 - float(layer) * 0.2);

        float2 layer_st = uv * layer_scale;
        float2 cuv_st = cuv;

        float2 time_offset = uTime * layer_speed;
        layer_st -= time_offset;

        float2 grid_st = frac(layer_st);
        float2 grid_id = floor(layer_st);

        float rand_seed = random(grid_id);

        float2 snow_pos = float2(
			0.5 + (0.3 * sin((rand_seed * 6.28) + (uTime * star_wave.x))),
			0.5 + (0.2 * cos((rand_seed * 12.56) + (uTime * star_wave.y)))
		);

        float dist = distance(grid_st, snow_pos);
        float snow_size = layer_size * 0.01 * (0.5 + 0.5 * rand_seed);
        float brightness = 1.0 - (float(layer) * 0.3);

        float m = exp((-dist * dist) / (snow_size * snow_size));

        float2 fst = cuv_st - (get_snowflake_world_center(grid_id, time_offset, snow_pos, layer_scale) - 0.5) * 2.0;
        fst = rotate(fst, uTime * star_rotate_speed);

        float snowflake = m * 0.5 * (get_i(fst, 0.8 - (dist * 42.0), float2(1.0 / snow_size, 1.0 / snow_size)) + 1.0);

		// Twinkle effect
        float twinkle = (1.0 - twinkle_effect) + twinkle_effect * (sin(rand_seed * 100.0 + uTime * twinkle_speed) * cos(rand_seed * 120.0 + uTime * (twinkle_speed + 2.0)));
        snowflake *= twinkle;

        float3 color_type = float3(random(grid_id - 3.0), random(grid_id + 7.0), random(grid_id + 5.0));

        baseCol = max(baseCol, baseCol + ((snowflake * brightness) * float4(color_type, 1.0)));
    }
	    
    return float4(baseCol.rgb, 1.0) * storedBaseCol.a;
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}