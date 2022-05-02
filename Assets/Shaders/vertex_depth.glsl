//vertex
#version 450

layout(set = 0, binding = 0) uniform Default 
{
	mat4 ViewProj;
	mat4 Model;
};

layout(location = 0) in vec3 Position;
layout(location = 0) out vec4 FragPos;

void main()
{
    gl_Position = ViewProj * Model * vec4(Position, 1);
}