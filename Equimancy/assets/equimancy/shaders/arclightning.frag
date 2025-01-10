#version 330 core

in vec2 uv;

uniform sampler2D tex2d;
uniform float time;

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

  float glowLevel = 0.0;

  outGlow = vec4(glowLevel, 0, 0, colorA.a);
}

float median(float r, float g, float b) {
  return max(min(r, g), min(max(r, g), b));
}

void main() {
  float noise = gnoise(vec3(uv.x, uv.y * 4.0, time));

  vec2 movedUv = uv;

  float uvSdf = abs(uv.y * 2.0 - 1.0);
  float fade = smoothstep(0.9, 1.0, uvSdf);

  movedUv.y += time * 3.0;

  movedUv.x += noise * 0.5;

  if (movedUv.x > 0.5f) {
    movedUv.x += fade;
  } else {
    movedUv.x -= fade;
  }

  movedUv.x = clamp(movedUv.x, 0.0, 1.0);

  vec4 color = texture(tex2d, movedUv);

  float value = median(color.r, color.g, color.b);

  color.rgb = mix(vec3(0.0, 0.0, 1.0), vec3(1.0), value);

  drawPixel(color);
}