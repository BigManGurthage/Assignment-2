using UnityEngine;

public class PatrolPoint : MonoBehaviour {
    public Vector3 zone = new Vector3(15, 2, 15);

    public Vector3 GetRandomPos()
    {
        Vector3 newPos = transform.position + new Vector3(
            Random.Range(-zone.x / 2, zone.x / 2),
            0, // y-coordinate should always be 0
            Random.Range(-zone.z / 2, zone.z / 2)
        );
        return newPos;
    }

    void OnDrawGizmos () {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f);
        Gizmos.DrawWireCube(transform.position, zone);
    }
}
