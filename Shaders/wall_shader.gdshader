shader_type spatial;
render_mode unshaded;

uniform sampler2D tex : hint_screen_texture;
uniform float timestamp = 0.0f;

vec3 hash33(vec3 p)
{
	uvec3 q = uvec3(ivec3(p)) * uvec3(1597334673U, 3812015801U, 2798796415U);
	q = (q.x ^ q.y ^ q.z)*uvec3(1597334673U, 3812015801U, 2798796415U);
	return vec3(q) * 2.328306437080797e-10;
}

void fragment()
{
	vec2 uv = SCREEN_UV*vec2(450);
	vec3 color = hash33(vec3(uv, mod(TIME*256.,6400.)));

    ALBEDO = vec3(dot(vec3(.299,.584,.119),.5*color));
}