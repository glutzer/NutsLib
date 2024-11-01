#version 330 core

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;
layout(location = 2) in vec4 colorIn;

uniform mat4 modelMatrix;
uniform mat4 projectionMatrix;

out vec2 uv;
out vec4 color;

void main() {
  gl_Position = projectionMatrix * modelMatrix * vec4(vertexIn, 1.0);

  uv = uvIn;
  color = colorIn;
}