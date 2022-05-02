//vertex
#version 450

layout(set = 0, binding = 0) uniform Default 
{
	mat4 ViewProj;
	mat4 Model;
	vec4 Tint;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 3) in vec4 Color;

layout(location = 0) out vec2 inTexCoord;
layout(location = 1) out vec4 inColor;
layout(location = 2) out vec3 inNormal;
layout(location = 3) out vec3 inFragPos;

void main()
{
    gl_Position = ViewProj * Model * vec4(Position, 1);
	inFragPos = vec3(Model * vec4(Position, 1));
	inNormal = Normal;
	inTexCoord = TexCoord;
    inColor = Color * Tint;
}