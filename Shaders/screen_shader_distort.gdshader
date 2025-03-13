shader_type canvas_item;
uniform sampler2D tex : hint_screen_texture, filter_nearest;

#define POST_STEPS 5.
// https://www.shadertoy.com/view/XdGfRR
vec2 hash22(vec2 p)
{
	uvec2 q = uvec2(ivec2(p))*uvec2(1597334673U, 3812015801U);
	q = (q.x ^ q.y) * uvec2(1597334673U, 3812015801U);
	return vec2(q) * 2.328306437080797e-10;
}

vec3 posterize(vec3 col) { return floor(col*POST_STEPS)/POST_STEPS; }
float luma(vec3 col) { return dot(col, vec3(.299,.584,.119)); }

mat3 saturation(float s)
{
    float s_1 = 1. - s;
    vec3 l = vec3(.299,.584,.119);
    vec3 s_r = vec3(l.x*s_1)+vec3(s,0,0);
    vec3 s_g = vec3(l.y*s_1)+vec3(0,s,0);
    vec3 s_b = vec3(l.z*s_1)+vec3(0,0,s);
    
    return mat3(s_r,s_g,s_b);
}

vec2 uv_offset(vec2 p)
{
    vec2 pg = floor(p);
    vec2 pc = fract(p);
    
    return mix(
        mix(hash22(pg+vec2(0,0)),hash22(pg+vec2(1,0)),pc.x),
        mix(hash22(pg+vec2(0,1)),hash22(pg+vec2(1,1)),pc.x),
    pc.y)*2.-1.;
}

void fragment() {
	vec2 scaler = vec2(8.);
    vec2 uv2 = UV+uv_offset((UV+floor(TIME*.45*scaler))*8.)*.003;
    //vec3 col = posterize(texture(tex,uv2).rgb)*saturation(2.);
    //col *= vec3(1., .7, .65);
    
    COLOR = texture(tex, uv2);
}


