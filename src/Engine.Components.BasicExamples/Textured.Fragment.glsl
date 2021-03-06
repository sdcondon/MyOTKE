#version 330

// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 Position_worldspace;
in vec3 Normal_cameraspace;
in vec3 EyeDirection_cameraspace;
in vec3 LightDirection_cameraspace;

// Ouput data
out vec4 color;

// Uniforms - values that stay constant for the whole mesh
uniform sampler2D TextureSampler;
uniform vec3 LightPosition;
uniform vec3 LightColor;
uniform float LightPower;
uniform vec3 AmbientLightColor;

void main(){

	// Material properties
	vec4 MaterialDiffuseColor = texture(TextureSampler, UV);
	vec4 MaterialSpecularColor = vec4(0.3, 0.3, 0.3, 1);

	// Distance to the light
	float distance = length(LightPosition - Position_worldspace);

	// Normal of the computed fragment, in camera space
	vec3 n = normalize( Normal_cameraspace );
	// Direction of the light (from the fragment to the light)
	vec3 l = normalize( LightDirection_cameraspace );
	// Cosine of the angle between the normal and the light direction, 
	// clamped above 0
	//  - light is at the vertical of the triangle -> 1
	//  - light is perpendicular to the triangle -> 0
	//  - light is behind the triangle -> 0
	float cosTheta = clamp( dot( n,l ), 0, 1 );
	
	// Eye vector (towards the camera)
	vec3 E = normalize(EyeDirection_cameraspace);
	// Direction in which the triangle reflects the light
	vec3 R = reflect(-l,n);
	// Cosine of the angle between the Eye vector and the Reflect vector,
	// clamped to 0
	//  - Looking into the reflection -> 1
	//  - Looking elsewhere -> < 1
	float cosAlpha = clamp( dot( E,R ), 0, 1 );
	
	color = 
		// Ambient : simulates indirect lighting
		MaterialDiffuseColor * vec4(AmbientLightColor, 1) +
		// Diffuse : "color" of the object
		MaterialDiffuseColor * vec4(LightColor, 1) * LightPower * cosTheta / (distance*distance) +
		// Specular : reflective highlight, like a mirror
		MaterialSpecularColor * vec4(LightColor, 1) * LightPower * pow(cosAlpha,5) / (distance*distance);

}