#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

// Example of a standard way of doing opaque shading with shadows.

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;

uniform mat4 modelMatrix;

#include vertexflagbits.ash
#include vertexwarp.vsh

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
  vec4 worldPos = modelMatrix * vec4(pointMid, 1.0);
  gl_Position = perspectiveViewMatrix * worldPos;
}