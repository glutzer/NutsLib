#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;

struct ParticleInstance {
  vec3 position;
  vec4 color;
  vec4 light;
  vec2 scaleRot;
};

layout(std140) uniform billboardParticles { ParticleInstance instances[900]; };

layout(std140, binding = 1) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

out vec2 uv;
out vec4 colorOut;

uniform vec4 rgbaFogIn;
out vec4 rgbaFog;

// Glow value from 0-255;
uniform int glowAmount = 0;

uniform vec3 rgbaAmbientIn;
uniform float fogMinIn;
uniform float fogDensityIn;
out float fogAmount;

#include vertexflagbits.ash
#include shadowcoords.vsh
#include fogandlight.vsh

mat3 rotateZ(float radians) {
  float c = cos(radians);
  float s = sin(radians);

  return mat3(c, -s, 0.0, s, c, 0.0, 0.0, 0.0, 1.0);
}

void main() {
  vec3 vert = vertexIn * instances[gl_InstanceID].scaleRot.x;
  vert = rotateZ(instances[gl_InstanceID].scaleRot.y) * vert;

  mat4 billboardView = viewMatrix;
  vec3 offset = instances[gl_InstanceID].position;

  // Bake the offset into the view matrix translation.
  billboardView[3] = billboardView * vec4(offset, 1.0);

  // Billboard.
  billboardView[0].xyz = vec3(1.0, 0.0, 0.0);
  billboardView[1].xyz =
      vec3(0.0, 1.0, 0.0); // Disable [1] to not billboard vertically.
  billboardView[2].xyz = vec3(0.0, 0.0, 1.0);

  vec4 worldSpace = billboardView * vec4(vert, 1.0);
  gl_Position = perspectiveMatrix * worldSpace;

  uv = uvIn;
  colorOut = instances[gl_InstanceID].color *
             applyLight(rgbaAmbientIn, instances[gl_InstanceID].light,
                        glowAmount, worldSpace);

  // Render flag is here replaced with glowAmount, since that's the first 8 bits
  // of the flags.

  // Calc shadow map coordinates from the frag shader.
  vec4 worldPos = vec4(vert, 1.0);
  calcShadowMapCoords(billboardView,
                      vec4(worldPos.x + offset.x, worldPos.y + offset.y,
                           worldPos.z + offset.z, worldPos.w));

  rgbaFog = rgbaFogIn;
  fogAmount = getFogLevel(vec4(offset, 0), fogMinIn, fogDensityIn);
}