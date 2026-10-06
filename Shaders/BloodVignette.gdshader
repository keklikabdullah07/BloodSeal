shader_type canvas_item;

uniform float vignette_intensity : hint_range(0.0, 1.0) = 0.55;
uniform vec4 blood_color : source_color = vec4(0.75, 0.05, 0.1, 1.0);
uniform float pulse_speed : hint_range(0.0, 10.0) = 4.5;

void fragment() {
    vec2 uv = UV - vec2(0.5);
    float dist = length(uv) * 1.414;
    float pulse = sin(TIME * pulse_speed) * 0.15 + 0.85;
    float alpha = smoothstep(0.4, 0.95, dist) * vignette_intensity * pulse;
    COLOR = vec4(blood_color.rgb, alpha);
}
