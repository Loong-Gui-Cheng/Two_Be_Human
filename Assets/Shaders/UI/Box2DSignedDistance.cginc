// Signed distance to a 2D rounded box with four individual corner sizes
// p = position, b = box half width/height
// r = corner radiuses (top right, bottom right, top left, bottom left)
float sdRoundBox( in float2 p, in float2 b, in float4 r )
{
	// We choose the radius based on the quadrant we're in
    // We cap the radius based on the minimum of the box half width/height
    r.xy = (p.x>0.0)?r.xy : r.zw;
    r.x = (p.y>0.0)?r.x : r.y;
    r.x = min(2.0f*r.x, min(b.x, b.y));

    float2 q = abs(p)-b+r.x;
    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
}