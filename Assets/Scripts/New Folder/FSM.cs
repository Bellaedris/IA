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
    private AgentStates _currentState;

    private Vector3 _lastPlayerPos;
    
    private int _patrolPointIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
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
        if(CanSeePlayer())
        {
            _currentState = AgentStates.Chase;
            Debug.Log("Chase");
        }
        
        var dir = (patrolPoints[_patrolPointIndex].transform.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, patrolPoints[_patrolPointIndex].transform.position, Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        if (transform.position == patrolPoints[_patrolPointIndex].transform.position)
            _patrolPointIndex = (_patrolPointIndex + 1) % patrolPoints.Length;
    }

    private void Chase()
    {
        if (!CanSeePlayer())
        {
            _currentState = AgentStates.Search;
            Debug.Log("Search");
            return;
        }

        _lastPlayerPos = player.transform.position;
        var dir = ( - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    private void Search()
    {
        if (transform.position == _lastPlayerPos)
        {
            _currentState = AgentStates.Patrol;
            Debug.Log("Patrol");
        }
        else
        {
            var dir = (_lastPlayerPos - transform.position).normalized;
            transform.Translate(Time.deltaTime * dir, Space.World);
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 dir = player.transform.position - transform.position;
        float angle = Vector3.Angle(dir, transform.forward);

        if (angle < viewAngle)
            if (dir.sqrMagnitude < viewDistance * viewDistance)
                return true;

        return false;
    }
}
