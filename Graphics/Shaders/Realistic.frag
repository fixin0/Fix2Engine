#version 330

in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;
out vec4 finalColor;

uniform sampler2D texture0;
uniform sampler2D albedoMap;
uniform sampler2D metallicMap;
uniform sampler2D roughnessMap;
uniform sampler2D occlusionMap;
uniform sampler2D emissionMap;
uniform vec4 colDiffuse;
uniform vec3 viewPos;
uniform vec3 lightDirection;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform float metallic;
uniform float roughness;
uniform float hasAlbedoMap;
uniform float hasMetallicMap;
uniform float hasRoughnessMap;
uniform float hasOcclusionMap;
uniform float hasEmissionMap;

const float PI = 3.14159265359;

float DistributionGGX(vec3 n, vec3 h, float r)
{
    float a = r * r;
    float a2 = a * a;
    float nDotH = max(dot(n, h), 0.0);
    float nDotH2 = nDotH * nDotH;
    float denominator = nDotH2 * (a2 - 1.0) + 1.0;
    return a2 / max(PI * denominator * denominator, 0.0001);
}

float GeometrySchlickGGX(float nDotV, float r)
{
    float k = (r + 1.0) * (r + 1.0) / 8.0;
    return nDotV / (nDotV * (1.0 - k) + k);
}

float GeometrySmith(vec3 n, vec3 v, vec3 l, float r)
{
    return GeometrySchlickGGX(max(dot(n, v), 0.0), r) * GeometrySchlickGGX(max(dot(n, l), 0.0), r);
}

vec3 FresnelSchlick(float cosine, vec3 f0)
{
    return f0 + (1.0 - f0) * pow(1.0 - cosine, 5.0);
}

void main()
{
    vec3 defaultAlbedo = texture(texture0, fragTexCoord).rgb;
    vec3 albedoTexture = texture(albedoMap, fragTexCoord).rgb;
    vec3 albedo = mix(defaultAlbedo, albedoTexture, hasAlbedoMap) * colDiffuse.rgb * fragColor.rgb;
    vec3 n = normalize(fragNormal);
    vec3 v = normalize(viewPos - fragPosition);
    vec3 l = normalize(-lightDirection);
    vec3 h = normalize(v + l);
    float m = clamp(mix(metallic, texture(metallicMap, fragTexCoord).b, hasMetallicMap), 0.0, 1.0);
    float r = clamp(mix(roughness, texture(roughnessMap, fragTexCoord).g, hasRoughnessMap), 0.06, 1.0);
    vec3 f0 = mix(vec3(0.04), albedo, m);
    vec3 f = FresnelSchlick(max(dot(h, v), 0.0), f0);
    float ndf = DistributionGGX(n, h, r);
    float g = GeometrySmith(n, v, l, r);
    vec3 specular = (ndf * g * f) / max(4.0 * max(dot(n, v), 0.0) * max(dot(n, l), 0.0), 0.001);
    vec3 kd = (1.0 - f) * (1.0 - m);
    float nDotL = max(dot(n, l), 0.0);
    vec3 direct = (kd * albedo / PI + specular) * lightColor * nDotL;
    float occlusion = mix(1.0, texture(occlusionMap, fragTexCoord).r, hasOcclusionMap);
    vec3 emission = texture(emissionMap, fragTexCoord).rgb * hasEmissionMap;
    vec3 color = (ambientColor * albedo * occlusion + direct) + emission;
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0 / 2.2));
    finalColor = vec4(color, colDiffuse.a * fragColor.a);
}
