#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

layout(location = 0) in vec3 vertexPositionIn;
layout(location = 1) in vec2 uvIn;
layout(location = 2) in vec4 colorIn;

layout(location = 3) in int flags;
layout(location = 4) in float damageEffectIn;
layout(location = 5) in int jointId;

layout(location = 3) in int renderFlagsIn;

layout(std140) uniform guiTransforms {
  mat4 transform;
  int doTrans;
};

layout(std140, binding = 1) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

uniform vec4 rgbaIn;
uniform vec4 rgbaGlowIn;
uniform int extraGlow;

uniform mat4 modelMatrix;
uniform int applyModelMat;
uniform int applyColor;
uniform int removeDepth;

out vec2 uv;
out vec2 uvOverlay;
out vec4 color;
out vec4 rgbaGlow;
out vec2 clipPos;
out float damageEffectV;

flat out vec3 normal;
out float normalShadeIntensity;

#include vertexflagbits.ash
#include fogandlight.vsh

void main() {
  damageEffectV = damageEffectIn;
  uv = uvIn;

  int glow = min(255, extraGlow + (renderFlagsIn & GlowLevelBitMask));

  glowLevel = glow / 255.0;
  rgbaGlow = rgbaGlowIn;

  color = rgbaIn;

  if (applyColor == 1)
    color *= colorIn;

  if (doTrans == 0) {
    gl_Position =
        orthographicMatrix * modelMatrix * vec4(vertexPositionIn, 1.0);
  } else {
    vec4 animatedPos = transform * modelMatrix * vec4(vertexPositionIn, 1.0);
    gl_Position = orthographicMatrix * animatedPos;
  }

  clipPos = gl_Position.xy;

  normal = unpackNormal(renderFlagsIn);
  if (applyModelMat > 0) {
    normal = mat3(modelMatrix) * normal;
    normal = normalize(normal);
  }

  normalShadeIntensity = 1;

  if (removeDepth == 1) {
    gl_Position.z = gl_Position.w;
  }
}