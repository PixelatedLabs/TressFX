﻿using UnityEngine;
using System.Collections;

namespace TressFX
{
	public abstract class ATressFXRender : MonoBehaviour
	{
		/// <summary>
		/// The debug bounding box flag.
		/// </summary>
		public bool debugBoundingBox = false;

		/// <summary>
		/// The shadow shader.
		/// </summary>
		public Shader shadowShader;
		
		/// <summary>
		/// The shadow material.
		/// </summary>
		protected Material shadowMaterial;

		/// <summary>
		/// The TressFX master class.
		/// </summary>
		protected TressFX master;
		
		/// <summary>
		/// The triangle indices buffer.
		/// </summary>
		protected ComputeBuffer g_TriangleIndicesBuffer;
		
		/// <summary>
		/// The triangle meshes.
		/// Meshes are built of indices. Every vertices x-position will contain a triangleindex buffer index.
		/// </summary>
		protected Mesh[] triangleMeshes;
		
		/// <summary>
		/// The line meshes.
		/// </summary>
		protected Mesh[] lineMeshes;
		
		/// <summary>
		/// The rendering bounds.
		/// </summary>
		protected Bounds renderingBounds;

		
		public virtual void Start()
		{
			// Get TressFX master
			this.master = this.GetComponent<TressFX> ();
			
			// Set triangle indices buffer
			this.g_TriangleIndicesBuffer = new ComputeBuffer (this.master.hairData.m_TriangleIndices.Length, 4);
			this.g_TriangleIndicesBuffer.SetData (this.master.hairData.m_TriangleIndices);
			
			// Generate meshes
			this.triangleMeshes = this.GenerateTriangleMeshes ();
			this.lineMeshes = this.GenerateLineMeshes ();
			
			// Initialize shadow material
			this.shadowMaterial = new Material (this.shadowShader);

			// Create render bounds
			Vector3 center = this.master.hairData.m_bSphere.center;
			center.Scale(transform.lossyScale);
            this.renderingBounds = new Bounds (center, new Vector3(this.master.hairData.m_bSphere.radius * transform.lossyScale.x, this.master.hairData.m_bSphere.radius * transform.lossyScale.y, this.master.hairData.m_bSphere.radius * transform.lossyScale.z));

		}
		
		public void Update()
		{
			if (!this.debugBoundingBox)
				return;
			
			Color color = Color.green;
			// Render bounding box
			Vector3 v3Center = this.renderingBounds.center;
			Vector3 v3Extents = this.renderingBounds.extents;
			
			Vector3 v3FrontTopLeft     = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
			Vector3 v3FrontTopRight    = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
			Vector3 v3FrontBottomLeft  = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
			Vector3 v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
			Vector3 v3BackTopLeft      = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
			Vector3 v3BackTopRight        = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
			Vector3 v3BackBottomLeft   = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
			Vector3 v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner
			
			v3FrontTopLeft     = transform.TransformPoint(v3FrontTopLeft);
			v3FrontTopRight    = transform.TransformPoint(v3FrontTopRight);
			v3FrontBottomLeft  = transform.TransformPoint(v3FrontBottomLeft);
			v3FrontBottomRight = transform.TransformPoint(v3FrontBottomRight);
			v3BackTopLeft      = transform.TransformPoint(v3BackTopLeft);
			v3BackTopRight     = transform.TransformPoint(v3BackTopRight);
			v3BackBottomLeft   = transform.TransformPoint(v3BackBottomLeft);
			v3BackBottomRight  = transform.TransformPoint(v3BackBottomRight);  
			
			Debug.DrawLine (v3FrontTopLeft, v3FrontTopRight, color);
			Debug.DrawLine (v3FrontTopRight, v3FrontBottomRight, color);
			Debug.DrawLine (v3FrontBottomRight, v3FrontBottomLeft, color);
			Debug.DrawLine (v3FrontBottomLeft, v3FrontTopLeft, color);
			
			Debug.DrawLine (v3BackTopLeft, v3BackTopRight, color);
			Debug.DrawLine (v3BackTopRight, v3BackBottomRight, color);
			Debug.DrawLine (v3BackBottomRight, v3BackBottomLeft, color);
			Debug.DrawLine (v3BackBottomLeft, v3BackTopLeft, color);
			
			Debug.DrawLine (v3FrontTopLeft, v3BackTopLeft, color);
			Debug.DrawLine (v3FrontTopRight, v3BackTopRight, color);
			Debug.DrawLine (v3FrontBottomRight, v3BackBottomRight, color);
			Debug.DrawLine (v3FrontBottomLeft, v3BackBottomLeft, color);
			
		}
		
