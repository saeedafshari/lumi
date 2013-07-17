//sampler TextureSampler  : register(s0);

float2 lightPos;
float2 vertices[4];
bool strip = false;

bool intersects(float2 pos, float2 a, float2 b)
{
	if (length(a-b)==0) return false;

	float ua = ((b.x - a.x) * (lightPos.y - a.y) - (b.y - a.y) * (lightPos.x - a.x)) /
		((b.y - a.y) * (pos.x - lightPos.x) - (b.x - a.x) * (pos.y - lightPos.y));
	float ub = ((pos.x - lightPos.x) * (lightPos.y - a.y) - (pos.y - lightPos.y) * (lightPos.x - a.x)) /
		((b.y - a.y) * (pos.x - lightPos.x) - (b.x - a.x) * (pos.y - lightPos.y));

	return (ua >= 0.0f && ua <= 1.0f && ub >= 0.0f && ub <= 1.0f);
}

float4 ShadeFunction(float4 tint : COLOR0, float2 uv : TEXCOORD) : COLOR
{
	//if (!strip)
		if (
			intersects(uv, vertices[0], vertices[1]) ||
			intersects(uv, vertices[2], vertices[3]))
		{ 
			return float4(0,0,0,1);
		}
	/*else
	{
		if (
				intersects(uv, vertices[1], vertices[0]) ||
				intersects(uv, vertices[2], vertices[1]) ||
				intersects(uv, vertices[0], vertices[2]))
				return float4(0,0,0,1);
	
		if (vCount >= 2 && intersects(uv, vertices[0], vertices[1]))
			return float4(0,0,0,1);
		if (vCount >= 3 && intersects(uv, vertices[1], vertices[2]))
			return float4(0,0,0,1);
		if (vCount >= 4 && intersects(uv, vertices[2], vertices[3]))
			return float4(0,0,0,1);
	}*/

	clip(-1);
	return float4(1,0,0,1); //Does this even execute?!
}

technique TexLighting
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 ShadeFunction();
    }
}