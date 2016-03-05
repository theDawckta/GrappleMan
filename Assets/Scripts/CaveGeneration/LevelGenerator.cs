using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelGenerator : MonoBehaviour {

    public Material MeshMaterial;
    public string Seed;
    public int SeedLength = 8;
    public int LengthMin = 1;
    public int LengthMax = 20;
    public int WidthMin = 1;
    public int WidthMax = 20;
    public int HeightMin = 1;
    public int HeightMax = 5;
    public float TotalLength = 280;
    public float AvailableHeight;

    private string seed;
    private MeshFilter filter;
    private MeshRenderer renderer;
    private Mesh mesh;
    private int VertexCountIndex = 24;
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> triangles = new List<int>();

	void Start () {

        float length = 0;
        float width = 0;
        float height = 0;
        float totalLength = 0;
        float previousLength = 0;
        int meshCount = 0;
        Vector3 location = new Vector3(-TotalLength / 2, 0.0f, 0.0f);

        if (String.IsNullOrEmpty(Seed))
        {
            seed = RandomString(SeedLength);
            Seed = seed;
        }
        else
            seed = Seed;

        System.Random pseudoRandom = new System.Random(Seed.GetHashCode());

        filter = gameObject.AddComponent<MeshFilter>();
        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = MeshMaterial;
        mesh = filter.mesh;
        mesh.Clear();

        while(totalLength < TotalLength)
        {
            
            length = pseudoRandom.Next(LengthMin, LengthMax);
            width = pseudoRandom.Next(WidthMin, WidthMax);
            height = pseudoRandom.Next(HeightMin, HeightMax);
            if (TotalLength < totalLength + length)
            {
                length = TotalLength - totalLength;
                totalLength = totalLength + length;
            }
            else
                totalLength = totalLength + length;

            location = new Vector3(location.x + (length / 2) + (previousLength / 2), (AvailableHeight - width) / 2, 0.0f);
            previousLength = length;

            GenerateMesh(length, width, height, meshCount, location);
            meshCount = meshCount + 1;
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        //mesh.RecalculateBounds();
        //mesh.Optimize();

        transform.position = new Vector3(0.0f, 0.0f , 0.0f);
	}
	
	// This is for runtime generation excitement, HOLLAH!
	void Update () {
	
	}

    void GenerateMesh(float length, float width, float height, int meshCount, Vector3 location)
    {

        Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f) + location;
        Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f) + location;
        Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f) + location;
        Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f) + location;

        Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f) + location;
        Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f) + location;
        Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f) + location;
        Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f) + location;

        vertices.AddRange(CreateCubeVertices(p0, p1, p2, p3, p4, p5 ,p6 ,p7));

        // make triangle
        triangles.AddRange(CreateCubeTriangles(VertexCountIndex * meshCount));

        // make normals
        normals.AddRange(CreateCubeNormals());

        // make UVs
        Vector2 _00 = new Vector2(0f, 0f);
        Vector2 _10 = new Vector2(1f, 0f);
        Vector2 _01 = new Vector2(0f, 1f);
        Vector2 _11 = new Vector2(1f, 1f);

        uvs.AddRange(CreateCubeUvs(_00, _10, _01, _11));
    }

    List<Vector3> CreateCubeVertices(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p5, Vector3 p6, Vector3 p7)
    {
        List<Vector3> verticeCollection = new List<Vector3>();
        // Bottom
        verticeCollection.Add(p0);
        verticeCollection.Add(p1);
        verticeCollection.Add(p2);
        verticeCollection.Add(p3);
 
        // Left
        verticeCollection.Add(p7);
        verticeCollection.Add(p4);
        verticeCollection.Add(p0);
        verticeCollection.Add(p3);
 
        // Front
        verticeCollection.Add(p4);
        verticeCollection.Add(p5);
        verticeCollection.Add(p1);
        verticeCollection.Add(p0);
 
        // Back
        verticeCollection.Add(p6);
        verticeCollection.Add(p7);
        verticeCollection.Add(p3);
        verticeCollection.Add(p2);
 
        // Right
        verticeCollection.Add(p5);
        verticeCollection.Add(p6);
        verticeCollection.Add(p2);
        verticeCollection.Add(p1);
 
        // Top
        verticeCollection.Add(p7);
        verticeCollection.Add(p6);
        verticeCollection.Add(p5);
        verticeCollection.Add(p4);

        return verticeCollection;
    }

    List<int> CreateCubeTriangles(int previousVertCount)
    {
        List<int> trianglesCollection = new List<int>();

        // Bottom
        trianglesCollection.Add(3 + previousVertCount);
        trianglesCollection.Add(1 + previousVertCount);
        trianglesCollection.Add(0 + previousVertCount);
        trianglesCollection.Add(3 + previousVertCount);
        trianglesCollection.Add(2 + previousVertCount);
        trianglesCollection.Add(1 + previousVertCount);

        // Left
        trianglesCollection.Add((3 + 4 * 1) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 1) + previousVertCount);
        trianglesCollection.Add((0 + 4 * 1) + previousVertCount);
        trianglesCollection.Add((3 + 4 * 1) + previousVertCount);
        trianglesCollection.Add((2 + 4 * 1) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 1) + previousVertCount);

        // Front
        trianglesCollection.Add((3 + 4 * 2) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 2) + previousVertCount);
        trianglesCollection.Add((0 + 4 * 2) + previousVertCount);
        trianglesCollection.Add((3 + 4 * 2) + previousVertCount);
        trianglesCollection.Add((2 + 4 * 2) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 2) + previousVertCount);

        // Back
        trianglesCollection.Add((3 + 4 * 3) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 3) + previousVertCount);
        trianglesCollection.Add((0 + 4 * 3) + previousVertCount);
        trianglesCollection.Add((3 + 4 * 3) + previousVertCount);
        trianglesCollection.Add((2 + 4 * 3) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 3) + previousVertCount);

        // Right
        trianglesCollection.Add((3 + 4 * 4) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 4) + previousVertCount);
        trianglesCollection.Add((0 + 4 * 4) + previousVertCount);
        trianglesCollection.Add((3 + 4 * 4) + previousVertCount);
        trianglesCollection.Add((2 + 4 * 4) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 4) + previousVertCount);

        // Top
        trianglesCollection.Add((3 + 4 * 5) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 5) + previousVertCount);
        trianglesCollection.Add((0 + 4 * 5) + previousVertCount);
        trianglesCollection.Add((3 + 4 * 5) + previousVertCount);
        trianglesCollection.Add((2 + 4 * 5) + previousVertCount);
        trianglesCollection.Add((1 + 4 * 5) + previousVertCount);

        return trianglesCollection;
    }

    List<Vector3> CreateCubeNormals()
    {
        List<Vector3> normalsCollection = new List<Vector3>();
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 front = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        // Bottom
        normalsCollection.Add(down);
        normalsCollection.Add(down);
        normalsCollection.Add(down);
        normalsCollection.Add(down);
 
        // Left
        normalsCollection.Add(left);
        normalsCollection.Add(left);
        normalsCollection.Add(left);
        normalsCollection.Add(left);
 
        // Front
        normalsCollection.Add(front);
        normalsCollection.Add(front);
        normalsCollection.Add(front);
        normalsCollection.Add(front);
 
        // Back
        normalsCollection.Add(back);
        normalsCollection.Add(back);
        normalsCollection.Add(back);
        normalsCollection.Add(back);
 
        // Right
        normalsCollection.Add(right);
        normalsCollection.Add(right);
        normalsCollection.Add(right);
        normalsCollection.Add(right);
 
        // Top
        normalsCollection.Add(up);
        normalsCollection.Add(up);
        normalsCollection.Add(up);
        normalsCollection.Add(up);

        return normalsCollection;
    }

    List<Vector2> CreateCubeUvs(Vector2 _00, Vector2 _10, Vector2 _01, Vector2 _11)
    {
        List<Vector2> uvsCollection = new List<Vector2>();
        int sides = 6;

        for(int i = 0; i < sides; i++)
        {
            uvsCollection.Add(_11);
            uvsCollection.Add(_01);
            uvsCollection.Add(_00);
            uvsCollection.Add(_10);
        }

        return uvsCollection;
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
