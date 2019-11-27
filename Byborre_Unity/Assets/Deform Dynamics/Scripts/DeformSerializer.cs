using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
//using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Serializer")]
public class DeformSerializer : MonoBehaviour
{
	public string fileName;

	public void Save()
	{
		DeformDynamics.DeformPlugin.Serialization.SaveSerializedData(fileName);

		// Serialize unity specific data (transform, start/end points of seams, materials, stiffnesses, etc)
		DeformBody[] deformObjects = FindObjectsOfType<DeformBody>();
		string deformObjectsJson = "";


		for (int i = 0; i < deformObjects.Length; i++)
		{
			Wireframe wf = deformObjects[i].GetComponent<Wireframe>();
			int num = deformObjects[i].gameObject.GetComponents<Wireframe>().Length;

			bool wf_exists = false;

			if (wf && wf.enabled)
			{
				wf_exists = true;
			}

			DeformObjectJson doj = new DeformObjectJson
			{
				id = deformObjects[i].GetId(),

				name = deformObjects[i].name,

				position = deformObjects[i].transform.position,
				rotation = deformObjects[i].transform.rotation,
				scale = deformObjects[i].transform.localScale,

				//disableRendering = deformObjects[i].disableRendering,
				material = deformObjects[i].GetComponent<MeshRenderer>().sharedMaterial,
				wireframe = wf_exists
			};

			deformObjectsJson += JsonUtility.ToJson(doj) + "\n";
		}

		DeformSeam[] deformSeams = FindObjectsOfType<DeformSeam>();

		string deformSeamsJson = "";

		for (int i = 0; i < deformSeams.Length; i++)
		{
			DeformSeamJson dsj = new DeformSeamJson
			{
				id = deformSeams[i].seamId,

				name = deformSeams[i].name,

				bodyASeamBeginPointOld	= deformSeams[i].bodyASeamBeginPointOld,
				bodyBSeamBeginPointOld	= deformSeams[i].bodyBSeamBeginPointOld,
				bodyASeamEndPointOld	= deformSeams[i].bodyASeamEndPointOld,
				bodyBSeamEndPointOld	= deformSeams[i].bodyBSeamEndPointOld
			};

			deformSeamsJson += JsonUtility.ToJson(dsj) + "\n";
		}
		
		// Create the path to the unity specific file
		string path = fileName.Substring(0, fileName.LastIndexOf("/") + 1);
		string name = fileName.Substring(fileName.LastIndexOf("/") + 1);
		name = name.Substring(0, name.IndexOf(".def"));
		string completePath = path + name + ".unity.def";

		// Enables us to identify which part of the string that contains the objects, and the seams
		// Needed for the deserialization, but it could be done more elegantly
		string deformJson = deformObjectsJson + "@@@" + deformSeamsJson;

		File.WriteAllText(completePath, deformJson);

		// Write the data
		//FileStream fs = new FileStream(completePath, FileMode.Create);
		//BinaryFormatter formatter = new BinaryFormatter();
		//formatter.Serialize(fs, deformJson);
		//fs.Close();
	}

