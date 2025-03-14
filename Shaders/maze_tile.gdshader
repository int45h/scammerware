shader_type canvas_item;
uniform int maze_size = 5;
uniform int maze_size_px = 16;
uniform int tiles[225];

varying flat int instance_id;

vec2 get_offset_with_index(int idx)
{
	return vec2(
		float(idx &3),
		float(idx>>2)
	);
}

vec2 get_offset(int _id)
{
	int idx = tiles[_id] & 15;
	return get_offset_with_index(idx);
}


void vertex() { instance_id = INSTANCE_ID; }
void fragment()
{
	vec2 uv = vec2(UV.x, 1.-UV.y);
	//vec2 offset = get_offset_with_index(int(floor(TIME)) % 16);
	vec2 offset = get_offset(instance_id);
	COLOR = texture(TEXTURE, ((uv+offset)/4.0f));
}