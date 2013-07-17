sampler TextureSampler  : register(s0);

float2 lightPos;
float2 targetPos;
float2 lv; //always = targetPos - lightPos;
float maxAngle;
float falloff = 5;
float intensity = 1;
float rangeInverse = 1;
float4 LightFunction(float4 tint : COLOR0, float2 uv : TEXCOORD) : COLOR
{
	float4 texCol = tex2D(TextureSampler,uv);
	float2 sv = uv-lightPos;
	float2 a = normalize(lv);
	float2 b = normalize(sv);
	float rayangle = acos(dot(a,b));

	if (rayangle > maxAngle) clip(-1);
	
	/*
	//OLD BEAUTIFUL METHOD, PROBLEM: DOES NOT ACCOUNT FOR TARGET-POSITION DISTANCE
	return texCol * intensity * saturate(
		falloff * tint * (maxAngle-rayangle))
		* (1.0f - saturate(length(sv)*rangeInverse));*/

	return texCol * intensity * saturate(
		falloff * tint * (maxAngle-rayangle))
		* saturate(length(lv) - length(sv)*rangeInverse);
}

technique TexLighting
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 LightFunction();
    }
}