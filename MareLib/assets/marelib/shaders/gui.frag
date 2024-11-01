#version 330 core

in vec2 uv;

uniform int shaderType;
uniform sampler2D tex2d;
uniform vec4 color = vec4(1.0, 1.0, 1.0, 1.0);

// 9 slice stuff.
uniform vec2 dimensions;
uniform vec2 border;
uniform vec2 centerScale;

// const float smoothing = 1.0 / 16.0;
const float pxRange = 12.0;

out vec4 fragColor;

float median(float r, float g, float b) {
  return max(min(r, g), min(max(r, g), b));
}

float screenPxRange() {
  vec2 unitRange = vec2(pxRange) / vec2(textureSize(tex2d, 0));
  vec2 screenTexSize = vec2(1.0) / fwidth(uv);
  return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

float map(float value, float originalMin, float originalMax, float newMin,
          float newMax) {
  return (value - originalMin) / (originalMax - originalMin) *
             (newMax - newMin) +
         newMin;
}

float processAxis(float coord, float textureBorder, float windowBorder,
                  float scale) {
  // Before.
  if (coord < windowBorder)
    return map(coord, 0, windowBorder, 0, textureBorder);

  // Middle.
  if (coord < 1 - windowBorder) {
    float mappedValue = map(coord, windowBorder, 1 - windowBorder,
                            textureBorder, 1 - textureBorder);

    float dist = (mappedValue - textureBorder) * scale;

    dist = mod(dist, 1 - textureBorder * 2);

    return textureBorder + dist;
  }

  // After.
  return map(coord, 1 - windowBorder, 1, 1 - textureBorder, 1);
}

vec4 do9Slice() {
  vec2 newUV = vec2(processAxis(uv.x, border.x, dimensions.x, centerScale.x),
                    processAxis(uv.y, border.y, dimensions.y, centerScale.y));

  return texture(tex2d, newUV);
}

void main() {
  if (shaderType == 0) { // Regular ui texture.
    fragColor = texture(tex2d, uv) * color;
  } else if (shaderType == 1) { // 9-slice ui texture.
    fragColor = do9Slice() * color;
  } else if (shaderType == 2) { // Msdf font.
    vec3 msd = texture(tex2d, uv).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange() * (sd - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    fragColor = vec4(color.xyz, opacity * color.w);
  }
}