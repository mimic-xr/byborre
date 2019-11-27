using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DeformBodyProxy : MonoBehaviour
{
	public DeformBody source;
	private SkinnedMeshRenderer skinnedMeshRenderer;
	private MeshFilter meshFilter;
	private bool hasLoggedError = false;

	/**
	 * Register event handlers for the delegates
	 **/
	protected void OnEnable()
	{
		DeformManager.PreSimulationStarted += Initialize;
		DeformManager.OnSimulationUpdated += OnSimulationUpdated;
	}

	/**
     * Unregister event handlers for the delegates
     **/
	protected void OnDisable()
	{
		DeformManager.PreSimulationStarted -= Initialize;
		DeformManager.OnSimulationUpdated -= OnSimulationUpdated;
	}

	public virtual void OnValidate()
	{
		if (!Application.isPlaying)
		{
			Invoke("SetMeshes", 0.0f);
		}
	}

	private void SetMeshes()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		meshFilter = GetComponent<MeshFilter>();

		if (!source) return;

		if (skinnedMeshRenderer && source.GetComponent<SkinnedMeshRenderer>())
		{
			skinnedMeshRenderer.sharedMesh = source.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		}
		else if (meshFilter && source.GetComponent<MeshFilter>())
		{
			meshFilter.sharedMesh = source.GetComponent<MeshFilter>().sharedMesh;
		}
	}
	
	/**
     * Adds and initializes the DeformBody to the simulation engine
     **/
	void Initialize()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		meshFilter = GetComponent<MeshFilter>();
	}

	void OnSimulationUpdated()
	{
		// Do something you want to time
		if (skinnedMeshRenderer && source.GetComponent<SkinnedMeshRenderer>())
		{
			skinnedMeshRenderer.sharedMesh = source.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		}
		else if (meshFilter && source.GetComponent<MeshFilter>())
		{
			meshFilter.sharedMesh = source.GetComponent<MeshFilter>().sharedMesh;
		} else
		{
			LogError();
		}
	}

	void LogError()
	{
		if (!hasLoggedError)
		{
			Debug.LogWarning("Make sure that the DeformBodyProxy object has a either a MeshFilter and MeshRenderer or SkinnedMeshRenderer corresponding to the source object!");
			hasLoggedError = true;
		}
	}

	[MenuItem("GameObject/Deform Dynamics/Deform Body Proxy", false, 1)]
	static void CreateDeformBodyProxy(MenuCommand menuCommand)
	{
		GameObject c = new GameObject("DeformBodyProxy");
		GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
		Undo.RegisterCreatedObjectUndo(c, "Create Deform Body Proxy");
		c.AddComponent<DeformBodyProxy>();
		Selection.activeObject = c;

	}
}