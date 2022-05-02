//fragment
#version 450

layout(set = 0, binding = 1) uniform texture2D Texture1;
layout(set = 0, binding = 2) uniform sampler Texture1Sampler;

layout(location = 0) in vec2 inTexCoord;
layout(location = 1) in vec4 inColor;
layout(location = 0) out vec4 outColor;

void main()
{
	float NEAR = 0.001;
	float FAR = 1000.0;
	float s = texture(sampler2D(Texture1, Texture1Sampler), inTexCoord).r;
	float r = (2.0 * NEAR) / (FAR + NEAR - s * (FAR - NEAR));
	outColor = vec4(r, r, r, 1);
}