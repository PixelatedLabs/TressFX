﻿Shader "TressFX/Hair Rendering Shader"
{
    SubShader
    {
        Pass
        {
    		Tags { "LightMode" = "ForwardBase" }
        	Blend SrcAlpha OneMinusSrcAlpha // turn on alpha blending
        	ZWrite On
        	Cull Off
        	
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            #pragma multi_compile_fwdbase
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            
            #include "UnityCG.cginc"
 
            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> _VertexPositionBuffer;
            StructuredBuffer<int> _StrandIndicesBuffer;
            uniform float4 _HairColor;
            uniform float _HairThickness;
            uniform float4 _CameraDirection;
 
            //A simple input struct for our pixel shader step containing a position.
            struct ps_input {
                float4 pos : SV_POSITION;
                int vertexIndex : COLOR0;
            };
            
 
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                ps_input o;
                
                // Position transformation
                o.pos = mul (UNITY_MATRIX_VP, float4(_VertexPositionBuffer[id],1.0f));
                o.vertexIndex = id;
                
                return o;
            }

			[maxvertexcount(6)]
			void geom (line ps_input input[2], inout LineStream<ps_input> outStream)
			{
				/*outStream.Append(input[0]);
				if (_StrandIndicesBuffer[input[0].vertexIndex+1] == 0)
				{
					outStream.RestartStrip();
				}
				outStream.Append(input[1]);*/
				
				ps_input vertices[4];
				vertices[0] = (ps_input)0;
				vertices[1] = (ps_input)0;
				vertices[2] = (ps_input)0;
				vertices[3] = (ps_input)0;
				
				float4 v = input[1].pos - input[0].pos;
				float3 linedirection = float3(v.x, v.y, v.z);
				
				float4 direction1 = float4(cross(linedirection, float3(_CameraDirection.x, _CameraDirection.y, _CameraDirection.z)), 0.0f);
				direction1 = mul(_HairThickness/2, normalize(direction1));
				vertices[0].pos = input[0].pos + direction1;
				vertices[1].pos = input[0].pos - direction1;
				vertices[2].pos = input[1].pos + direction1;
				vertices[3].pos = input[1].pos - direction1;
				
				outStream.Append(vertices[0]);
				outStream.Append(vertices[1]);
				outStream.Append(vertices[2]);
				outStream.Append(vertices[1]);
				outStream.Append(vertices[2]);
				outStream.Append(vertices[3]);
			}
 
            //Pixel function returns a solid color for each point.
            float4 frag (ps_input i) : COLOR
            {
                return _HairColor;
            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
}