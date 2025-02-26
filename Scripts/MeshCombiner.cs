using Unity.VisualScripting;
using UnityEngine;

public class MeshCombiner : MonoBehaviour{

    //material for the combined mesh
    [SerializeField] private Material combinedMeshMaterial;
    
    //combines meshes from all children of the specified transform
    public void CombineMeshes(Transform root, string meshName){
        //get all mesh filters in children and make the combine instance array
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        //go through all mesh filters and add them to the combine instance array
        for(int i = 0; i < meshFilters.Length; i++){
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            
            //disable the wall after combining into a larger chunk
            meshFilters[i].gameObject.SetActive(false);
        }

        //create the new combined mesh
        Mesh mesh = new Mesh{
            name = meshName
        };
        
        //combine collected meshes into new mesh
        mesh.CombineMeshes(combine);

        //put a mesh filter and a mesh renderer on the root and put the new combined mesh
        MeshFilter filter = root.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer rend = root.AddComponent<MeshRenderer>();
        rend.material = combinedMeshMaterial;

        //also add a collider in case we would like to have physics interaction with the maze
        root.AddComponent<MeshCollider>();
    }
}
