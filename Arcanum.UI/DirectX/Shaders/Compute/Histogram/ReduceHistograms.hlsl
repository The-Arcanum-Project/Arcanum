struct TopTwoColors
{
    float4 most_frequent;
    float4 second_most_frequent;
};

#define HISTOGRAM_SIZE 32768
#define GROUP_SIZE 256
// --- Resources ---
StructuredBuffer<uint> GlobalHistogram : register(t0);
RWStructuredBuffer<TopTwoColors> OutputColors : register(u0);

groupshared uint shared_indices[GROUP_SIZE * 2]; // 512 entries
groupshared uint shared_counts[GROUP_SIZE * 2]; // 512 entries

// Helper to de-quantize a 15-bit index back to a float4 color
float4 dequantize_color(uint index)
{
    uint r5 = (index >> 10) & 0x1F;
    uint g5 = (index >> 5) & 0x1F;
    uint b5 = index & 0x1F;
    return float4(r5 / 31.0f, g5 / 31.0f, b5 / 31.0f, 1.0f);
}

[numthreads(256, 1, 1)]
void main(uint3 group_id : SV_GroupID,
          uint3 group_thread_id : SV_GroupThreadID)
{
    uint thread_index = group_thread_id.x;
    uint location_index = group_id.x;

    // Base address for this rectangle's histogram.
    uint histogram_base = location_index * HISTOGRAM_SIZE;

    // --- Parallel Reduction to find top 2 ---
    // Each thread finds the top 2 in its own slice of the histogram.
    uint local_top_idx1 = 0, local_top_idx2 = 0;
    uint local_top_cnt1 = 0, local_top_cnt2 = 0;

    for (uint i = thread_index; i < HISTOGRAM_SIZE; i += 256)
    {
        uint count = GlobalHistogram[histogram_base + i];
        if (count > local_top_cnt1)
        {
            local_top_idx2 = local_top_idx1;
            local_top_idx1 = i;
            local_top_cnt2 = local_top_cnt1;
            local_top_cnt1 = count;
        }
        else if (count > local_top_cnt2)
        {
            local_top_idx2 = i;
            local_top_cnt2 = count;
        }
    }


    shared_indices[thread_index * 2] = local_top_idx1;
    shared_indices[thread_index * 2 + 1] = local_top_idx2;
    shared_counts[thread_index * 2] = local_top_cnt1;
    shared_counts[thread_index * 2 + 1] = local_top_cnt2;

    GroupMemoryBarrierWithGroupSync();

    if (thread_index == 0)
    {
        uint final_top_idx1 = 0, final_top_idx2 = 0;
        uint final_top_cnt1 = 0, final_top_cnt2 = 0;

        // Leader thread scans the 512 candidates in shared memory.
        for (uint i = 0; i < 512; i++)
        {
            uint count = shared_counts[i];
            uint index = shared_indices[i];
            if (count > final_top_cnt1)
            {
                final_top_idx2 = final_top_idx1;
                final_top_idx1 = index;
                final_top_cnt2 = final_top_cnt1;
                final_top_cnt1 = count;
            }
            else if (count > final_top_cnt2)
            {
                final_top_idx2 = index;
                final_top_cnt2 = count;
            }
        }

        if (final_top_cnt2 == 0) { final_top_idx2 = final_top_idx1; }

        TopTwoColors result;
        result.most_frequent = dequantize_color(final_top_idx1);
        result.second_most_frequent = dequantize_color(final_top_idx2);
        OutputColors[location_index] = result;
    }
}
