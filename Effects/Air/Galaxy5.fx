sampler uImage0 : register(s0);

float uTime = 0.0;


const int iterations = 12; //12
const float formuparam2 = 0.79;
const float volsteps = 7.0;
const float stepsize = 0.290;
const float zoom = 3.0;
const float tile = 0.850;
uniform float speed2 = 0.2;
 
const float brightness = 0.0015;
const float darkmatter = 0.100;
const float distfading = 0.560;
const float saturation = 0.90;


const float transverseSpeed = 1.0; //zoom;
const float cloud = 0.17;

float tri(float x, float a)
{
    float output2 = 2.0 * abs(3.0 * ((x / a) - floor((x / a) + 0.5))) - 1.0;
    return output2;
}


const float TAU = 6.28318;
const float PI = 3.141592;
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

//GLSLs mod function is different than HLSL's fmod function
float2 glslmod(float2 ab, float y)
{
    float xA = ab.x - y * floor(ab.x / y);
    float yB = ab.y - y * floor(ab.y / y);
    return float2(xA, yB);
    
    //return x - y * floor(x / y);
}

float field(in float3 p)
{
    float strength = 7. + .03 * log(1.e-6 + frac(sin(uTime) * 373.11));
    float accum = 0.;
    float prev = 0.;
    float tw = 0.;

    for (int i = 0; i < 6; ++i)
    {
        float mag = dot(p, p);
        p = abs(p) / mag + float3(-.5, -.8 + 0.1 * sin(-uTime * 0.1 + 2.0), -1.1 + 0.3 * cos(uTime * 0.3));
        float w = exp(-float(i) / 7.);
        accum += w * exp(-strength * pow(abs(mag - prev), 2.3));
        tw += w;
        prev = mag;
    }
    return max(0., 5. * accum / tw - .7);
}

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 uv = screenSpace.xy;  
    float4 baseCol = tex2D(uImage0, screenSpace.xy);
    
    return baseCol;
    
    /*
    float time2 = uTime;
    float speed = -speed2;
    speed = .005 * cos(time2 * 0.02 + 3.1415926 / 4.0);
	//speed = 0.0;	
    float formuparam = formuparam2;
	
    	//get coords and direction	
	//float2 uv = uvs;		       
	//mouse rotation
    float a_xz = 0.9;
    float a_yz = -.6;
    float a_xy = 0.9 + uTime * 0.08;
	
    float2x2 rot_xz = float2x2(float2(cos(a_xz), sin(a_xz)), float2(-sin(a_xz), cos(a_xz)));
    float2x2 rot_yz = float2x2(float2(cos(a_yz), sin(a_yz)), float2(-sin(a_yz), cos(a_yz)));
    float2x2 rot_xy = float2x2(float2(cos(a_xy), sin(a_xy)), float2(-sin(a_xy), cos(a_xy)));
	
    float v2 = 1.0;
    float3 dir = float3(uv * zoom, 1.);
    float3 from = float3(0.0, 0.0, 0.0);
        //from.x -= 2.0*(mouse.x-0.5);
        //from.y -= 2.0*(mouse.y-0.5);

    float3 forward = float3(0., 0., 1.);
    from.x += transverseSpeed * (1.0) * cos(0.01 * uTime) + 0.001 * uTime;
    from.y += transverseSpeed * (1.0) * sin(0.01 * uTime) + 0.001 * uTime;
    from.z += 0.003 * uTime;
	
    dir.xy *= rot_xy;
    forward.xy *= rot_xy;
    dir.xz *= rot_xz;
    forward.xz *= rot_xz;
    dir.yz *= rot_yz;
    forward.yz *= rot_yz;
	
    from.xy *= -1.0 * rot_xy;
    from.xz *= rot_xz;
    from.yz *= rot_yz;
	 
	//zoom
    float zooom = (time2 - 3311.) * speed;
    from += forward * zooom;
    float sampleShift = glslmod(zooom, stepsize); //!! mod -> fmod
	 
    float zoffset = -sampleShift;
    sampleShift /= stepsize; // make from 0 to 1
	
	//volumetric rendering
    float s = 0.24;
    float s3 = s + stepsize / 2.0;
    float3 v = float3(0.);
    float t3 = 0.0;
	
    float3 backCol2 = float3(0.);
	
	//float3 p2=from+(s+zoffset)*dir;
	//return float4(p2,1.0);
	
    for (float r = 0.0; r < volsteps; r++)
    {
        float3 p2 = from + (s + zoffset) * dir; // + vec3(0.,0.,zoffset);
        float3 p3 = from + (s3 + zoffset) * dir; // + vec3(0.,0.,zoffset);
		
        p2 = abs(float3(tile) - fmod(p2, float3(tile * 2.))); // tiling fold
        p3 = abs(float3(tile) - fmod(p3, float3(tile * 2.))); // tiling fold		
		// #ifdef cloud
        t3 = field(p3);
		
        float pa, a = pa = 0.;
        for (int i = 0; i < iterations; i++)
        {
            p2 = abs(p2) / dot(p2, p2) - formuparam; // the magic formula
			//p=abs(p)/max(dot(p,p),0.005)-formuparam; // another interesting way to reduce noise
            float D = abs(length(p2) - pa); // absolute sum of average change
            a += i > 7 ? min(12., D) : D;
            pa = length(p2);
        }
		
		
		//float dm=max(0.,darkmatter-a*a*.001); //dark matter
        a *= a * a; // add contrast
		//if (r>3) fade*=1.-dm; // dark matter, don't render near
		// brightens stuff up a bit
        float s1 = s + zoffset;
		// need closed form expression for this, now that we shift samples
        float fade = pow(distfading, max(0., float(r) - sampleShift));
		//t3 += fade;		
        v += fade;
	       	//backCol2 -= fade;

		// fade out samples as they approach the camera
        if (r == 0.0)
            fade *= (1. - (sampleShift));
		// fade in samples as they approach from the distance
        if (r == volsteps - 1.0)
            fade *= sampleShift;
        v += float3(s1, s1 * s1, s1 * s1 * s1 * s1) * a * brightness * fade; // coloring based on distance
		
        backCol2 += lerp(.4, 1., v2) * float3(1.8 * t3 * t3 * t3, 1.4 * t3 * t3, t3) * fade;

		
        s += stepsize;
        s3 += stepsize;
    }
		       
		       
    v = lerp(float3(length(v)), v, saturation); //color adjust	

    float4 forCol2 = float4(v * .01, 1.);
    backCol2 *= cloud;
    backCol2.b *= 1.8;
    backCol2.r *= 0.05;
	
    backCol2.b = 0.5 * lerp(backCol2.g, backCol2.b, 0.8);
    backCol2.g = 0.0;
	//backCol2.bg = lerp(backCol2.gb, backCol2.bg, 1.0);	
	
	//backCol2 = rgb2hsv(backCol2);
	
	//backCol2.r = cloudHue;
	//backCol2 = hsv2rgb(backCol2);
	
    float4 toReturn = forCol2 + float4(backCol2, 1.0);
    
    return float4(toReturn.rgb, 1.0) * baseCol.a;
    */
}
    
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}