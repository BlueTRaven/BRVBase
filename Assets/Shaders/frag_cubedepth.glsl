//fragment
#version 450

layout(location = 0) in flat uint outFace;

layout(location = 0) out vec4 outColor0;
layout(location = 1) out vec4 outColor1;
layout(location = 2) out vec4 outColor2;
layout(location = 3) out vec4 outColor3;
layout(location = 4) out vec4 outColor4;
layout(location = 5) out vec4 outColor5;

void main()
{
	if (outFace == 0)
		outColor0 = gl_FragCoord.zzzz;
	if (outFace == 1)
		outColor1 = gl_FragCoord.zzzz;
	if (outFace == 2)
		outColor2 = gl_FragCoord.zzzz;
	if (outFace == 3)
		outColor3 = gl_FragCoord.zzzz;
	if (outFace == 4)
		outColor4 = gl_FragCoord.zzzz;
	if (outFace == 5)
		outColor5 = gl_FragCoord.zzzz;
}