struct VS_INPUT
{
    float4 rectNdc : TEXCOORD0;
    float4 uvRect  : TEXCOORD1;
    float4 col     : COLOR0;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 uv  : TEXCOORD0;
    float4 col : COLOR0;
};

static const float2 Quad[4] =
{
    float2(0.0f, 0.0f), float2(1.0f, 0.0f),
    float2(1.0f, 1.0f), float2(0.0f, 1.0f)
};

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    float2 corner = Quad[vertexId];
    PS_INPUT output;
    output.pos = float4(
        corner.x < 0.5f ? input.rectNdc.x : input.rectNdc.z,
        corner.y < 0.5f ? input.rectNdc.y : input.rectNdc.w,
        0.0f, 1.0f);
    output.uv = float2(
        corner.x < 0.5f ? input.uvRect.x : input.uvRect.z,
        corner.y < 0.5f ? input.uvRect.y : input.uvRect.w);
    output.col = input.col;
    return output;
}
