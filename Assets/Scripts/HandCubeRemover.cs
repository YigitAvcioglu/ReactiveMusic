using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandCubeRemover : MonoBehaviour
{
    void Update()
    {
        // VR dünyasında "Cube" ismine sahip veya XR Controller tarafından otomatik oluşturulmuş sahte modelleri yakala
        MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>(true);
        foreach (var mesh in meshes)
        {
            if (mesh != null && mesh.sharedMesh != null && mesh.sharedMesh.name == "Cube")
            {
                // Mavi eldiven modeline dokunmadan sadece "Küp" renderını yok et
                MeshRenderer mr = mesh.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.enabled = false;
                }
            }
        }
    }
}
