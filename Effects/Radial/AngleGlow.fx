//Very Largely based off of https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Effects/SpreadTelegraph.fx

sampler uImage0 : register(s0);

float totalAngle = 3.14 / 4.0;

//The opacity of the effect as a whole
float totalOpacity = 1.0;

//The opacity of the center of the effect (not the borders)
float centerOpacity = 1.0;

float3 edgeColor;
float3 centerColor;

float edgeBlendLength;
float edgeBlendStrength;

//Gets the distance of a plot from a line with a specified origin and angle
float distanceFromLine(float2 line_origin, float line_angle, float2 plot)
{
    //https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
    //If the line passes through the point P = (Px, Py) with angle theta, then the distance of some point (x0, y0) to the line is:
    // | cos(theta)(Py - y0) - sin(theta)(Px - x0) |
    return abs(cos(line_angle) * (line_origin.y - plot.y) - sin(line_angle) * (line_origin.x - plot.x));
}

float4 AngleGlow(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 baseUV = screenspace.xy;
    
    //Halfs totalAngle for calculations
    float adjustedTotalAngle = totalAngle / 2.0;
    
    //Centers our xCoordinate and mirrors the yCoordinate over the xAxis
    float2 mappedUv = float2(baseUV.x - 0.5, abs(baseUV.y - 0.5));
    
    //get the length of the doubled distance, so that 0 = at the center of the sprite and 1 = at the very edge of the circle
    float distanceFromCenter = length(mappedUv) * 2.0;
    
    //Grabs the angle (only as a positive angle, since it's a mirror image udnerneath.)
    float angle = atan2(mappedUv.y, mappedUv.x);
    
    //Grabs the distance of the point from the edge line.
    float distFromLine = distanceFromLine(float2(0, 0), adjustedTotalAngle, mappedUv);

    //If we are behind the line, just give the distance between the start point and the plot
    if (abs(angle - adjustedTotalAngle) >= 1.5707)
     	distFromLine =  length(float2(0,0) - mappedUv);
    
    //Gives 0 if we are outside the radius, and 1 if we are in it.
    float withinRadius = step(angle, adjustedTotalAngle);
    
    //Grab the colors to blend the edge lines with.
    float3 color = lerp(centerColor, edgeColor, pow(angle / adjustedTotalAngle, 9)) * withinRadius;
    float opacity = centerOpacity * pow(1 - distanceFromCenter, 2) * withinRadius;

    
    //If we are further from the line than the edge's blending length, just don't even do the blending.
    if (distFromLine > edgeBlendLength)
        return float4(color * opacity * totalOpacity, opacity * totalOpacity);
    
    //The higher this value is, the more we blend with the edge's opacity & color.
    float edgeBlendFactor = pow(1 - distFromLine / edgeBlendLength, edgeBlendStrength);
    
    color = lerp(color, edgeColor, edgeBlendFactor);
    opacity = lerp(opacity, 1 * pow(1 - distanceFromCenter, 3), edgeBlendFactor) * totalOpacity;
    
    color = color * opacity;
    return float4(color, opacity);
}

technique Technique1
{
    pass Aura 
    {
        PixelShader = compile ps_3_0 AngleGlow();
    }
}