		/// <summary>
		/// Generates the triangle meshes.
		/// Meshes are built of indices. Every vertices x-position will contain a triangleindex buffer index.
		/// </summary>
		/// <returns>The triangle meshes.</returns>
		protected Mesh[] GenerateTriangleMeshes()
		{
			// Counter
			int indexCounter = 0;
			MeshBuilder meshBuilder = new MeshBuilder (MeshTopology.Triangles);
			
			// Write all indices to the meshes
			for (int i = 0; i < this.master.hairData.m_TriangleIndices.Length; i+=6)
			{
				// Check for space
				if (!meshBuilder.HasSpace(6))
				{
					// Reset index counter
					indexCounter = 0;
				}
				
				Vector3[] vertices = new Vector3[6];
				Vector3[] normals = new Vector3[6];
				int[] indices = new int[6];
				Vector2[] uvs = new Vector2[6];
				
				// Add vertices
				for (int j = 0; j < 6; j++)
				{
					// Prepare data
					vertices[j] = new Vector3(i+j,0,0);
					normals[j] = Vector3.up;
					indices[j] = indexCounter+j;
					uvs[j] = Vector2.one;
				}
				
				// Add mesh data to builder
				meshBuilder.AddVertices(vertices, indices, uvs, normals);
				
				indexCounter += 6;
			}
			
			return meshBuilder.GetMeshes ();
		}
		
		/// <summary>
		/// Generates the line meshes.
		/// Meshes are built of indices. Every vertices x-position will contain a vertex list index.
		/// </summary>
		/// <returns>The line meshes.</returns>
		protected Mesh[] GenerateLineMeshes()
		{
			// Counter
			int indexCounter = 0;
			MeshBuilder meshBuilder = new MeshBuilder (MeshTopology.Lines);
			
			// Write all indices to the meshes
			for (int i = 0; i < this.master.hairData.m_pVertices.Length; i+=2)
			{
				// Check for space
				if (!meshBuilder.HasSpace(2))
				{
					// Reset index counter
					indexCounter = 0;
				}
				
				Vector3[] vertices = new Vector3[2];
				Vector3[] normals = new Vector3[2];
				int[] indices = new int[2];
				Vector2[] uvs = new Vector2[2];
				
				// Add vertices
				for (int j = 0; j < 2; j++)
				{
					// Prepare data
					vertices[j] = new Vector3(this.master.hairData.m_LineIndices[i+j],0,0);
					normals[j] = Vector3.up;
					indices[j] = indexCounter+j;
					uvs[j] = Vector2.one;
				}
				
				// Add mesh data to builder
				meshBuilder.AddVertices(vertices, indices, uvs, normals);
				
				indexCounter += 2;
			}
			
			return meshBuilder.GetMeshes ();
		}
		
		/// <summary>
		/// Raises the destroy event.
		/// Releases all resources not needed any more.
		/// </summary>
		public void OnDestroy()
		{
			this.g_TriangleIndicesBuffer.Release ();
		}
		
		/// <summary>
		/// Convertes a Matrix4x4 to a float array.
		/// </summary>
		/// <returns>The to float array.</returns>
		/// <param name="matrix">Matrix.</param>
		protected static float[] MatrixToFloatArray(Matrix4x4 matrix)
		{
			return new float[] 
			{
				matrix.m00, matrix.m01, matrix.m02, matrix.m03,
				matrix.m10, matrix.m11, matrix.m12, matrix.m13,
				matrix.m20, matrix.m21, matrix.m22, matrix.m23,
				matrix.m30, matrix.m31, matrix.m32, matrix.m33
			};
		}
	}
}
