//vertex
#version 450

layout(set = 0, binding = 0) uniform Default 
{
	mat4 ViewProj;
	mat4 Model;
	mat4 CubeViewProj[5];
	uint Face;
};

layout(location = 0) in vec3 Position;
layout(location = 0) out flat uint outFace;

void main()
{
	if (Face == 0)
		gl_Position = ViewProj * Model * vec4(Position, 1);
	if (Face == 1)
		gl_Position = CubeViewProj[0] * Model * vec4(Position, 1);
	if (Face == 2)
		gl_Position = CubeViewProj[1] * Model * vec4(Position, 1);
	if (Face == 3)
		gl_Position = CubeViewProj[2] * Model * vec4(Position, 1);
	if (Face == 4)
		gl_Position = CubeViewProj[3] * Model * vec4(Position, 1);
	if (Face == 5)
		gl_Position = CubeViewProj[4] * Model * vec4(Position, 1);
		
	outFace = Face;
}