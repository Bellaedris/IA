using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum AgentStates
{
    Patrol,
    Chase,
    Search
}

public class FSM : MonoBehaviour
{
    public float viewDistance = 1f;
    public float viewAngle = 90f;

    public GameObject[] patrolPoints;
    
    private PlayerController player; 
    public AgentStates _currentState;

    private Vector3 _lastPlayerPos;
    
    private int _patrolPointIndex;
    private MeshRenderer _renderer;
    
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        _renderer = GetComponent<MeshRenderer>();
        _currentState = AgentStates.Patrol;
        _patrolPointIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentState)
        {
            case AgentStates.Patrol:
                Patrol();
                break;
            case AgentStates.Chase:
                Chase();
                break;
            case AgentStates.Search:
                Search();
                break;
        }
    }

    private void Patrol()
    {
        if (CanSeePlayer())
        {
            _currentState = AgentStates.Chase;
            _renderer.sharedMaterial.color = Color.red;
        }

        var dir = (patrolPoints[_patrolPointIndex].transform.position - transform.position).normalized;
        dir.y = 0;
        transform.position = Vector3.MoveTowards(transform.position, patrolPoints[_patrolPointIndex].transform.position, Time.deltaTime);
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

        if (transform.position == patrolPoints[_patrolPointIndex].transform.position)
            _patrolPointIndex = (_patrolPointIndex + 1) % patrolPoints.Length;
    }

    private void Chase()
    {
        if (!CanSeePlayer())
        {
            _currentState = AgentStates.Search;
            _renderer.sharedMaterial.color = Color.green;
            return;
        }

        _lastPlayerPos = player.transform.position;
        _lastPlayerPos.y = 1;
        var dir = (player.transform.position - transform.position).normalized;
        dir.y = 0;
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime);
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    private void Search()
    {
        if (CanSeePlayer())
        {
            _currentState = AgentStates.Chase;
            _renderer.sharedMaterial.color = Color.red;
            return;
        }

        if (transform.position == _lastPlayerPos)
        {
            _currentState = AgentStates.Patrol;
            _renderer.sharedMaterial.color = Color.white;
        }
        else
        {
            var dir = (_lastPlayerPos - transform.position).normalized;
            dir.y = 0;
            transform.position = Vector3.MoveTowards(transform.position, _lastPlayerPos, Time.deltaTime);
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 dir = player.transform.position - transform.position;
        float angle = Vector2.Angle(dir, transform.forward);

        return (angle <= viewAngle && dir.sqrMagnitude < viewDistance * viewDistance);
    }
}
