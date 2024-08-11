using UnityEngine;

public class PlayerTank : MonoBehaviour
{
    public float moveSpeed = 20.0f;  // units per second
    public float rotateSpeed = 3.0f;

    public int health = 100;
    public int maxHealth = 100; // Max health

    private Transform _transform;
    private Rigidbody _rigidbody;

    public GameObject turret;
    public GameObject bullet;
    public GameObject bulletSpawnPoint;

    public float turretRotSpeed = 3.0f;
    public float shootRate = 1.5f;
    protected float elapsedTime;

    void Start()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();

        elapsedTime = shootRate;
        rotateSpeed = rotateSpeed * 180 / Mathf.PI;
    }

    void Update()
    {
        float rot = _transform.localEulerAngles.y + rotateSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        Vector3 fwd = _transform.forward * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical");

        _rigidbody.MoveRotation(Quaternion.AngleAxis(rot, Vector3.up));
        _rigidbody.MovePosition(_rigidbody.position + fwd);

        if (turret)
        {
            Plane playerPlane = new Plane(Vector3.up, transform.position);
            Ray rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
            float hitDist = 0;

            if (playerPlane.Raycast(rayCast, out hitDist))
            {
                Vector3 rayHitPoint = rayCast.GetPoint(hitDist);
                Quaternion targetRotation = Quaternion.LookRotation(rayHitPoint - transform.position);
                turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed);
            }
        }

        if (Input.GetButton("Fire1"))
        {
            if (elapsedTime >= shootRate)
            {
                elapsedTime = 0.0f;
                if (bulletSpawnPoint && bullet)
                    Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
            }
        }

        elapsedTime += Time.deltaTime;
    }

    void ApplyDamage(int damage)
    {
        health -= damage;
        Debug.Log("Player Health: " + health);

        if (health <= 0)
        {
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log("Player Exploded!");
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger with: " + other.gameObject.name); // Log the name of the collider

        if (other.CompareTag("RecoveryTile"))
        {
            // Freeze all enemies
            SimpleFSM[] enemies = FindObjectsOfType<SimpleFSM>();
            foreach (SimpleFSM enemy in enemies)
            {
                enemy.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            Debug.Log("Enemies are frozen.");
        }
        else if (other.CompareTag("GoalTile"))
        {
            // Winning condition
            SimpleFSM[] enemies = FindObjectsOfType<SimpleFSM>();
            foreach (SimpleFSM enemy in enemies)
            {
                enemy.Explode(); // Ensure this method is public
            }
            Debug.Log("GoalTile Triggered");
        }
        else if (other.GetComponent<SimpleFSM>())
        {
            // Handle collision with enemy tanks
            ApplyDamage(10); // Adjust damage as needed
            Debug.Log("Player hit by enemy.");
        }
    }
}
