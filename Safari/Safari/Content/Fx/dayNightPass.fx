#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Uniforms
texture GameOutput;
float Time;

// Texture samplers
sampler OutputSampler = sampler_state {
    Texture = (GameOutput);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VertexShaderInput {
	float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput {
	float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input) {
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = float4(input.Position.xy, 0, 1.0);
    output.TexCoord = input.TexCoord;
	
	return output;
}

float4 NightColor(float4 old_color, float light_level) : COLOR {
    static const float4x4 night_correction = float4x4(
		0.45, 0.0, 0.0, 0.0,
		0.0, 0.55, 0.0, 0.0,
		0.1, 0.1, 0.85, 0.0,
		0.0, 0.0, 0.0, 1.0
	);
    static const float4 night_brightness_regular = -float4(.12, .12, .12, 0);
    static const float4 night_brightness_lamp = float4(.14, .14, .18, 0);
    return mul(night_correction, old_color) + night_brightness_regular + light_level * night_brightness_lamp;
}

float4 DayColor(float4 old_color) : COLOR {
    static const float4x4 day_correction = float4x4(
        1.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        0.0, 0.0, 0.0, 1.0
    );
    return mul(day_correction, old_color);

}

float4 MainPS(VertexShaderOutput input) : COLOR {
    float4 old_color = float4(tex2D(OutputSampler, input.TexCoord).rgb, 1.0);
    float4 night_color = NightColor(old_color, 0.0);
    float4 day_color = DayColor(old_color);
    if (Time > 0.52 && Time < 0.98)
    {
        // night time
        return night_color;
    }
    else if (Time >= 0.48 && Time <= 0.52)
    {
        // sunset
        float sunset_interp = (Time - 0.48) * 25.0;
        return lerp(old_color, night_color, sunset_interp);
    }
    else if (Time <= 0.02 || Time >= 0.98)
    {
        // sunrise
        float sunrise_interp = 0.0;
        if (Time >= 0.98)
        {
            sunrise_interp = (0.02 - (1.0 - Time)) * 25.0;
        }
        else
        {
            sunrise_interp = 0.5 + (0.02 - (0.02 - Time)) * 25.0;
        }
        return lerp(night_color, old_color, sunrise_interp);
    }
    else
    {
        return day_color;
    }
}

technique Main {
	pass P0 {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};