shader_type canvas_item;
uniform float progress: hint_range(0.0f, 1.0f, 0.01f) = 0.5f;

void fragment() 
{
	float m = smoothstep(progress, progress + .001, UV.x);
	vec3 color = mix(vec3(0,1,0),vec3(1,0,0),m);
	COLOR = vec4(color, 1.0f);
}
