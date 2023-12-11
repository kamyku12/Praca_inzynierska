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
            Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(collider[i].center), size / 2, transform.rotation, LayerMask.NameToLayer("Cars"));
            bool areAllCollidersSensors = true;
            foreach(var collider in colliders)
            {
                if(collider.gameObject.tag != "sensor")
                {
                    areAllCollidersSensors = false;
                }
            }
            if (colliders.Length > 0 && !areAllCollidersSensors)
            {
                transform.GetComponent<MeshRenderer>().enabled = true;
                transform.GetComponent<MeshRenderer>().material = materials[i];
                return;
            }
        }
        transform.GetComponent<MeshRenderer>().enabled = false;
    }
}
