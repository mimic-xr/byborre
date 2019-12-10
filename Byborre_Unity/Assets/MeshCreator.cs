using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;
using UnityEditor;

public class MeshCreator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR
    [Button]
    void CreateMeshes()
    {
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer r in renderers)
        {
            Mesh m = r.sharedMesh;

            var savePath = "Assets/M-XR/Models/Garment/DD/New/" + r.gameObject.name + ".asset";

            AssetDatabase.CreateAsset(m, savePath);
        }
    }
#endif
}
