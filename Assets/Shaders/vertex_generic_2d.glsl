//vertex
#version 450

layout(set = 0, binding = 0) uniform Default 
{
	mat4 ViewProj;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoord;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec2 inTexCoord;
layout(location = 1) out vec4 inColor;

void main()
{
    gl_Position = ViewProj * vec4(Position, -1, 1);
	inTexCoord = TexCoord;
    inColor = Color;
}