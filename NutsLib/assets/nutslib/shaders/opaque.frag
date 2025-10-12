#version 330 core

uniform sampler2D tex2d;

// Example of a standard way of doing opaque shading with shadows.

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outGlow;
#if SSAOLEVEL > 0
layout(location = 2) out vec4 outGNormal;
layout(location = 3) out vec4 outGPosition;
#endif

in vec2 uvOut;
in vec4 colorOut;
in vec4 gNormal;
in vec4 cameraPos;
in vec3 normal;

in float fogAmount;
in vec4 rgbaFog;

#include vertexflagbits.ash
#include fogandlight.fsh
#include underwatereffects.fsh

void main() {
  vec4 texColor = texture(tex2d, uvOut) * colorOut;
  outColor = texColor;

  outColor = applyFogAndShadow(texColor, fogAmount);

  float murkiness = getUnderwaterMurkiness();

  outColor.rgb = applyUnderwaterEffects(outColor.rgb, murkiness);

  outGlow = vec4(0);

#if SSAOLEVEL > 0
  outGPosition = vec4(cameraPos.xyz, fogAmount * 2 /*+ murkiness*/);
  outGNormal = gNormal;
#endif
}