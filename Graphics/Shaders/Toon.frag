#version 330

in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;
uniform vec3 viewPos;
uniform vec3 lightDirection;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform float steps;
uniform float rimStrength;

void main()
{
    vec3 albedo = texture(texture0, fragTexCoord).rgb * colDiffuse.rgb * fragColor.rgb;
    vec3 normal = normalize(fragNormal);
    float count = max(steps, 2.0);
    float diffuse = max(dot(normal, normalize(-lightDirection)), 0.0);
    diffuse = floor(diffuse * count) / (count - 1.0);
    float rim = pow(1.0 - max(dot(normal, normalize(viewPos - fragPosition)), 0.0), 3.0) * rimStrength;
    vec3 light = ambientColor + lightColor * diffuse + vec3(rim);
    finalColor = vec4(albedo * light, colDiffuse.a * fragColor.a);
}
