using UnityEngine;

public class SimpleFSM : MonoBehaviour
{
    public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
        Recover,
        Dead,
    }

    public FSMState curState;

    public float moveSpeed = 12.0f; // Speed of the tank
    public float rotSpeed = 2.0f; // Tank Rotation Speed
    public float chaseDistance = 25.0f; // Distance to start chasing the player
    public float attackDistance = 20.0f; // Distance to start attacking the player
    public float stopDistance = 10.0f; // Distance to stop moving but continue shooting

    protected Transform playerTransform; // Player Transform
    protected Vector3 destPos; // Next destination position of the NPC Tank
    protected GameObject[] pointList; // List of points for patrolling

    protected bool bDead;
    public int health = 100;
    public int maxHealth = 100; // Max health for recovery
    public GameObject recoveryZone; // Recovery zone

    public GameObject explosion;
    public GameObject smokeTrail;

    public GameObject bullet; // Bullet prefab
    public Transform bulletSpawnPoint; // Bullet spawn point
    public float shootRate = 2.0f; // Shooting rate
    private float elapsedTime;

    void Start()
    {
        curState = FSMState.Patrol;

        bDead = false;

        pointList = GameObject.FindGameObjectsWithTag("PatrolPoint");
        FindNextPoint();  // Set a random destination point first

        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;
        if (!playerTransform)
            Debug.Log("Player doesn't exist.. Please add one with Tag named 'Player'");

        elapsedTime = shootRate; // Initialize elapsed time for shooting
    }

    void Update()
    {
        switch (curState)
        {
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Recover: UpdateRecoverState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
        }

        if (health <= 0)
            curState = FSMState.Dead;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= stopDistance)
        {
            curState = FSMState.Attack;
        }
        else if (distance <= attackDistance)
        {
            curState = FSMState.Chase;
        }
        else if (health <= 0)
        {
            curState = FSMState.Recover;
        }
    }

    protected void UpdatePatrolState()
    {
        if (Vector3.Distance(transform.position, destPos) <= 2.0f)
        {
            FindNextPoint();
        }

        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed));
        GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + transform.forward * Time.deltaTime * moveSpeed);
    }

    protected void UpdateChaseState()
    {
        destPos = playerTransform.position;
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed));
        GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + transform.forward * Time.deltaTime * moveSpeed);
    }

    protected void UpdateAttackState()
    {
        Quaternion targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
        GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed));
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= shootRate)
        {
            elapsedTime = 0.0f;
            Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        }
    }

    protected void UpdateRecoverState()
    {
        if (Vector3.Distance(transform.position, recoveryZone.transform.position) > 1.0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(recoveryZone.transform.position - transform.position);
            GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed));
            GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + transform.forward * Time.deltaTime * moveSpeed);
        }
        else
        {
            health = (int)Mathf.Min(health + (maxHealth / 5) * Time.deltaTime, maxHealth);
            if (health >= maxHealth)
            {
                curState = FSMState.Patrol;
                FindNextPoint();
                Instantiate(gameObject, destPos, Quaternion.identity);
            }
        }
    }

    protected void UpdateDeadState()
    {
        if (!bDead)
        {
            bDead = true;
            Explode();
        }
    }

    public void Explode()
    {
        float rndX = Random.Range(8.0f, 12.0f);
        float rndZ = Random.Range(8.0f, 12.0f);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<Rigidbody>().AddExplosionForce(10.0f, transform.position - new Vector3(rndX, 2.0f, rndZ), 45.0f, 40.0f);
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(rndX, 10.0f, rndZ));
        }

        if (smokeTrail)
        {
            GameObject clone = Instantiate(smokeTrail, transform.position, transform.rotation) as GameObject;
            clone.transform.parent = transform;
        }
        Invoke("CreateFinalExplosionEffect", 1.4f);
        Destroy(gameObject, 1.5f);
    }

    protected void CreateFinalExplosionEffect()
    {
        if (explosion)
            Instantiate(explosion, transform.position, transform.rotation);
    }

    public void ApplyDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("Enemy Health: " + health); // Log the health of the enemy for debugging
    }

    protected void FindNextPoint()
    {
        int rndIndex = Random.Range(0, pointList.Length);
        destPos = pointList[rndIndex].transform.position;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, destPos);
    }
}
