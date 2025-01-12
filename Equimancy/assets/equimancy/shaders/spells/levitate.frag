#version 330 core

in vec2 uv;

uniform vec4 color;
uniform float time;

in vec3 vertex;

layout(location = 0) out vec4 outAccu;
layout(location = 1) out vec4 outReveal;
layout(location = 2) out vec4 outGlow;

#include noise3d.ash

void drawPixel(vec4 colorA) {
  float weight =
      colorA.a *
      clamp(0.03 / (1e-5 + pow(gl_FragCoord.z / 200, 4.0)), 1e-2, 3e3);

  outAccu = vec4(colorA.rgb * colorA.a, colorA.a) * weight;

  outReveal.r = colorA.a;

  float glowLevel = 1.0;

  outGlow = vec4(glowLevel, 0, 0, colorA.a);
}

void main() {
  float noise = gnoise(vertex + vec3(time));
  noise += 1.0;
  noise /= 2.0;

  vec4 noiseColor = color - vec4(noise) * 0.5;

  drawPixel(noiseColor);
}