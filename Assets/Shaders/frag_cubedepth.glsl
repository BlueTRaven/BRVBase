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
	{
		outColor0 = gl_FragCoord.zzzz;
		outColor1 = vec4(0);
		outColor2 = vec4(0);
		outColor3 = vec4(0);
		outColor4 = vec4(0);
		outColor5 = vec4(0);
	}
	if (outFace == 1)
	{
		outColor0 = vec4(0);
		outColor1 = gl_FragCoord.zzzz;
		outColor2 = vec4(0);
		outColor3 = vec4(0);
		outColor4 = vec4(0);
		outColor5 = vec4(0);
	}
	if (outFace == 2)
	{
		outColor0 = vec4(0);
		outColor1 = vec4(0);
		outColor2 = gl_FragCoord.zzzz;
		outColor3 = vec4(0);
		outColor4 = vec4(0);
		outColor5 = vec4(0);
	}
	if (outFace == 3)
	{
		outColor0 = vec4(0);
		outColor1 = vec4(0);
		outColor2 = vec4(0);
		outColor3 = gl_FragCoord.zzzz;
		outColor4 = vec4(0);
		outColor5 = vec4(0);
	}
	if (outFace == 4)
	{
		outColor0 = vec4(0);
		outColor1 = vec4(0);
		outColor2 = vec4(0);
		outColor3 = vec4(0);
		outColor4 = gl_FragCoord.zzzz;
		outColor5 = vec4(0);
	}
	if (outFace == 5)
	{
		outColor0 = vec4(0);
		outColor1 = vec4(0);
		outColor2 = vec4(0);
		outColor3 = vec4(0);
		outColor4 = vec4(0);
		outColor5 = gl_FragCoord.zzzz;
	}
}