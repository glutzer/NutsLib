#version 330 core

in vec2 uv;

uniform int shaderType;
uniform sampler2D tex2d;
uniform vec4 color = vec4(1.0);
uniform vec4 fontColor = vec4(1.0);

// 9 slice stuff.
uniform vec4 dimensions;
uniform vec4 border;
uniform vec2 centerScale;
uniform int bold;

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

float processAxis(float coord, vec2 textureBorder, vec2 windowBorder,
                  float scale) {
  // Before.
  if (coord < windowBorder.x)
    return map(coord, 0, windowBorder.x, 0, textureBorder.x);

  // Middle.
  if (coord < 1 - windowBorder.y) {
    float mappedValue = map(coord, windowBorder.x, 1 - windowBorder.y,
                            textureBorder.x, 1 - textureBorder.y);

    float dist = (mappedValue - textureBorder.x) * scale;

    dist = mod(dist, 1 - (textureBorder.x + textureBorder.y));

    return textureBorder.x + dist;
  }

  // After.
  return map(coord, 1 - windowBorder.y, 1, 1 - textureBorder.y, 1);
}

vec4 do9Slice() {
  vec2 newUV =
      vec2(processAxis(uv.x, vec2(border.x, border.z),
                       vec2(dimensions.x, dimensions.z), centerScale.x),
           processAxis(uv.y, vec2(border.y, border.w),
                       vec2(dimensions.y, dimensions.w), centerScale.y));

  return texture(tex2d, newUV);
}

void main() {
  if (shaderType == 0) { // Regular ui texture.
    fragColor = texture(tex2d, uv) * color;
  } else if (shaderType == 1) { // 9-slice ui texture.
    fragColor = do9Slice() * color;
  } else if (shaderType == 2) { // Msdf font.

    float sdPower = 0.5;
    if (bold == 1) {
      sdPower -= 0.1;
    }

    // 1 dist = in middle. 0 dist = edge.
    vec3 msd = texture(tex2d, uv).rgb;
    float dist = median(msd.r, msd.g, msd.b) - sdPower;

    float pxDist = screenPxRange() * dist;

    float edgeSmoothness = 0.1; // Adjust this for more/less smoothing.
    float smoothOpacity = smoothstep(0.0, edgeSmoothness, pxDist);

    float opacity = clamp(pxDist + (1.0 - sdPower), 0.0, 1.0);
    fragColor = vec4(fontColor.xyz, opacity * fontColor.w);
  }
}