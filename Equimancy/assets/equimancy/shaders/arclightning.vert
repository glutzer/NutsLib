#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;

uniform mat4 modelMatrix;

layout(std140, binding = 1) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

out vec2 uv;

void main() {
  gl_Position =
      perspectiveMatrix * viewMatrix * modelMatrix * vec4(vertexIn, 1.0);
  uv = uvIn;
}