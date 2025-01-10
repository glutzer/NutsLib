#version 330 core

in vec2 uv;

uniform sampler2D tex2d;
uniform vec4 color = vec4(1.0);

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outGlow;

void main() {
  outColor = color;
  outGlow = vec4(0, 0, 0, min(1, outColor.a));
}