	public void Load()
	{
		DeformDynamics.DeformPlugin.InitializePlugin();

		int num_objects;
		int num_seams;

		string pathToFile = fileName;

		// If the user loads the unity file
		if (fileName.Contains(".unity.def"))
		{
			pathToFile = pathToFile.Remove(pathToFile.IndexOf(".unity.def"), ".unity".Length);
		}

        DeformDynamics.DeformPlugin.Serialization.LoadSerializedData(pathToFile, out num_objects, out num_seams);

		// Create the correct unity file path
		string pathToFileUnity = pathToFile;
		pathToFileUnity = pathToFileUnity.Substring(0, pathToFile.IndexOf(".def"));
		pathToFileUnity = pathToFileUnity + ".unity.def";

		// Read the unity specific data
		//FileStream fs = new FileStream(pathToFileUnity, FileMode.Open);
		//BinaryFormatter formatter = new BinaryFormatter();
		//string deformJsonString = (string)formatter.Deserialize(fs);
		//fs.Close();


		string deformJsonString1 = File.ReadAllText(pathToFileUnity);

		//Debug.Log(deformJsonString1);

		byte[] bytes = System.Text.Encoding.ASCII.GetBytes(deformJsonString1);

		string deformJsonString = System.Text.Encoding.UTF8.GetString(bytes);

		// Divide the whole string into two strings, one containing the object data and one containing
		// the seam data
		string deformObjectJsonString	= deformJsonString.Substring(0, deformJsonString.IndexOf("@@@"));
		string deformSeamJsonString		= deformJsonString.Substring(deformJsonString.IndexOf("@@@") + 3);

		// Create an array of json strings, one per object/seam
		string[] deformObjectJson	= deformObjectJsonString.Split('\n');
		string[] deformSeamJson		= deformSeamJsonString.Split('\n');

		


		List<DeformBody> loadedDeformBodies = new List<DeformBody>();

		for (int object_id = 0; object_id < num_objects; object_id++)
		{
			int num_vertices;
			int num_indices;
			int num_rv_distance;
			int num_rv_bending;

			DeformObjectJson serializedUnityData = JsonUtility.FromJson<DeformObjectJson>(deformObjectJson[object_id]);

			DeformDynamics.DeformPlugin.Serialization.RetrieveSerializedObjectInfo(object_id, out num_vertices, out num_indices, out num_rv_distance, out num_rv_bending);

			// Unity specific data
			if (object_id != serializedUnityData.id) { continue; }

			Vector3 position = serializedUnityData.position;
			Quaternion rotation = serializedUnityData.rotation;
			Vector3 scale = serializedUnityData.scale;
			//bool disableRendering = serializedUnityData.disableRendering;			

			GameObject go = new GameObject(serializedUnityData.name + " [Serialized]");

			//Add Components
			go.AddComponent<MeshFilter>();
			go.AddComponent<MeshRenderer>();
			go.AddComponent<DeformBody>();

			go.transform.position = position;
			go.transform.rotation = rotation;
			go.transform.localScale = scale;

			Vector3[] vertices = new Vector3[num_vertices / 3];
			int[] indices = new int[num_indices];
			float[] rv_distance = new float[num_rv_distance];
			float[] rv_bending = new float[num_rv_bending];

			float[] kfriction = new float[num_vertices / 3];
			float[] sfriction = new float[num_vertices / 3];
			float[] invmass = new float[num_vertices / 3];

			var verticesHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
			var verticesPtr = verticesHandle.AddrOfPinnedObject();

			var indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
			var indicesPtr = indicesHandle.AddrOfPinnedObject();

			var rvDistanceHandle = GCHandle.Alloc(rv_distance, GCHandleType.Pinned);
			var rvDistancePtr = rvDistanceHandle.AddrOfPinnedObject();

			var rvBendingHandle = GCHandle.Alloc(rv_bending, GCHandleType.Pinned);
			var rvBendingPtr = rvBendingHandle.AddrOfPinnedObject();

			var kfrictionHandle = GCHandle.Alloc(kfriction, GCHandleType.Pinned);
			var kfrictionPtr = kfrictionHandle.AddrOfPinnedObject();

			var sfrictionHandle = GCHandle.Alloc(sfriction, GCHandleType.Pinned);
			var sfrictionPtr = sfrictionHandle.AddrOfPinnedObject();

			var invmassHandle = GCHandle.Alloc(invmass, GCHandleType.Pinned);
			var invmassPtr = invmassHandle.AddrOfPinnedObject();

			float distance_stiffness;
			float bending_stiffness;

			DeformDynamics.DeformPlugin.Serialization.RetrieveSerializedObjectData(object_id,
																	verticesPtr,
																	indicesPtr,
																	rvDistancePtr,
																	rvBendingPtr,
																	kfrictionPtr,
																	sfrictionPtr,
																	invmassPtr,
																	out distance_stiffness,
																	out bending_stiffness);
			
			verticesHandle.Free();
			indicesHandle.Free();
			rvDistanceHandle.Free();
			rvBendingHandle.Free();

			kfrictionHandle.Free();
			sfrictionHandle.Free();
			invmassHandle.Free();
			

			// Deserialize Unity specific data (transform, start/end points of seams, materials?, stiffnesses)
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = go.transform.InverseTransformPoint(vertices[i]);
			}

			Mesh newMesh = new Mesh
			{
				vertices = vertices,
				triangles = indices
			};

			newMesh.RecalculateNormals();
			newMesh.RecalculateBounds();

			go.GetComponent<MeshFilter>().sharedMesh = newMesh;

			go.GetComponent<MeshRenderer>().material = serializedUnityData.material;//Resources.Load<Material>("Materials/DeformNoMaterial");


			DeformBody db = go.GetComponent<DeformBody>();

			db.oldSimulationMesh = newMesh;

			db.id = object_id;

			db.rvDistance = rv_distance.ToList();
			db.rvBending = rv_bending.ToList();

			bool[] fixedVertices = new bool[num_vertices / 3];

			for (int i = 0; i < num_vertices / 3; i++)
			{
				fixedVertices[i] = (invmass[i] == 0.0f);
			}

			db.kfriction = kfriction;
			db.sfriction = sfriction;

			db.fixedVertices = fixedVertices;
			//db.disableRendering = disableRendering;

			loadedDeformBodies.Add(db);

			if (serializedUnityData.wireframe)
			{
				go.AddComponent<Wireframe>();
			}
		}

