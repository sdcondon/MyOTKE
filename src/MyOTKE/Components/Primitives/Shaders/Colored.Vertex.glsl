#version 330

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec4 vertexColor;
layout(location = 2) in vec3 vertexNormal_modelspace;

// Output data ; will be interpolated for each fragment.
out vec4 matColor;
out vec3 Position_worldspace;
out vec3 Normal_worldspace;
out vec3 Normal_cameraspace;
out vec3 EyeDirection_cameraspace;
out vec3 PointLightDirection_cameraspace;

// Values that stay constant for the whole mesh.
uniform vec3 PointLightPosition;
uniform mat4 M;
layout (std140) uniform Camera
{
    mat4 V;
	mat4 P;
};

void main(){

	// Output position of the vertex, in clip space : MVP * position
	gl_Position =  P * V * M * vec4(vertexPosition_modelspace, 1);
	
	// Position of the vertex, in worldspace : M * position
	Position_worldspace = (M * vec4(vertexPosition_modelspace, 1)).xyz;
	
	// Vector that goes from the vertex to the camera, in camera space.
	// In camera space, the camera is at the origin (0, 0, 0).
	vec3 vertexPosition_cameraspace = (V * M * vec4(vertexPosition_modelspace, 1)).xyz;
	EyeDirection_cameraspace = vec3(0, 0, 0) - vertexPosition_cameraspace;

	// Vector that goes from the vertex to the light, in camera space. M is omited because it's identity.
	vec3 PointLightPosition_cameraspace = (V * vec4(PointLightPosition, 1)).xyz;
	PointLightDirection_cameraspace = PointLightPosition_cameraspace + EyeDirection_cameraspace;
	
	// Normal of the the vertex, in camera space
	Normal_cameraspace = (V * M * vec4(vertexNormal_modelspace, 0)).xyz; // Only correct if M does not scale the model! Use its inverse transpose if not.

	Normal_worldspace = (M * vec4(vertexNormal_modelspace, 0)).xyz;

	// Color of the vertex. No special space for this one.
	matColor = vertexColor;
}

