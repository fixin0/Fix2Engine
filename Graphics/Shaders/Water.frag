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
uniform vec3 shallowColor;
uniform vec3 deepColor;
uniform float transparency;

void main()
{
    vec3 normal = normalize(fragNormal);
    vec3 viewDirection = normalize(viewPos - fragPosition);
    float fresnel = pow(1.0 - max(dot(normal, viewDirection), 0.0), 4.0);
    float diffuse = max(dot(normal, normalize(-lightDirection)), 0.0);
    vec3 tinted = mix(shallowColor, deepColor, clamp(fresnel + 0.28, 0.0, 1.0));
    vec3 reflection = lightColor * (0.20 + diffuse * 0.45) * fresnel;
    vec3 base = texture(texture0, fragTexCoord).rgb * colDiffuse.rgb * fragColor.rgb;
    finalColor = vec4((tinted + reflection + ambientColor * 0.22) * base, transparency * colDiffuse.a);
}
