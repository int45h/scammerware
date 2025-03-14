shader_type spatial;
render_mode unshaded;

uniform float squashFactor: hint_range(0.8,1.1,0.01) = 1.0f;
uniform sampler2D _tex  : source_color, filter_nearest;
uniform sampler2D _tex2 : source_color, filter_nearest;

uniform int fivetailMode = 0;

void vertex() {
	VERTEX.y *= squashFactor;
}

void fragment() {
	vec4 c = (fivetailMode == 0)
		? texture(_tex, UV)
		: texture(_tex2, UV);

	ALBEDO = c.xyz;
	ALPHA = c.w;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
