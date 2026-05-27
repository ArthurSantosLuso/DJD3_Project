using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Xsl;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


// This code need changes... The way it inserts and removes the outline materials are not the way it should be
// Later on, change it to be 2 lists, one with only the original mesh materials and the other one with original materials and outline materials
// and everytime the player is in range of interaction, change it to the list with outline and remove when its not in the range.

[DisallowMultipleComponent]

public class Outline : MonoBehaviour
{
    private static HashSet<Mesh> regisMeshes = new HashSet<Mesh>();

    private bool needUpdate;
    private Renderer[] renderers;
    private Material outlineFillMat;
    private Material outlineMaskMat;


    public float OutlineWidth
    {
        get { return outlineWidth; }

        set
        {
            outlineWidth = value;
            needUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField]
    private Color outlineColor = Color.black;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 2f;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    private void OnDestroy()
    {
        Destroy(outlineMaskMat);
        Destroy(outlineFillMat);
    }

    private void OnValidate()
    {
        needUpdate = true;

        if (bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        if (bakeKeys.Count == 0)
        {
            Bake();
        }
    }


    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        outlineMaskMat = Instantiate(Resources.Load<Material>("Materials/Outline/OutlineMask"));
        outlineFillMat = Instantiate(Resources.Load<Material>("Materials/Outline/OutlineFill"));

        outlineMaskMat.name = "OutlineMask (Instance)";
        outlineFillMat.name = "OutlineFill (Instance)";

        LoadSmoothNormals();

        needUpdate = true;
    }

    private void Update()
    {
        if (needUpdate)
        {
            needUpdate = false;

            UpdateMaterialProperties();
        }
    }

    public void ToggleOutline(bool activateOutline)
    {
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials.ToList();

            switch (activateOutline)
            {
                case true:
                    materials.Add(outlineMaskMat);
                    materials.Add(outlineFillMat);
                    break;
                case false:
                    materials.Remove(outlineMaskMat);
                    materials.Remove(outlineFillMat);
                    break;
            }
            renderer.materials = materials.ToArray();
        }
    }

    private void Bake()
    {
        HashSet<Mesh> bakedMeshes = new HashSet<Mesh>();

        foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
                continue;

            List<Vector3> smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }

    private List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups =
            mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

        List<Vector3> smoothNormals = new List<Vector3>(mesh.normals);

        foreach (var group in groups)
        {
            if (group.Count() == 1) continue;

            Vector3 smoothNormal = Vector3.zero;

            foreach (var pair in group) smoothNormal += smoothNormals[pair.Value];

            smoothNormal.Normalize();

            foreach (var pair in group) smoothNormals[pair.Value] = smoothNormal;
        }

        return smoothNormals;
    }

    private void LoadSmoothNormals()
    {
        foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!regisMeshes.Add(meshFilter.sharedMesh)) continue;

            int index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            List<Vector3> smoothNormals = (index >= 0) ?
                bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

            meshFilter.sharedMesh.SetUVs(3, smoothNormals);

            Renderer renderer = meshFilter.GetComponent<Renderer>();

            if (renderer != null) CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!regisMeshes.Add(skinnedMeshRenderer.sharedMesh)) continue;


                skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

                CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
            }
        }
    }

    private void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        if (mesh.subMeshCount == 1) return;

        if (mesh.subMeshCount > materials.Length) return;

        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    private void UpdateMaterialProperties()
    {
        outlineFillMat.SetColor("_Color", outlineColor);

        outlineMaskMat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);

        outlineFillMat.SetFloat("_Width", outlineWidth);
    }
}