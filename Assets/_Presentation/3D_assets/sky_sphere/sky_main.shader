Shader "Kaleidos/unnamed pattern (5) 3D URP" {
    Properties {
        _Zoom ("Zoom (mesh size in metres)", Float) = 1
        _Pan ("Pan", Vector) = (0,0,0,0)
        _Rotation ("Rotation", Float) = 0
        _Tint ("Tint", Color) = (1,1,1,1)
        _Speed ("Speed", Float) = 1
        _CellSize ("Pixel Size", Float) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _Zoom;
            float4 _Pan;
            float _Rotation;
            float4 _Tint;
            float _Speed;
            float _CellSize;

            #define uTime (_Time.y * _Speed)
            #define cellSize _CellSize

static const float noiseScale = 6.0;
static const float gain = 0.5;
float hash3(float3 p){
  p = frac(p * float3(127.1, 311.7, 74.7));
  p += dot(p, p.yxz + 19.19);
  return frac((p.x + p.y) * p.z);
}
float snoise3(float3 p){
  float3 i = floor(p); float3 fr = frac(p);
  float3 u = fr * fr * (3.0 - 2.0 * fr);
  return lerp(
    lerp(lerp(hash3(i+float3(0,0,0)), hash3(i+float3(1,0,0)), u.x), lerp(hash3(i+float3(0,1,0)), hash3(i+float3(1,1,0)), u.x), u.y),
    lerp(lerp(hash3(i+float3(0,0,1)), hash3(i+float3(1,0,1)), u.x), lerp(hash3(i+float3(0,1,1)), hash3(i+float3(1,1,1)), u.x), u.y),
    u.z) * 2.0 - 1.0;
}

float fbm3(float3 p){
  float v = 0.0, a = 0.5, norm = 0.0;
  for (int i = 0; i < 6; i++){
    float sn = snoise3(p);
    float t = sn * 0.5 + 0.5;
    v += a * t; norm += a;
    p = p * 2.0 + float3(5.3, 1.7, 3.1);
    a *= gain;
  }
  return v / norm;
}

float fieldVolume(float3 p){
  float x01 = atan2(p.z, p.x) / 6.28318531 + 0.5;
  float yz = length(p.xz) * noiseScale + uTime * 0.08;
  float dx = uTime * 0.08;
  float hz = p.y * noiseScale + uTime * 0.08;
  float3 pa = float3(x01 * noiseScale + dx, yz, hz);
  float3 pb = float3((x01 - 1.0) * noiseScale + dx, yz, hz);
  return lerp(fbm3(pa), fbm3(pb), x01);
}

static const float4 PALETTE[110] = {
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.274306,0.248867,0.200304,1.0),
  float4(0.267361,0.242165,0.194065,1.0),
  float4(0.260417,0.235463,0.187826,1.0),
  float4(0.253472,0.228761,0.181586,1.0),
  float4(0.246528,0.222059,0.175347,1.0),
  float4(0.239583,0.215357,0.169108,1.0),
  float4(0.232639,0.208655,0.162869,1.0),
  float4(0.225694,0.201953,0.15663,1.0),
  float4(0.21875,0.195251,0.150391,1.0),
  float4(0.21875,0.195251,0.150391,1.0),
  float4(0.21875,0.195251,0.150391,1.0),
  float4(0.21875,0.195251,0.150391,1.0),
  float4(0.21875,0.195251,0.150391,1.0),
  float4(0.226563,0.202791,0.15741,1.0),
  float4(0.234375,0.210331,0.164429,1.0),
  float4(0.242188,0.217871,0.171448,1.0),
  float4(0.25,0.22541,0.178467,1.0),
  float4(0.257813,0.23295,0.185486,1.0),
  float4(0.265625,0.24049,0.192505,1.0),
  float4(0.273438,0.24803,0.199524,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.257813,0.232258,0.183472,1.0),
  float4(0.234375,0.208946,0.1604,1.0),
  float4(0.210938,0.185635,0.137329,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.200893,0.175644,0.127441,1.0),
  float4(0.214286,0.188965,0.140625,1.0),
  float4(0.227679,0.202286,0.153809,1.0),
  float4(0.241071,0.215607,0.166992,1.0),
  float4(0.254464,0.228928,0.180176,1.0),
  float4(0.267857,0.242249,0.193359,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.28125,0.255569,0.206543,1.0),
  float4(0.28125,0.254059,0.202148,1.0),
  float4(0.28125,0.252548,0.197754,1.0),
  float4(0.28125,0.251038,0.193359,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.302951,0.270998,0.209997,1.0),
  float4(0.290123,0.258923,0.199359,1.0),
  float4(0.277296,0.246848,0.188722,1.0),
  float4(0.264468,0.234773,0.178084,1.0),
  float4(0.25164,0.222698,0.167446,1.0),
  float4(0.238812,0.210623,0.156809,1.0),
  float4(0.225984,0.198548,0.146171,1.0),
  float4(0.213156,0.186473,0.135533,1.0),
  float4(0.200328,0.174398,0.124896,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.208194,0.182312,0.132901,1.0),
  float4(0.228889,0.202302,0.151545,1.0),
  float4(0.249583,0.222291,0.170189,1.0),
  float4(0.254107,0.227045,0.175382,1.0),
  float4(0.259536,0.231542,0.178099,1.0),
  float4(0.264964,0.236038,0.180815,1.0),
  float4(0.270393,0.240534,0.183532,1.0),
  float4(0.275821,0.245031,0.186248,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.28125,0.249527,0.188965,1.0),
  float4(0.281507,0.250343,0.190846,1.0),
  float4(0.281764,0.251158,0.192728,1.0),
  float4(0.282022,0.251974,0.19461,1.0),
  float4(0.282279,0.252789,0.196491,1.0),
  float4(0.282536,0.253605,0.198373,1.0),
  float4(0.282793,0.254421,0.200255,1.0),
  float4(0.28305,0.255236,0.202136,1.0),
  float4(0.299126,0.270232,0.215073,1.0),
  float4(0.315201,0.285229,0.228009,1.0),
  float4(0.331276,0.300225,0.240946,1.0),
  float4(0.347351,0.315221,0.253882,1.0),
  float4(0.363426,0.330217,0.266819,1.0),
  float4(0.340278,0.308126,0.246745,1.0),
  float4(0.31713,0.286034,0.226671,1.0),
  float4(0.293981,0.263943,0.206597,1.0),
  float4(0.270833,0.241852,0.186523,1.0),
  float4(0.243056,0.215342,0.162435,1.0),
  float4(0.215278,0.188833,0.138346,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.1875,0.162323,0.114258,1.0),
  float4(0.5,0.459717,0.382813,1.0),
  float4(0.5,0.459717,0.382813,1.0),
  float4(0.5,0.459717,0.382813,1.0),
  float4(0.5,0.459717,0.382813,1.0),
  float4(0.5,0.459717,0.382813,1.0),
  float4(0.5,0.459717,0.382813,1.0),
  float4(0.5,0.459717,0.382813,1.0)
};
float4 paletteLookup(float x){
  int i = clamp(int(clamp(x,0.0,1.0)*256.0),0,255);
  return PALETTE[clamp(int(float(i)/255.0*110.0),0,109)];
}

float fieldSample(float3 p){
  float vox = cellSize / 640.0;
  if (cellSize > 1.0) p = floor(p / vox) * vox;
  float n = clamp(fieldVolume(p), 0.0, 1.0);
  return n;
}

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 objPos : TEXCOORD0; };
            Varyings vert(Attributes IN){
                Varyings o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.objPos = IN.positionOS.xyz;
                return o;
            }

            float4 frag(Varyings i) : SV_Target {
                float3 p = i.objPos;
                float s = sin(_Rotation), cs = cos(_Rotation);
                p.xz = mul(p.xz, float2x2(cs, -s, s, cs));
                p = p / max(_Zoom, 0.0001) + float3(_Pan.xy, 0.0);
                float n = fieldSample(p);
                float4 c = paletteLookup(n);
                float3 col = c.rgb * c.a * _Tint.rgb;
                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}