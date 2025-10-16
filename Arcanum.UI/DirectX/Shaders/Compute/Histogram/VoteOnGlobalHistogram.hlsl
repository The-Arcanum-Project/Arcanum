struct Rect
{
    int left, top, right, bottom;
};

#define HISTOGRAM_SIZE 32768 // R5G5B5 - now we can use the big one!

// --- Resources ---
Texture2D<float4> SrcTexture : register(t0);
StructuredBuffer<Rect> LocationBounds : register(t1);
RWStructuredBuffer<uint> GlobalHistogram : register(u0);

[numthreads(16, 16, 1)] // 256 threads per group
void main(uint3 group_id : SV_GroupID,
          uint3 group_thread_id : SV_GroupThreadID)
{
    uint thread_index = group_thread_id.y * 16 + group_thread_id.x;
    uint location_index = group_id.x;

    // Calculate the base address for this rectangle's histogram.
    uint histogram_base = location_index * HISTOGRAM_SIZE;

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
        for (int i = thread_index; i < total_pixels; i += 256)
        {
            int x = start_x + (i % (end_x - start_x));
            int y = start_y + (i / (end_x - start_x));

            float4 pixel = SrcTexture.Load(int3(x, y, 0));

            // Quantize R8G8B8 to R5G5B5
            uint r5 = (uint)(pixel.r * 31.0f);
            uint g5 = (uint)(pixel.g * 31.0f);
            uint b5 = (uint)(pixel.b * 31.0f);

            uint histogram_index = (r5 << 10) | (g5 << 5) | b5;

            // Atomically increment the counter in the GLOBAL histogram.
            InterlockedAdd(GlobalHistogram[histogram_base + histogram_index], 1);
        }
    }
}
