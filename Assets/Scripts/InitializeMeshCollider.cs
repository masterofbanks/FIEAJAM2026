using UnityEngine;

public class InitializeMeshCollider : MonoBehaviour
{
    private MeshCollider meshCollReference;
    private MeshFilter mF;

    private void Awake()
    {
        meshCollReference = GetComponent<MeshCollider>();
        mF = GetComponentInChildren<MeshFilter>();
    }

    private void Start()
    {
        meshCollReference.sharedMesh = mF.mesh;
    }
}
