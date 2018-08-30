using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this to invert the normals of an object
/// </summary>
public class FlipNormals : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] normals = mesh.normals;

        for(int i = 0; i < normals.Length; i++) {
            normals[i] = normals[i] * -1; // flip the normals
        }

        mesh.normals = normals; // set the new normals of the mesh

        for(int i = 0; i < mesh.subMeshCount; i++) {
            int[] triangles = mesh.GetTriangles(i);
            for(int j = 0; j < triangles.Length; j += 3) {
                // Swap the order of the triangle vertices
                int temp = triangles[j];
                triangles[j] = triangles[j + 1];
                triangles[j + 1] = temp;
            }
            mesh.SetTriangles(triangles, i);
        }	
	}

}
