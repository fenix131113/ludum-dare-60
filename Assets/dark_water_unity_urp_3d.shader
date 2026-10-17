Shader "Kaleidos/dark_water 3D URP" {
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




#define spinPhase (uTime*0.35)
static const float jitter = 1.0;
static const float minkowskiP = 2.4;
static const float starPoints = 6.0;
static const float starDepth = 0.0;
static const float cellStretch = 1.0;
static const float cellAngle = 0.0;
static const float cellRadius = 1.1;
static const float smoothK = 0.0;
static const float edgeSharpen = 3.0;
static const float octaves = 2.0;
static const float gain = 0.8;

static const float4 PALETTE[110] = {
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.109945,0.336806,0.315027,1.0),
  float4(0.107096,0.329861,0.308476,1.0),
  float4(0.104248,0.322917,0.301924,1.0),
  float4(0.1014,0.315972,0.295373,1.0),
  float4(0.0985514,0.309028,0.288822,1.0),
  float4(0.0957031,0.302083,0.282271,1.0),
  float4(0.0928548,0.295139,0.27572,1.0),
  float4(0.0900065,0.288194,0.269168,1.0),
  float4(0.0871582,0.28125,0.262617,1.0),
  float4(0.0743408,0.25,0.233137,1.0),
  float4(0.0615234,0.21875,0.203656,1.0),
  float4(0.0679321,0.234375,0.218396,1.0),
  float4(0.0743408,0.25,0.233137,1.0),
  float4(0.0775452,0.257813,0.240507,1.0),
  float4(0.0807495,0.265625,0.247877,1.0),
  float4(0.0839539,0.273438,0.255247,1.0),
  float4(0.0871582,0.28125,0.262617,1.0),
  float4(0.0903625,0.289063,0.269987,1.0),
  float4(0.0935669,0.296875,0.277357,1.0),
  float4(0.0967712,0.304688,0.284728,1.0),
  float4(0.0999756,0.3125,0.292098,1.0),
  float4(0.10318,0.320313,0.299468,1.0),
  float4(0.106384,0.328125,0.306838,1.0),
  float4(0.109589,0.335938,0.314208,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.112793,0.34375,0.321578,1.0),
  float4(0.117841,0.3537,0.331057,1.0),
  float4(0.120365,0.358675,0.335797,1.0),
  float4(0.122889,0.363649,0.340536,1.0),
  float4(0.125413,0.368624,0.345276,1.0),
  float4(0.127937,0.373599,0.350016,1.0),
  float4(0.130461,0.378574,0.354755,1.0),
  float4(0.132985,0.383549,0.359495,1.0),
  float4(0.135509,0.388524,0.364234,1.0),
  float4(0.138033,0.393499,0.368974,1.0),
  float4(0.140557,0.398473,0.373713,1.0),
  float4(0.143081,0.403448,0.378453,1.0),
  float4(0.145605,0.408423,0.383193,1.0),
  float4(0.148129,0.413398,0.387932,1.0),
  float4(0.150653,0.418373,0.392672,1.0),
  float4(0.153177,0.423348,0.397411,1.0),
  float4(0.155701,0.428323,0.402151,1.0),
  float4(0.158225,0.433297,0.40689,1.0),
  float4(0.160749,0.438272,0.41163,1.0),
  float4(0.163273,0.443247,0.41637,1.0),
  float4(0.165797,0.448222,0.421109,1.0),
  float4(0.168321,0.453197,0.425849,1.0),
  float4(0.170845,0.458172,0.430588,1.0),
  float4(0.173369,0.463147,0.435328,1.0),
  float4(0.175893,0.468121,0.440067,1.0),
  float4(0.178417,0.473096,0.444807,1.0),
  float4(0.180941,0.478071,0.449547,1.0),
  float4(0.183465,0.483046,0.454286,1.0),
  float4(0.185989,0.488021,0.459026,1.0),
  float4(0.188513,0.492996,0.463765,1.0),
  float4(0.191037,0.497971,0.468505,1.0),
  float4(0.193561,0.502945,0.473244,1.0),
  float4(0.196085,0.50792,0.477984,1.0),
  float4(0.198609,0.512895,0.482724,1.0),
  float4(0.201133,0.51787,0.487463,1.0),
  float4(0.203657,0.522845,0.492203,1.0),
  float4(0.206181,0.52782,0.496942,1.0),
  float4(0.208705,0.532795,0.501682,1.0),
  float4(0.211229,0.537769,0.506421,1.0),
  float4(0.213753,0.542744,0.511161,1.0),
  float4(0.216277,0.547719,0.515901,1.0),
  float4(0.218801,0.552694,0.52064,1.0),
  float4(0.221325,0.557669,0.52538,1.0),
  float4(0.223849,0.562644,0.530119,1.0),
  float4(0.226373,0.567619,0.534859,1.0),
  float4(0.247559,0.609375,0.574641,1.0),
  float4(0.231421,0.577568,0.544338,1.0),
  float4(0.233945,0.582543,0.549078,1.0),
  float4(0.236469,0.587518,0.553817,1.0),
  float4(0.247559,0.609375,0.574641,1.0),
  float4(0.241517,0.597468,0.563296,1.0),
  float4(0.242524,0.599452,0.565187,1.0),
  float4(0.243531,0.601437,0.567078,1.0),
  float4(0.244538,0.603421,0.568968,1.0),
  float4(0.245545,0.605406,0.570859,1.0),
  float4(0.246552,0.60739,0.57275,1.0),
  float4(0.247559,0.609375,0.574641,1.0)
};
float4 paletteLookup(float x){
  int i = clamp(int(clamp(x,0.0,1.0)*256.0),0,255);
  return PALETTE[clamp(int(float(i)/255.0*110.0),0,109)];
}

