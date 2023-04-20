using UnityEngine;

public class SensorController : MonoBehaviour
{
    [SerializeField]
    public BoxCollider[] collider;

    [SerializeField]
    public Material[] materials;

    private void Update()
    {
        for (int i = 0; i < collider.Length; i++)
        {
            Vector3 size = Vector3.Scale(collider[i].size, transform.localScale);
            if (Physics.CheckBox(transform.TransformPoint(collider[i].center), size / 2, transform.rotation, LayerMask.NameToLayer("Cars")))
            {
                transform.GetComponent<MeshRenderer>().enabled = true;
                transform.GetComponent<MeshRenderer>().material = materials[i];
                return;
            }
        }
        transform.GetComponent<MeshRenderer>().enabled = false;
    }

    //private void OnDrawGizmos()
    //{
    //    Vector3 size = Vector3.Scale(collider[0].size, transform.localScale);
    //    Gizmos.DrawCube(transform.TransformPoint(collider[0].center), size);
    //}
}
