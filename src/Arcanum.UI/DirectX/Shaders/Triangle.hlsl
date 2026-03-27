cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
    float4 Settings;
    float EffectMode;
}

// EffectMode Mapping:
// 0: Standard (Alpha channel only)
// 1: Popcorn (Individual wiggle)
// 2: Ripple (Wave from mouse)
// 3: Magnet (Look at mouse)
// 4: Swirl (Vortex)

// The new structured buffer for our color lookup table
StructuredBuffer<float4> ColorLookup : register(t0);

struct VSInput
{
    float2 Position : POSITION;
    float2 CenterPosition: CENTER;
    uint PolygonId : POLYGON_ID; // Changed from Color
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color: COLOR; // Pass the ID to the pixel shader
};

PSInput VSMain(VSInput input)
{
    PSInput result;
    float2 local_pos = input.Position - input.CenterPosition;
    float4 color = ColorLookup[input.PolygonId];

    float time = Settings.x;
    float2 mouse_pos = Settings.yz;
    float aspect = Settings.w;
    int mode = (int)EffectMode;

    // Default rotation from your Alpha channel
    float angle = color.a;

    // --- EFFECT LOGIC ---
    if (mode == 1) // Popcorn
    {
        angle += sin(time * 5.0f + input.PolygonId * 0.5f) * 0.5f;
    }
    else if (mode == 2) // Ripple
    {
        float dist = distance(input.CenterPosition, mouse_pos);
        angle += sin(dist * 20.0f - time * 10.0f) * 0.5f;
    }
    else if (mode == 3) // Magnet
    {
        float2 dir = mouse_pos - input.CenterPosition;

        if (length(dir) > 0.001f)
        {
            dir = normalize(dir);
            angle = atan2(dir.y, dir.x);
        }
    }
    else if (mode == 4) // Swirl
    {
        float2 map_center = float2(0.5f, 0.5f * aspect);
        float dist = distance(input.CenterPosition, map_center);

        angle += time * (dist * 5.0f);
    }

    float sin_a, cos_a;
    sincos(angle, sin_a, cos_a);

    float2 rotated;
    rotated.x = local_pos.x * cos_a - local_pos.y * sin_a;
    rotated.y = local_pos.x * sin_a + local_pos.y * cos_a;

    // Fix alpha for rendering
    color.a = 1.0f;

    float4 pos = float4(rotated + input.CenterPosition, 0.0f, 1.0f);
    result.Position = mul(pos, WorldViewProjection);
    result.Color = color;
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    // Look up the color from the buffer using the PolygonId
    return input.Color;
}
