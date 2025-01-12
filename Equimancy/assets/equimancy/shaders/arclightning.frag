#version 330 core

in vec2 uv;

uniform sampler2D tex2d;
uniform float time;
uniform vec3 colorIn;

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

float median(float r, float g, float b) {
  return max(min(r, g), min(max(r, g), b));
}

float map(float value, float min1, float max1, float min2, float max2) {
  return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

void main() {
  float noise = gnoise(vec3(uv.x, uv.y * 4.0, time));

  vec2 movedUv = uv;
  movedUv.y += time * 3.0;

  // Map to new value to fade at top and bottom.
  float fade = smoothstep(0.85, 1.0, abs(uv.y * 2.0 - 1.0));
  movedUv.x = mix(movedUv.x, map(movedUv.x, 0.0, 1.0, -3.0, 4.0), fade);

  movedUv.x += noise * 0.5;

  movedUv.x = clamp(movedUv.x, 0.0, 1.0);

  vec4 color = texture(tex2d, movedUv);

  float value = median(color.r, color.g, color.b);

  color.rgb = mix(colorIn, vec3(1.0), value);

  drawPixel(color);
}