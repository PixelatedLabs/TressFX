using UnityEngine;
using System.Collections;

namespace TressFX
{
	/// <summary>
	/// Tress FX main class.
	/// </summary>
	public class TressFX : MonoBehaviour
	{
		public TressFXHair hairData;

		/// <summary>
		/// The hair vertex positions buffer.
		/// </summary>
		public ComputeBuffer g_HairVertexPositions;
		public ComputeBuffer g_HairVertexPositionsPrev;
		public ComputeBuffer g_HairVertexTangents;
		public ComputeBuffer g_InitialHairPositions;
		public ComputeBuffer g_GlobalRotations;
		public ComputeBuffer g_LocalRotations;

		public ComputeBuffer g_HairThicknessCoeffs;
		public ComputeBuffer g_HairRestLengthSRV;
		public ComputeBuffer g_HairStrandType;
		public ComputeBuffer g_HairRefVecsInLocalFrame;
		public ComputeBuffer g_FollowHairRootOffset;
		public ComputeBuffer g_TexCoords;

		[HideInInspector]
		public TressFXSimulation simulation;

		/// <summary>
		/// Start this instance.
		/// Initializes all buffers and other resources needed by tressfx simulation and rendering.
		/// </summary>
		public void Start()
		{
			if (this.hairData == null)
			{
				Debug.LogError("No hair data assigned to TressFX :(");
			}

			// Vertex buffers
			this.g_HairVertexPositions = this.InitializeBuffer(this.GetHairVertices(), 16);
			this.g_HairVertexPositionsPrev = this.InitializeBuffer(this.GetHairVertices(), 16);
			this.g_InitialHairPositions = this.InitializeBuffer(this.GetHairVertices(), 16);

			// Tangents and rotations
			this.g_HairVertexTangents = this.InitializeBuffer(this.hairData.m_pTangents, 16);
			this.g_GlobalRotations = this.InitializeBuffer(this.hairData.m_pGlobalRotations, 16);
			this.g_LocalRotations = this.InitializeBuffer(this.hairData.m_pLocalRotations, 16);

			// Others
			this.g_HairRestLengthSRV = this.InitializeBuffer(this.GetHairRestLengths(), 4);
			this.g_HairStrandType = this.InitializeBuffer(this.hairData.m_pHairStrandType, 4);
			this.g_HairRefVecsInLocalFrame = this.InitializeBuffer(this.GetHairRefVectors(), 16);
			this.g_FollowHairRootOffset = this.InitializeBuffer(this.GetHairRootOffsets(), 16);
			this.g_HairThicknessCoeffs = this.InitializeBuffer(this.GetHairThicknessCoeffs(), 4);
			this.g_TexCoords = this.InitializeBuffer(this.hairData.m_TexCoords, 16);

			// Get other parts
			this.simulation = this.GetComponent<TressFXSimulation>();
		}

		/// <summary>
		/// Initializes the a new ComputeBuffer.
		/// </summary>
		/// <returns>The buffer.</returns>
		/// <param name="data">Data.</param>
		/// <param name="stride">Stride.</param>
		private ComputeBuffer InitializeBuffer(System.Array data, int stride)
		{
			ComputeBuffer returnBuffer = new ComputeBuffer(data.Length, stride);
			returnBuffer.SetData(data);
			return returnBuffer;
		}

		/// <summary>
		/// Raises the destroy event.
		/// Cleans up all used resources.
		/// </summary>
		public void OnDestroy()
		{
			this.g_HairVertexPositions.Release();
			this.g_HairVertexPositionsPrev.Release();
			this.g_InitialHairPositions.Release();

			this.g_HairVertexTangents.Release();
			this.g_GlobalRotations.Release();
			this.g_LocalRotations.Release();

			this.g_HairThicknessCoeffs.Release();
			this.g_HairRestLengthSRV.Release();
			this.g_HairStrandType.Release();
			this.g_HairRefVecsInLocalFrame.Release();
			this.g_FollowHairRootOffset.Release();
			this.g_TexCoords.Release();
		}

		Vector4[] GetHairVertices()
		{
			Vector4[] result = new Vector4[this.hairData.m_pVertices.Length];
			for (int i = 0; i < this.hairData.m_pVertices.Length; i++)
			{
				result[i].x = this.hairData.m_pVertices[i].x * transform.lossyScale.x;
				result[i].y = this.hairData.m_pVertices[i].y * transform.lossyScale.y;
				result[i].z = this.hairData.m_pVertices[i].z * transform.lossyScale.z;
				result[i].w = this.hairData.m_pVertices[i].w * transform.lossyScale.z;
			}
			return result;
		}

		float[] GetHairRestLengths()
		{
			float[] result = new float[this.hairData.m_pRestLengths.Length];
			for (int i = 0; i < this.hairData.m_pRestLengths.Length; i++)
			{
				result[i] = this.hairData.m_pRestLengths[i] * transform.lossyScale.y;
			}
			return result;
		}

		Vector4[] GetHairRootOffsets()
		{
			Vector4[] result = new Vector4[this.hairData.m_pFollowRootOffset.Length];
			for (int i = 0; i < this.hairData.m_pFollowRootOffset.Length; i++)
			{
				result[i].x = this.hairData.m_pFollowRootOffset[i].x * transform.lossyScale.x;
				result[i].y = this.hairData.m_pFollowRootOffset[i].y * transform.lossyScale.y;
				result[i].z = this.hairData.m_pFollowRootOffset[i].z * transform.lossyScale.z;
				result[i].w = this.hairData.m_pFollowRootOffset[i].w * transform.lossyScale.z;
			}
			return result;
		}

		Vector4[] GetHairRefVectors()
		{
			Vector4[] result = new Vector4[this.hairData.m_pRefVectors.Length];
			for (int i = 0; i < this.hairData.m_pRefVectors.Length; i++)
			{
				result[i].x = this.hairData.m_pRefVectors[i].x * transform.lossyScale.x;
				result[i].y = this.hairData.m_pRefVectors[i].y * transform.lossyScale.y;
				result[i].z = this.hairData.m_pRefVectors[i].z * transform.lossyScale.z;
				result[i].w = this.hairData.m_pRefVectors[i].w * transform.lossyScale.z;
			}
			return result;
		}

		float[] GetHairThicknessCoeffs()
		{
			float[] result = new float[this.hairData.m_pThicknessCoeffs.Length];
			for (int i = 0; i < this.hairData.m_pThicknessCoeffs.Length; i++)
			{
				result[i] = this.hairData.m_pThicknessCoeffs[i] * transform.lossyScale.x;
			}
			return result;
		}
	}
}