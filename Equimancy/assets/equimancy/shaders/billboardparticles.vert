#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;

struct ParticleInstance {
  vec3 position;
  vec4 color;
  float scale;
};

layout(std140) uniform billboardParticles { ParticleInstance instances[999]; };

layout(std140, binding = 1) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

out vec2 uv;
out vec4 colorOut;

void main() {
  mat4 billboardView = viewMatrix;

  // Bake the offset into the view matrix translation.
  billboardView[3] =
      billboardView * vec4(instances[gl_InstanceID].position, 1.0);

  // Billboard.
  billboardView[0].xyz = vec3(1.0, 0.0, 0.0);
  billboardView[1].xyz =
      vec3(0.0, 1.0, 0.0); // Disable [1] to not billboard vertically.
  billboardView[2].xyz = vec3(0.0, 0.0, 1.0);

  vec4 worldSpace =
      billboardView * vec4(vertexIn * instances[gl_InstanceID].scale, 1.0);
  gl_Position = perspectiveMatrix * worldSpace;

  uv = uvIn;
  colorOut = instances[gl_InstanceID].color;
}