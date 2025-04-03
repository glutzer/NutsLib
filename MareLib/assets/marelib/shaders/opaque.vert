#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

// Example of a standard way of doing opaque shading with shadows.

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;
layout(location = 2) in vec3 normalIn;

uniform mat4 modelMatrix;

out vec2 uvOut;    // Uv of this vertex.
out vec4 colorOut; // Color of this vertex.
out vec4 gNormal;
out vec4 cameraPos;
out vec3 normal;

out float fogAmount;
out vec4 rgbaFog;

uniform vec3 rgbaAmbientIn;
uniform vec4 rgbaLightIn;
uniform vec4 rgbaFogIn;
uniform float fogMinIn;
uniform float fogDensityIn;

#include vertexflagbits.ash
#include shadowcoords.vsh
#include fogandlight.vsh

uniform mat4 offsetViewMatrix;

layout(std140) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
  mat4 perspectiveViewMatrix;
  float zNear;
  float zFar;
};

float catenary(float x, float d, float a) {
  return a * (cosh((x - (d / 2.0)) / a) - cosh((d / 2.0) / a));
}

mat3 rotateToVector(vec3 targetVector) {
  vec3 zAxis = normalize(targetVector);
  vec3 xAxis = normalize(cross(vec3(0.0, 1.0, 0.0), zAxis));
  vec3 yAxis = cross(zAxis, xAxis);

  return mat3(xAxis, yAxis, zAxis);
}

void main() {
  vec4 worldPos = modelMatrix * vec4(vertexIn, 1.0);
  cameraPos = offsetViewMatrix * worldPos;
  gl_Position = perspectiveMatrix * cameraPos;

  // Take the view matrix and the world pos relative to the camera, send shadow
  // info to the frag.
  calcShadowMapCoords(offsetViewMatrix, worldPos);

  // Get the fog amount at the current pos.
  fogAmount = getFogLevel(worldPos, fogMinIn, fogDensityIn);

  // Apply point lights/block light.
  colorOut = applyLight(rgbaAmbientIn, rgbaLightIn, 0, cameraPos);

  // rgbaFog for fragment include.
  rgbaFog = rgbaFogIn;

  uvOut = uvIn;

  // renderFlagsIn (vertex attribute).
  // normal = unpackNormal, but this uses it's own normal in the vertices.

  normal = normalIn;
#if SSAOLEVEL > 0
  gNormal = viewMatrix * modelMatrix * vec4(normalIn, 0);
#endif
}