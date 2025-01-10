#version 330 core

in vec2 uv;

uniform sampler2D tex2d;
uniform vec4 color = vec4(1.0);
uniform float time;
uniform float progress;

out vec4 fragColor;

#include noise3d.ash

float median(float r, float g, float b) {
  return max(min(r, g), min(max(r, g), b));
}

float map(float value, float originalMin, float originalMax, float newMin,
          float newMax) {
  return (value - originalMin) / (originalMax - originalMin) *
             (newMax - newMin) +
         newMin;
}

// Vertical status bar.
void main() {
  // Increase time for frequency.
  float noise = gnoise(vec3(uv.x, uv.y * 5.0, time));

  fragColor = color;

  // Darker in some spots.
  fragColor.rgb *= 0.75 + noise / 4.0;

  float alpha =
      1.0 - smoothstep(progress - 0.02, progress + 0.02, uv.y + noise / 30.0);

  fragColor.a = alpha;
}