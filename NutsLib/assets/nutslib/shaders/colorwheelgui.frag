#version 330 core

in vec2 uv;

uniform sampler2D tex2d;
uniform vec4 color = vec4(1.0);
uniform float hue; // Hue between 1 and 360.

out vec4 fragColor;

float median(float r, float g, float b) {
  return max(min(r, g), min(max(r, g), b));
}

float map(float value, float originalMin, float originalMax, float newMin,
          float newMax) {
  return (value - originalMin) / (originalMax - originalMin) *
             (newMax - newMin) +
         newMin;
}

vec3 hsvToRgb(vec3 hsv) {
  float h = hsv.x;
  float s = hsv.y;
  float v = hsv.z;

  if (s == 0.0) {
    return vec3(v);
  }

  float c = v * s;
  float x = c * (1.0 - abs(mod(h / 60.0, 2.0) - 1.0));
  float m = v - c;

  vec3 rgb;

  if (h >= 0.0 && h < 60.0) {
    rgb = vec3(c, x, 0.0);
  } else if (h >= 60.0 && h < 120.0) {
    rgb = vec3(x, c, 0.0);
  } else if (h >= 120.0 && h < 180.0) {
    rgb = vec3(0.0, c, x);
  } else if (h >= 180.0 && h < 240.0) {
    rgb = vec3(0.0, x, c);
  } else if (h >= 240.0 && h < 300.0) {
    rgb = vec3(x, 0.0, c);
  } else {
    rgb = vec3(c, 0.0, x);
  }

  return rgb +
         vec3(m); // Add m to each color component to match the final RGB value.
}

void main() {
  float sat = uv.x;
  float value = 1 - uv.y;

  fragColor = vec4(hsvToRgb(vec3(hue, sat, value)), 1.0);
}