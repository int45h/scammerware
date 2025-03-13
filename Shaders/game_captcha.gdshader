shader_type canvas_item;
#define PI 3.14159265359

uniform float _t : hint_range(0,1) = 0.0f;

float hash12(vec2 p)
{
	uvec2 q = uvec2(ivec2(p)) * uvec2(1597334673U, 3812015801U);
	uint n = (q.x ^ q.y) * 1597334673U;
	return float(n) * 2.328306437080797e-10;
}

vec2 dr(float t)
{
    t = t * 2. * PI;
    return vec2(cos(t), sin(t));
}

float perlin2d(vec2 p)
{
    vec2 pg = floor(p), pc = p-pg;
    vec2 pc_p = pc*pc*(3.-2.*pc);
    vec2 o = vec2(0,1);
    return mix(
        mix(dot(dr(hash12(p+o.xx)),pc-o.xx),dot(dr(hash12(p+o.yx)),pc-o.yx),pc_p.x),
        mix(dot(dr(hash12(p+o.xy)),pc-o.xy),dot(dr(hash12(p+o.yy)),pc-o.yy),pc_p.x),
        pc_p.y
    );
}

void fragment()
{
    float n = perlin2d(UV*vec2(6,4)+vec2(TIME,0));
    float t = pow(2., 1.-_t) - 1.;
    
    float f = mix(1.,0.,t);
    //n = sin(n*200.);
    vec2 uvp = UV+.08*f*n;
    COLOR = texture(TEXTURE,uvp);
}