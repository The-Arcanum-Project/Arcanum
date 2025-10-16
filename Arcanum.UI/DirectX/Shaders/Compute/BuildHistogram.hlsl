struct Rect
{
    uint left, top, right, bottom;
};

// We will pass the specific rectangle we're analyzing in a constant buffer.
cbuffer Constants : register(b0)
{
    Rect processing_rect;
}

Texture2D SrcTexture : register(t0);
RWStructuredBuffer<uint> Histogram : register(u0);

// We dispatch one thread per pixel in the rectangle.
[numthreads(16, 16, 1)]
void main(uint3 dispatch_thread_id : SV_DispatchThreadID,
          uint3 group_thread_id : SV_GroupThreadID,
          uint3 group_id : SV_GroupID)
{
    // Calculate the pixel coordinate this thread is responsible for.
    uint2 pixel_coord = group_id.xy * uint2(16, 16) + group_thread_id.xy;

    // Add the top-left offset of our processing rectangle.
    pixel_coord += uint2(processing_rect.left, processing_rect.top);

    // Bounds check: if this thread is outside the rectangle, do nothing.
    if (pixel_coord.x >= processing_rect.right || pixel_coord.y >= processing_rect.bottom)
    {
        return;
    }

    float4 pixel = SrcTexture.Load(int3(pixel_coord, 0));

    // Quantize R8G8B8 to R5G5B5
    uint r5 = (uint)(pixel.r * 31.0f);
    uint g5 = (uint)(pixel.g * 31.0f);
    uint b5 = (uint)(pixel.b * 31.0f);

    uint histogram_index = (r5 << 10) | (g5 << 5) | b5;

    // Atomically increment the counter for this color in the GLOBAL histogram buffer.
    InterlockedAdd(Histogram[histogram_index], 1);
}
