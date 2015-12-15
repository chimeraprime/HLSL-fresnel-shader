float4x4 WorldViewProjection  : WORLDVIEWPROJECTION;
float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 WorldView : WORLDVIEW;

float4x4 TexTransform
<
    string SasBindAddress = "Ventuz.Texture.Mapping";
>;


float Reflectivity = 50.0f;
float Rzero = 1.0f;

texture Reflection;
sampler ReflectionSampler = sampler_state
{
    Texture = <Reflection>;    
};

texture Diffuse;
sampler DiffuseSampler = sampler_state
{
    Texture = <Diffuse>;
};


// Vertex shader program input
struct VS_INPUT
{
    float4 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
};


// Vertex shader program output
struct VS_OUTPUT
{
    float4 Position : POSITION;
    float3 Normal : TEXCOORD0;
    float3 ViewVec : TEXCOORD1;
    float2 TexCoord : TEXCOORD3;
    
};


// Vertex Shader Program

VS_OUTPUT VS( VS_INPUT Input )
{
    VS_OUTPUT Output;

    float3 viewposition = mul(float4(0.0f, 0.0f, 0.0f, 1.0f), View ).xyz;
    
    Output.Position = mul( Input.Position, WorldViewProjection );
    Output.Normal = mul( float4(Input.Normal, 0.0f), World ).xyz;
    Output.ViewVec = -normalize( mul(Input.Position, WorldView).xyz );
    Output.TexCoord = mul(float4(Input.TexCoord.x, Input.TexCoord.y, 1.0, 0.0f), TexTransform).xy;
 
    return Output;
}


// Pixel Shader Program

float4 PS( VS_OUTPUT Input ) : COLOR
{
    Input.Normal = normalize(Input.Normal);
    Input.ViewVec = normalize(Input.ViewVec);


    float3 reflVec = reflect(-Input.ViewVec, Input.Normal);
    float4 reflection = texCUBE(ReflectionSampler, reflVec);

    float4 t = tex2D( DiffuseSampler, Input.TexCoord);
    
    float4 fresnel = Rzero + (1.0f - Rzero) * pow(abs(1.0f - dot(Input.Normal, Input.ViewVec)), 5.0 );    
    float4 result =  5*t +  lerp(t, reflection, fresnel) ;
    result.a = 1;
    return result;
}


technique SimpleReflection
{
    pass pass0
    {
   	 vertexshader = compile vs_3_0 VS();
   	 pixelshader  = compile ps_3_0 PS();
    }
}

