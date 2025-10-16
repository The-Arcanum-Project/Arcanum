struct Rect
{
    int left, top, right, bottom;
};

struct TopTwoColors
{
    float4 most_frequent;
    float4 second_most_frequent;
};

// --- Resources ---
Texture2D SrcTexture : register(t0);
StructuredBuffer<Rect> LocationBounds : register(t1);
RWStructuredBuffer<TopTwoColors> OutputColors : register(u0);

// --- Constants ---
#define GROUP_SIZE_X 16
#define GROUP_SIZE_Y 16
#define GROUP_SIZE (GROUP_SIZE_X * GROUP_SIZE_Y) // 256 threads
#define HISTOGRAM_SIZE 4096 // R4G4B4 = 16*16*16 = 4096 colors

// --- Group Shared Memory (Now fits within the 32KB limit!) ---
groupshared uint histogram[HISTOGRAM_SIZE];

// Helper to de-quantize a 12-bit index back to a float4 color
float4 dequantize_color(uint index)
{
    uint r4 = index >> 8 & 0x0F;
    uint g4 = index >> 4 & 0x0F;
    uint b4 = index & 0x0F;

    // Scale 4-bit [0,15] back to [0,1]
    return float4(r4 / 15.0f, g4 / 15.0f, b4 / 15.0f, 1.0f);
}

[numthreads(GROUP_SIZE_X, GROUP_SIZE_Y, 1)]
void main(uint3 group_id : SV_GroupID,
          uint3 group_thread_id : SV_GroupThreadID)
{
    uint thread_index = group_thread_id.y * GROUP_SIZE_X + group_thread_id.x;
    uint location_index = group_id.x;

    for (uint i = thread_index; i < HISTOGRAM_SIZE; i += GROUP_SIZE)
    {
        histogram[i] = 0;
    }
    GroupMemoryBarrierWithGroupSync();

    Rect location_rect = LocationBounds[location_index];
    uint width, height;
    SrcTexture.GetDimensions(width, height);

    int start_x = max(location_rect.left, 0);
    int start_y = max(location_rect.top, 0);
    int end_x = min(location_rect.right, width);
    int end_y = min(location_rect.bottom, height);
    int total_pixels = (end_x - start_x) * (end_y - start_y);

    if (total_pixels > 0)
    {
        for (int i = thread_index; i < total_pixels; i += GROUP_SIZE)
        {
            int x = start_x + i % (end_x - start_x);
            int y = start_y + i / (end_x - start_x);

            float4 pixel = SrcTexture.Load(int3(x, y, 0));

            // Quantize R8G8B8 to R4G4B4
            uint r4 = (uint)(pixel.r * 15.0f);
            uint g4 = (uint)(pixel.g * 15.0f);
            uint b4 = (uint)(pixel.b * 15.0f);

            uint histogram_index = r4 << 8 | g4 << 4 | b4;

            InterlockedAdd(histogram[histogram_index], 1);
        }
    }
    GroupMemoryBarrierWithGroupSync();

    if (thread_index == 0)
    {
        uint final_top_idx1 = 0, final_top_idx2 = 0;
        uint final_top_cnt1 = 0, final_top_cnt2 = 0;

        for (uint i = 0; i < HISTOGRAM_SIZE; i++)
        {
            uint count = histogram[i];
            if (count > final_top_cnt1)
            {
                final_top_cnt2 = final_top_cnt1;
                final_top_idx2 = final_top_idx1;
                final_top_cnt1 = count;
                final_top_idx1 = i;
            }
            else if (count > final_top_cnt2)
            {
                final_top_cnt2 = count;
                final_top_idx2 = i;
            }
        }

        if (final_top_cnt2 == 0)
        {
            final_top_idx2 = final_top_idx1;
        }

        TopTwoColors result;
        result.most_frequent = dequantize_color(final_top_idx1);
        result.second_most_frequent = dequantize_color(final_top_idx2);
        OutputColors[location_index] = result;
    }
}
