using System.Collections.Generic;
using UnityEngine;


public class MeshProcessor : MonoBehaviour
{   
    public static Mesh GetMeshObject(string[] strInfos)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        string[] lines = strInfos;
        foreach (string line in lines)
        {
            if (line.StartsWith("v ")) // 정점(Vertex)
            {
                string[] parts = line.Split(' ');
                vertices.Add(new Vector3(
                    -float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                ));
            }
            else if (line.StartsWith("vt ")) // UV 좌표
            {
                string[] parts = line.Split(' ');
                uvs.Add(new Vector2(
                    float.Parse(parts[1]),
                    float.Parse(parts[2])
                ));
            }
            else if (line.StartsWith("vn ")) // 법선(Normal)
            {
                string[] parts = line.Split(' ');
                normals.Add(new Vector3(
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                ));
            }
            else if (line.StartsWith("f ")) // 삼각형 인덱스
            {
                string[] parts = line.Split(' ');
                for (int i = 1; i <= 3; i++)
                {
                    string[] indices = parts[i].Split('/');
                    int vertexIndex = int.Parse(indices[0]) - 1;
                    triangles.Add(vertexIndex);
                }
            }
        }

        for (int i = 0; i < triangles.Count; i += 3)
        {
            (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray(),
            normals = normals.ToArray()
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }


    public static GameObject GetGenerateMapObject(string[] strInfos, Texture2D texture)
    {
        Mesh mesh = GetMeshObject(strInfos);
        if (mesh != null)
        {
            GameObject obj = new GameObject("LoadedOBJ");
            obj.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial.mainTexture = texture;
            
            return obj;
        }

        return null;
    }
}