#define TAU 6.28318530718
#define MAX_OCTAVES 10
#define VOLUME_Z    0.7

float2 latticeOffset(float2 cell) {
    float row = floor(cell.y);
    return (float2)(0.0);
}

float3 hash3(float3 p) {
    p = float3(dot(p, float3(127.1, 311.7, 74.7)),
             dot(p, float3(269.5, 183.3, 246.1)),
             dot(p, float3(113.5, 271.9, 124.6)));
    return frac(sin(p) * 43758.5453);
}

float metricDist3(float3 d, float ang) {
    float s = sin(ang), c = cos(ang);
    d.xy = mul(d.xy, float2x2(c, -s, s, c));
    d.x *= cellStretch; d.y /= cellStretch;
    float p  = max(minkowskiP, 0.1);
    float md = pow(pow(abs(d.x), p) + pow(abs(d.y), p) + pow(abs(d.z), p), 1.0 / p);
    return md * (1.0 + starDepth * cos(starPoints * atan2(d.y, d.x)));
}

float cellSpin(float2 cell) {
    float turns = cellAngle / 360.0;
    turns += spinPhase;
    return -turns * TAU;
}

float smin(float a, float b, float k) {
    if (k <= 0.0) return min(a, b);
    float h = clamp(0.5 + 0.5 * (a - b) / k, 0.0, 1.0);
    return lerp(a, b, h) - k * h * (1.0 - h);
}

void consider(float d, inout float f1, inout float f2) {
    if      (d < f1) f2 = f1;
    else if (d < f2) f2 = d;
    f1 = smin(f1, d, smoothK);
}

float comboValue(float f1, float f2) {
    float s = 1.0 / max(cellRadius, 0.001);
    f1 *= s; f2 *= s;
    float v;
    v = f1 / max(f2, 0.001);
    v = clamp(v, 0.0, 1.0);
    v = pow(v, edgeSharpen);
    
    return clamp(v, 0.0, 1.0);
}

#define CELL_EMPTY_LO 4.0
#define CELL_EMPTY_HI 7.0
#define CELL_EMPTY_VALUE 1.0

static float2 cBaseMouse;

float cellFinish(float f1, float f2, bool doCells) {
    float v = comboValue(f1, f2);
    if (doCells) v = lerp(v, CELL_EMPTY_VALUE, smoothstep(CELL_EMPTY_LO, CELL_EMPTY_HI, f1));
    
    return v;
}

float cellFieldVol(float3 P){
  float3 ip = floor(P), fp = frac(P);
  float f1 = 8.0, f2 = 8.0;
  for (int k = -1; k <= 1; k++) {
    for (int j = -2; j <= 2; j++) {
      for (int i = -2; i <= 2; i++) {
        float3 g    = float3(float(i), float(j), float(k));
        float3 cell = ip + g;
        float3 rnd  = hash3(cell);
        float3 pt   = g + float3(lerp((float2)(0.5), rnd.xy, jitter), rnd.z);
        pt.xy    += latticeOffset(cell.xy) * (1.0 - jitter);
        float3 d    = pt - fp;
        d.z      *= VOLUME_Z;
        consider(metricDist3(d, cellSpin(cell.xy)), f1, f2);
      }
    }
  }
  return cellFinish(f1, f2, false);
}
float cellFBMVol(float3 p){
  float v = 0.0, a = 0.5, norm = 0.0;
  int oct = int(octaves + 0.5);
  for (int i = 0; i < MAX_OCTAVES; i++) {
    if (i >= oct) break;
    v    += a * cellFieldVol(p);
    norm += a;
    p     = p * 2.0 + float3(5.3, 1.7, 3.1);
    a    *= gain;
  }
  return v / norm;
}

float fieldSample(float3 p){
  float vox = cellSize / 640.0;
  if (cellSize > 1.0) p = floor(p / vox) * vox;
  float2 pl = p.xz * 10.0;
  float pz = p.y * 10.0 + uTime * 0.5;
  float n = clamp(cellFBMVol(float3(pl, pz)), 0.0, 1.0);
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