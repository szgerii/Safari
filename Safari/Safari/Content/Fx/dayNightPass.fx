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
sampler OutputSampler = sampler_state
{
    Texture = (GameOutput);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VertexShaderInput
{
	float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

// night consts
static const float4x4 night_correction = float4x4(
		0.45, 0.0, 0.0, 0.0,
		0.0, 0.55, 0.0, 0.0,
		0.1, 0.1, 0.85, 0.0,
		0.0, 0.0, 0.0, 1.0
	);
static const float4 night_brightness_regular = -float4(.15, .15, .15, 0);
static const float4 night_brightness_lamp = float4(.18, .18, .20, 0);
static const float4 night_moon = float4(0.07, 0.07, 0.07, 0);
// day consts
static const float4x4 day_correction = float4x4(
        1.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        0.0, 0.0, 0.0, 1.0
    );
static const float4 day_brightness = -float4(.12, .12, .12, 0);
static const float4 day_sun = float4(.20, .20, .20, 0);
// sunrise consts
static const float4x4 sunrise_correction = float4x4(
        1.0, 0.1, 0.1, 0.0,
        0.1, 1.0, 0.1, 0.0,
        0.0, 0.0, 0.8, 0.0,
        0.0, 0.0, 0.0, 1.0
    );
static const float4 sunrise_brightness = -float4(.3, .35, .4, 0);
static const float sunrise_start = 0.96;
static const float sunrise_end = 0.04;
// sunset consts
static const float4x4 sunset_correction = float4x4(
        1.0, 0.35, 0.35, 0.0,
        -0.05, 1.0, 0.1, 0.0,
        -0.1, -0.1, 1.0, 0.0,
        0.0, 0.0, 0.0, 1.0
    );
static const float4 sunset_brightness = -float4(.1, .08, .2, 0);
static const float sunset_start = 0.46;
static const float sunset_end = 0.54;

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = float4(input.Position.xy, 0, 1.0);
    output.TexCoord = input.TexCoord;
	
	return output;
}

float4 NightColor(float4 old_color, float light_level) : COLOR
{
    return mul(night_correction, old_color) + night_brightness_regular + light_level * night_brightness_lamp;
}

float4 DayColor(float4 old_color) : COLOR
{
    return mul(day_correction, old_color) + day_brightness;
}

float4 SunriseColor(float4 old_color) : COLOR {
    return mul(sunrise_correction, old_color) + sunrise_brightness;
}

float4 SunsetColor(float4 old_color) : COLOR {
    return mul(sunset_correction, old_color) + sunset_brightness;
}

float Movement(float interp)
{
    return -pow((2.0 * interp - 1.0), 2) + 1.0;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 old_color = float4(tex2D(OutputSampler, input.TexCoord).rgb, 1.0);
    float4 night_color = NightColor(old_color, 0.0);
    float4 night_color_no_lights = NightColor(old_color, 0.0);
    float4 day_color = DayColor(old_color);
    float4 sunrise_color = lerp(night_color, SunriseColor(old_color), 0.5);
    float4 sunset_color = lerp(day_color, SunsetColor(old_color), 0.5);
    if (Time > sunset_end && Time < sunrise_start)
    {
        // night time
        float night_interp = (sunrise_start - Time) / (sunrise_start - sunset_end);
        float mov = Movement(night_interp);
        return night_color + mov * night_moon;
    }
    else if (Time >= sunset_start && Time <= sunset_end)
    {
        // sunset
        float sunset_interp = (Time - sunset_start) / (sunset_end - sunset_start);
        float mov = Movement(sunset_interp);
        if (sunset_interp < 0.5)
        {
            return lerp(day_color, sunset_color, mov);
        }
        else
        {
            return lerp(night_color, sunset_color, mov);
        }
    }
    else if (Time <= sunrise_end || Time >= sunrise_start)
    {
        // sunrise
        float sunrise_interp = 0.0;
        float sunrise_len = 1.0 - sunrise_start + sunrise_end;
        if (Time >= sunrise_start)
        {
            sunrise_interp = (sunrise_len / 2.0 - (1.0 - Time)) / sunrise_len;
            float mov = Movement(sunrise_interp);
            return lerp(night_color, sunrise_color, mov);
        }
        else
        {
            sunrise_interp = 0.5 + (sunrise_len / 2.0 - (sunrise_end - Time)) / sunrise_len;
            float mov = Movement(sunrise_interp);
            return lerp(day_color, sunrise_color, mov);
        }
    }
    else
    {
        float day_interp = (sunset_start - Time) / (sunset_start - sunrise_end);
        float mov = Movement(day_interp);
        return day_color + mov * day_sun;
    }
}

technique Main {
	pass P0 {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};