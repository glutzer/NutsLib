#version 330 core

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;
layout(location = 2) in vec3 normalIn;
layout(location = 3) in vec4 colorIn;

uniform mat4 modelMatrix;

layout(std140) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

out vec2 uv;
out vec4 color;

void main() {
  gl_Position =
      perspectiveMatrix * viewMatrix * modelMatrix * vec4(vertexIn, 1.0);
  uv = uvIn;
  color = colorIn;
}