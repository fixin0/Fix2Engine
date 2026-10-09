#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matNormal;
uniform float time;
uniform float waveHeight;
uniform float waveFrequency;
uniform vec2 flowDirection;

out vec3 fragPosition;
out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;

void main()
{
    vec3 p = vertexPosition;
    vec2 flow = normalize(flowDirection + vec2(0.001));
    float wave = sin(dot(p.xz, flow) * waveFrequency + time * 1.7) +
                 cos(dot(p.xz, vec2(-flow.y, flow.x)) * waveFrequency * 0.72 + time * 1.25);
    p.y += wave * waveHeight * 0.5;
    fragPosition = vec3(matModel * vec4(p, 1.0));
    fragTexCoord = vertexTexCoord + flow * time * 0.035;
    fragColor = vertexColor;
    fragNormal = normalize(vec3(matNormal * vec4(vertexNormal, 0.0)));
    gl_Position = mvp * vec4(p, 1.0);
}
