//fragment
#version 450

layout(set = 0, binding = 1) uniform texture2D Texture1;
layout(set = 0, binding = 2) uniform sampler Texture1Sampler;

layout(set = 1, binding = 0) uniform UserDefined 
{
	vec4 AmbientStrength;
	vec4 AmbientColor;
	vec4 LightPos;
};

layout(location = 0) in vec2 inTexCoord;
layout(location = 1) in vec4 inColor;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec3 inFragPos;
layout(location = 0) out vec4 outColor;

void main()
{
	vec3 ambient = AmbientColor.rgb * AmbientStrength.r;
	
	//Normals are assumed to be normalized CPU side.
	vec3 normal = inNormal;
	vec3 lightDir = normalize(LightPos.xyz - inFragPos);
	
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * AmbientColor.rgb;
	
    outColor = texture(sampler2D(Texture1, Texture1Sampler), inTexCoord) * vec4(diffuse + ambient, 1) * inColor;
}