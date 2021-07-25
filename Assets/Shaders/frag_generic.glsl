//fragment
#version 450

layout(set = 0, binding = 1) uniform texture2D Texture1;
layout(set = 0, binding = 2) uniform sampler Texture1Sampler;

layout(location = 0) in vec2 inTexCoord;
layout(location = 1) in vec4 inColor;
layout(location = 0) out vec4 outColor;

void main()
{
    outColor = texture(sampler2D(Texture1, Texture1Sampler), inTexCoord) * inColor;
}