		for (int seam_id = 0; seam_id < num_seams; seam_id++)
		{
			int num_indices;
			DeformSeamJson serializedUnityData = JsonUtility.FromJson<DeformSeamJson>(deformSeamJson[seam_id]);
			
			DeformDynamics.DeformPlugin.Serialization.RetrieveSerializedSeamInfo(seam_id, out num_indices);
			
			if (seam_id != serializedUnityData.id) {
				continue;
			}

			GameObject go = new GameObject(serializedUnityData.name + " [Serialized]");
			go.AddComponent<DeformSeam>();

			int object_id_a;
			int object_id_b;
			
			int[] indices = new int[num_indices];
			var indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
			var indicesPtr = indicesHandle.AddrOfPinnedObject();

			float distance_stiffness;
			float bending_stiffness;

			int priority;
			DeformDynamics.DeformPlugin.Serialization.RetrieveSerializedSeamData(seam_id,
																  out object_id_a,
																  out object_id_b,
																  indicesPtr,
																  out distance_stiffness,
																  out bending_stiffness,
																  out priority);
			
			indicesHandle.Free();
			
			DeformSeam ds = go.GetComponent<DeformSeam>();
			ds.seamIndices = indices.ToList();
			

			for (int i = 0; i < loadedDeformBodies.Count; i++)
			{
				if (object_id_a == loadedDeformBodies[i].GetId())
				{
					ds.bodyA = loadedDeformBodies[i];
				}

				if (object_id_b == loadedDeformBodies[i].GetId())
				{
					ds.bodyB = loadedDeformBodies[i];
				}
			}

			ds.distanceStiffness = distance_stiffness;
			ds.bendingStiffness = bending_stiffness;

			ds.bodyASeamBeginPointOld	= serializedUnityData.bodyASeamBeginPointOld;
			ds.bodyASeamEndPointOld		= serializedUnityData.bodyASeamEndPointOld;
			ds.bodyBSeamBeginPointOld	= serializedUnityData.bodyBSeamBeginPointOld;
			ds.bodyBSeamEndPointOld		= serializedUnityData.bodyBSeamEndPointOld;

			// Disable priority sewing and damping by default when using the serialized seams
			// The cloths are already "sewed"
			ds.priority = 0;
			ds.damping = false;
			ds.fromSerialization = true;

		}

		DeformDynamics.DeformPlugin.ShutdownPlugin();
	}

	private class DeformObjectJson
	{
		public int id;
		public string name;

		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;

		//public bool disableRendering;

		public Material material;
		public bool wireframe;
	}

	private class DeformSeamJson
	{
		public int id;
		public string name;

		public Vector3 bodyASeamBeginPointOld;
		public Vector3 bodyBSeamBeginPointOld;

		public Vector3 bodyASeamEndPointOld;
		public Vector3 bodyBSeamEndPointOld;
	}
}


