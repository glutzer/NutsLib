#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;
layout(location = 2) in vec4 colorIn;

uniform mat4 modelMatrix;
uniform float italicSlant;

layout(std140) uniform guiTransforms {
  mat4 transform;
  int doTrans;
};

layout(std140, binding = 1) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

uniform int removeDepth;

out vec2 uv;
out vec4 color;

void main() {
  vec3 vert = vertexIn;

  // For italics, slant the quad.
  if (italicSlant > 0 && gl_VertexID > 1) {
    vert.x += italicSlant;
  }

  if (doTrans == 0) {
    gl_Position = orthographicMatrix * modelMatrix * vec4(vert, 1.0);
  } else {
    vec4 animatedPos = transform * modelMatrix * vec4(vert, 1.0);
    gl_Position = orthographicMatrix * animatedPos;
  }

  if (removeDepth == 1) {
    gl_Position.z = gl_Position.w;
  }

  uv = uvIn;
  color = colorIn;
}