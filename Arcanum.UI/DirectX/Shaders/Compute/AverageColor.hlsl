// Define the data structures that match our C# side.
// A standard float4 can represent an RGBA color.
struct rect
{
    int left;
    int top;
    int right;
    int bottom;
};

// Input texture (our PNG image)
Texture2D src_texture : register(t0);

// Input buffer of location bounds
StructuredBuffer<rect> location_bounds : register(t1);

// Output buffer for the results
RWStructuredBuffer<float4> average_colors : register(u0);

// Define the entry point for the compute shader.
// [numthreads(X, Y, Z)] defines the size of a thread group.
// 8x8 is a common, well-balanced size.
[numthreads(64, 1, 1)]
void main(uint3 dispatch_thread_id : SV_DispatchThreadID)
{
    // dispatchThreadID.x is the unique, global ID for this thread.
    // It corresponds to the index of the location we need to process.
    uint location_index = dispatch_thread_id.x;

    uint total_locations, _;
    location_bounds.GetDimensions(total_locations, _);
    if (location_index >= total_locations)
    {
        return; // This thread has no work to do, so it exits early.
    }

    rect location_rect = location_bounds[location_index];

    // Get the dimensions of the source texture.
    uint width, height;
    src_texture.GetDimensions(width, height);

    // Calculate the intersection bounds.
    // Use min/max to clamp the location bounds to the image bounds.
    int start_x = max(location_rect.left, 0);
    int start_y = max(location_rect.top, 0);
    int end_x = min(location_rect.right, width);
    int end_y = min(location_rect.bottom, height);

    float4 color_sum = 0.0f;
    int pixel_count = 0;

    // If the intersection is valid, loop through the pixels.
    if (start_x < end_x && start_y < end_y)
    {
        for (int y = start_y; y < end_y; y++)
        {
            for (int x = start_x; x < end_x; x++)
            {
                // Sample the color from the texture at this coordinate.
                color_sum += src_texture.Load(int3(x, y, 0));
            }
        }

        pixel_count = (end_x - start_x) * (end_y - start_y);
    }

    // Calculate the average and write it to the output buffer.
    // Avoid division by zero.
    if (pixel_count > 0)
    {
        // set a color where red is x normalized and b is y normalized with g being the count normalized
        // color_sum.rgb /= (float)pixel_count;
        // color_sum.a = start_x / (float)width;
        // color_sum.b = start_y / (float)height;

        average_colors[location_index] = color_sum / (float)pixel_count;
    }
    else
    {
        // Write a default value if the location was outside the image.
        average_colors[location_index] = (float4)0.0f;
    }
}
