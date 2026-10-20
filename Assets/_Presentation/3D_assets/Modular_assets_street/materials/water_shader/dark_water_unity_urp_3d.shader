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
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
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
  float4(0.0761719,0.203125,0.190938,0.904762),
  float4(0.0778114,0.205419,0.193168,0.904762),
  float4(0.079451,0.207712,0.195399,0.904762),
  float4(0.0810905,0.210006,0.19763,0.904762),
  float4(0.0827301,0.212299,0.199861,0.904762),
  float4(0.0843696,0.214593,0.202092,0.904762),
  float4(0.0860092,0.216886,0.204322,0.904762),
  float4(0.0876487,0.21918,0.206553,0.904762),
  float4(0.0892883,0.221474,0.208784,0.904762),
  float4(0.0909278,0.223767,0.211015,0.904762),
  float4(0.0925674,0.226061,0.213246,0.904762),
  float4(0.0942069,0.228354,0.215476,0.904762),
  float4(0.0958465,0.230648,0.217707,0.904762),
  float4(0.097486,0.232942,0.219938,0.904762),
  float4(0.0991256,0.235235,0.222169,0.904762),
  float4(0.100765,0.237529,0.224399,0.904762),
  float4(0.102405,0.239822,0.22663,0.904762),
  float4(0.104044,0.242116,0.228861,0.904762),
  float4(0.105684,0.244409,0.231092,0.904762),
  float4(0.107323,0.246703,0.233323,0.904762),
  float4(0.108963,0.248997,0.235553,0.904762),
  float4(0.110602,0.25129,0.237784,0.904762),
  float4(0.112242,0.253584,0.240015,0.904762),
  float4(0.113882,0.255877,0.242246,0.904762),
  float4(0.115521,0.258171,0.244477,0.904762),
  float4(0.117161,0.260464,0.246707,0.904762),
  float4(0.1188,0.262758,0.248938,0.904762),
  float4(0.12044,0.265052,0.251169,0.904762),
  float4(0.122079,0.267345,0.2534,0.904762),
  float4(0.123719,0.269639,0.255631,0.904762),
  float4(0.125358,0.271932,0.257861,0.904762),
  float4(0.126998,0.274226,0.260092,0.904762),
  float4(0.128637,0.276519,0.262323,0.904762),
  float4(0.130277,0.278813,0.264554,0.904762),
  float4(0.131917,0.281107,0.266785,0.904762),
  float4(0.133556,0.2834,0.269015,0.904762),
  float4(0.135196,0.285694,0.271246,0.904762),
  float4(0.136835,0.287987,0.273477,0.904762),
  float4(0.138475,0.290281,0.275708,0.904762),
  float4(0.140114,0.292575,0.277939,0.904762),
  float4(0.141754,0.294868,0.280169,0.904762),
  float4(0.143393,0.297162,0.2824,0.904762),
  float4(0.145033,0.299455,0.284631,0.904762),
  float4(0.146673,0.301749,0.286862,0.904762),
  float4(0.148312,0.304042,0.289092,0.904762),
  float4(0.149952,0.306336,0.291323,0.904762),
  float4(0.151591,0.30863,0.293554,0.904762),
  float4(0.153231,0.310923,0.295785,0.904762),
  float4(0.15487,0.313217,0.298016,0.904762),
  float4(0.15651,0.31551,0.300246,0.904762),
  float4(0.158149,0.317804,0.302477,0.904762),
  float4(0.159789,0.320097,0.304708,0.904762),
  float4(0.161428,0.322391,0.306939,0.904762),
  float4(0.163068,0.324685,0.30917,0.904762),
  float4(0.164708,0.326978,0.3114,0.904762),
  float4(0.166347,0.329272,0.313631,0.904762),
  float4(0.167987,0.331565,0.315862,0.904762),
  float4(0.169626,0.333859,0.318093,0.904762),
  float4(0.171266,0.336153,0.320324,0.904762),
  float4(0.172905,0.338446,0.322554,0.904762),
  float4(0.174545,0.34074,0.324785,0.904762),
  float4(0.176184,0.343033,0.327016,0.904762),
  float4(0.177824,0.345327,0.329247,0.904762),
  float4(0.179464,0.34762,0.331478,0.904762),
  float4(0.181103,0.349914,0.333708,0.904762),
  float4(0.182743,0.352208,0.335939,0.904762),
  float4(0.184382,0.354501,0.33817,0.904762),
  float4(0.186022,0.356795,0.340401,0.904762),
  float4(0.187661,0.359088,0.342631,0.904762),
  float4(0.189301,0.361382,0.344862,0.904762),
  float4(0.19094,0.363675,0.347093,0.904762),
  float4(0.19258,0.365969,0.349324,0.904762),
  float4(0.194219,0.368263,0.351555,0.904762),
  float4(0.195859,0.370556,0.353785,0.904762),
  float4(0.197499,0.37285,0.356016,0.904762),
  float4(0.199138,0.375143,0.358247,0.904762),
  float4(0.200778,0.377437,0.360478,0.904762),
  float4(0.202417,0.379731,0.362709,0.904762),
  float4(0.204057,0.382024,0.364939,0.904762),
  float4(0.205696,0.384318,0.36717,0.904762),
  float4(0.207336,0.386611,0.369401,0.904762),
  float4(0.208975,0.388905,0.371632,0.904762),
  float4(0.210615,0.391198,0.373863,0.904762),
  float4(0.212255,0.393492,0.376093,0.904762),
  float4(0.213894,0.395786,0.378324,0.904762),
  float4(0.215534,0.398079,0.380555,0.904762),
  float4(0.217173,0.400373,0.382786,0.904762),
  float4(0.218813,0.402666,0.385017,0.904762),
  float4(0.220452,0.40496,0.387247,0.904762),
  float4(0.222092,0.407253,0.389478,0.904762),
  float4(0.223731,0.409547,0.391709,0.904762),
  float4(0.225371,0.411841,0.39394,0.904762),
  float4(0.22701,0.414134,0.39617,0.904762),
  float4(0.22865,0.416428,0.398401,0.904762),
  float4(0.23029,0.418721,0.400632,0.904762),
  float4(0.231929,0.421015,0.402863,0.904762),
  float4(0.233569,0.423308,0.405094,0.904762),
  float4(0.235208,0.425602,0.407324,0.904762),
  float4(0.236848,0.427896,0.409555,0.904762),
  float4(0.238487,0.430189,0.411786,0.904762),
  float4(0.240127,0.432483,0.414017,0.904762),
  float4(0.241766,0.434776,0.416248,0.904762),
  float4(0.243406,0.43707,0.418478,0.904762),
  float4(0.245046,0.439364,0.420709,0.904762),
  float4(0.246685,0.441657,0.42294,0.904762),
  float4(0.248325,0.443951,0.425171,0.904762),
  float4(0.249964,0.446244,0.427402,0.904762),
  float4(0.251604,0.448538,0.429632,0.904762),
  float4(0.253243,0.450831,0.431863,0.904762),
  float4(0.254883,0.453125,0.434094,0.904762)
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
                float3 col = c.rgb * _Tint.rgb;
                return float4(col, c.a * _Tint.a);
            }
            ENDHLSL
        }
    }
}