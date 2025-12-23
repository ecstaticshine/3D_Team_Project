Shader "Custom/NeonUIShader"
{
    Properties
    {
        // UI Image의 기본 텍스처를 받아옵니다.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        // 네온의 기본 색상을 설정합니다. (HDR 설정)
        [HDR] _NeonColor ("Neon Color", Color) = (0, 1, 0, 1)
        // 깜빡이는 속도를 조절합니다.
        _FlickerSpeed ("Flicker Speed", Float) = 5.0
        // 최소 밝기를 설정하여 완전히 꺼지지 않게 합니다.
        _MinIntensity ("Min Intensity", Range(0, 1)) = 0.2
    }

    SubShader
    {
        // UI 환경에서 렌더링되기 위한 필수 태그 설정들입니다.
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        
        // UI의 알파 블렌딩(투명도)을 활성화합니다.
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert // 버텍스 쉐이더 함수 정의
            #pragma fragment frag // 프래그먼트(픽셀) 쉐이더 함수 정의
            #include "UnityCG.cginc"

            // 쉐이더 내부에서 사용할 변수들을 선언합니다.
            struct appdata_t {
                float4 vertex   : POSITION; // 정점 위치
                float2 texcoord : TEXCOORD0; // UV 좌표
                float4 color    : COLOR; // 정점 색상 (UI Color)
            };

            struct v2f {
                float4 vertex   : SV_POSITION; // 화면상 정점 위치
                float2 texcoord  : TEXCOORD0; // 전달할 UV 좌표
                fixed4 color    : COLOR; // 전달할 색상
            };

            sampler2D _MainTex; // 텍스처 데이터 저장 변수
            fixed4 _NeonColor; // 설정한 네온 색상 변수
            float _FlickerSpeed; // 깜빡임 속도 변수
            float _MinIntensity; // 최소 밝기 변수

            // 정점 쉐이더: 모델의 점들을 화면 좌표로 변환합니다.
            v2f vert (appdata_t v)
            {
                v2f o; // 출력 구조체 생성
                o.vertex = UnityObjectToClipPos(v.vertex); // 정점 좌표 변환
                o.texcoord = v.texcoord; // UV 좌표 전달
                o.color = v.color; // UI 컴포넌트의 색상값 전달
                return o;
            }

            // 프래그먼트 쉐이더: 각 픽셀의 최종 색상을 결정합니다.
            fixed4 frag (v2f i) : SV_Target
            {
                // 텍스처의 색상과 알파(투명도) 값을 읽어옵니다.
                fixed4 col = tex2D(_MainTex, i.texcoord);
                
                // 시간(Time)과 Sin 함수를 사용하여 -1 ~ 1 사이로 진동하는 값을 만듭니다.
                // 여기에 절대값(abs)을 취해 0 ~ 1 사이로 만들고 최소 밝기를 더합니다.
                float noise = abs(sin(_Time.y * _FlickerSpeed)); 
                float flicker = max(_MinIntensity, noise);

                // 최종 색상 계산: (PNG 로고 형태) * (설정한 네온 색상) * (깜빡임 수치)
                // RGB 값에 flicker 수치를 곱해 1이 넘어가면 Bloom 효과가 발생합니다.
                fixed4 finalColor = col * _NeonColor * flicker;
                
                // 원본 PNG의 투명도(Alpha)를 유지하여 로고 형태를 보존합니다.
                finalColor.a = col.a * i.color.a;

                return finalColor; // 최종 결과값을 화면에 출력합니다.
            }
            ENDCG
        }
    }
}