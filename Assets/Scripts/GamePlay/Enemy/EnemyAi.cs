using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyAi : MonoBehaviour
{
    float turnSpeed = 3f;
    float movementSpeed;
    float setSpeed; 
    //currentSpeed;
    //float changedSpeed;

    public LayerMask whatIsGround, whatIsPlayer, ignoreLayers;
    
    private CharacterStats stats;
    [SerializeField] private NavMeshPath path; 
    private NavMeshAgent agent;
    private CharacterCombat combat; //this characters comat scripts 

    public Transform target;// Targte obje
    Vector3 distanceToTarget, walkPoint, distanceToWalkPoint;
    bool canAttack, canRunAway;
    //bool canMove;
    float timeBetweenAttacks, sightRange;//Get From Enemy Stats!!!!!
    float attackRange;

    public float distanceToLastCorner, currentSpeed;
    public bool walkPointSet;

    enum State{Patrol, Idle, InCombat, Attacking, ResetAttackPosition, SearchingForTarget, Dead}
    [SerializeField] private State CurrentState = State.Idle;

    //animation
    private Animator animator;
    //private float animationSpeed;

    enum TargetRangeState{AttackRange,SightRange,OutOfRange}

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        stats = GetComponent<CharacterStats>();
        animator = GetComponent<Animator>();
    }
   
    private void Start()
    {
        path = new NavMeshPath();
        CurrentState = State.Idle;
        ChangeState(CurrentState);  

        //Stats
        sightRange = stats.sightRange;
        timeBetweenAttacks = stats.attackSpeed;
        attackRange = stats.range;

        canAttack = true;  
        //canMove = true;
        movementSpeed = stats.movementSpeed;
        setSpeed = 0f;

        //animationSpeed = 0f; // Default animation speed        
    }
       
    private void Update()
    {
        UpdateAnimationSpeed();

        if (agent.hasPath)
        {
            path = agent.path;
            
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                if(target)
                    Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
                else
                    Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.green);
            }
        }
     
    }

    public void UpdateEquipment()
    {
        attackRange = stats.range;
    }

//-----States-----///
    void ChangeState(State NewState)
    {
        StopAllCoroutines();
        CurrentState = NewState;

        switch (NewState)
        {
            case State.Idle:
                StartCoroutine(Idle());

                setSpeed = 0f;
                StartCoroutine(UpdateSpeed(setSpeed));
                break;

            case State.Patrol:
                StartCoroutine(Patrol());

                setSpeed = movementSpeed/2f;
                StartCoroutine(UpdateSpeed(setSpeed));
                break;

            case State.InCombat:
                StartCoroutine(InCombat());

                setSpeed = 0f;
                StartCoroutine(UpdateSpeed(setSpeed));
                break;
 
            case State.Attacking:
                StartCoroutine(Attacking());
                break;

            case State.ResetAttackPosition:
                StartCoroutine(ResetAttackPosition());
                
                setSpeed = movementSpeed; 
                StartCoroutine(UpdateSpeed(setSpeed));
                break;

            case State.SearchingForTarget:
                StartCoroutine(SearchingForTarget());
                break;

            case State.Dead:
                StartCoroutine(Dying());
                break;
        }
       
    }

    public IEnumerator Idle()
    {
        float timer = 0; 
        float randomT = Random.Range(2f,11f);

        // While the NPC is in the Idle state
        while(CurrentState == State.Idle)
        {   
            // Increment the timer by the time elapsed since the last frame
            timer += Time.deltaTime;
                     
            // Check if there are any targets nearby
            TargetSearch();
            
            // If the timer has reached the random time generated earlier, transition to the Patrol state
            if(timer >= randomT) 
            {
                //Debug.Log("timesuppp");
                ChangeState(State.Patrol);
                yield break;
                //Debug.Log("Still going opssssssss");
            }
            
            yield return null;
        }
    }

    public IEnumerator Patrol()
    {
        //Path     
        path = null; // initialize path to null
        Vector3 currentPosition = transform.position;

        while (CurrentState == State.Patrol)
        {
            // Check if there is no set walk point, search for one
            if (!walkPointSet)
            {
                //Debug.Log("SearchPoint");

                yield return StartCoroutine(SearchWalkPoint()); // use Coroutine to wait for search to finish
                
                if (!walkPointSet)
                {
                    // Search failed, change state to Idle
                    ChangeState(State.Idle);
                    yield break;
                }
            
                // Search successful, continue with patrol
                Vector3[] pathCorners = path.corners;
            
            
             // Check if the path is blocked by obstacles
                bool pathBlocked = false;
                int lastBlockedCornerIndex = -1;
                for (int i = 1; i < pathCorners.Length; i++) // Check each corner of the path
                {
                    NavMeshHit hit;
                    if (NavMesh.Raycast(pathCorners[i-1], pathCorners[i], out hit, NavMesh.AllAreas))
                    {
                        Debug.Log("Path Is Blocked OKkkk");
                        pathBlocked = true;
                        lastBlockedCornerIndex = i;
                        break;
                    }
                }

                if (!pathBlocked) // If the path is not blocked, set walkPointSet to true
                {
                    walkPointSet = true;
                }
                else // If the path is blocked, find a new path
                {
                    Vector3 targetPosition = pathCorners[pathCorners.Length - 1];
                    NavMeshPath newPath = new NavMeshPath();

                    // Try to find a new path to the target position
                    if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, newPath))
                    {
                        // If a new path is found, update the current path and continue with the patrol
                        path = newPath;
                        walkPointSet = true;
                    }
                    else
                    {
                        // If a new path cannot be found, change state to Idle
                        ChangeState(State.Idle);
                        yield break;
                    }
                }  
            
            
            }

            if (path != null && path.corners.Length > 0)
            {
                Vector3 lastCorner = path.corners[path.corners.Length - 1];
                distanceToLastCorner = Vector3.Distance(transform.position, lastCorner);

                // Debug.Log("Remaining distance on path: " + distanceToLastCorner);

                // Check if the last corner has been reached
                if (distanceToLastCorner <= 0.2f && walkPointSet)
                {
                    //Debug.Log("At Walk Point");
                    walkPointSet = false;
                    path = null; // reset path to null
                    ChangeState(State.Idle);
                    yield break;
                }
            }

            // Look for a target every 2 seconds
            if (Time.frameCount % 120 == 0)
            {
                TargetSearch();
            }

            yield return null;
        }
    }

    public IEnumerator InCombat()// In Combat Decison Tree
    {
        // Loop while the state is InCombat
        while (CurrentState == State.InCombat)
        {
            
            // If target does not exist, switch to SearchingForTarget state and exit coroutine
            if (target == null)
            {
                ChangeState(State.SearchingForTarget);
                yield break;
            }

            // Look towards the target's position
            LookAtTarget();

            // Calculate the distance to the target
            distanceToTarget = transform.position - target.position;
            RaycastHit hitData;

            // Add a pause before making a decision
            //Change the wait scale by the enemy speed
            yield return new WaitForSeconds(1.0f); // wait for 1 second

            if (IsTargetInSight(out hitData))//Fix
            {   
                if (!IsTargetBlocked(hitData))
                {                    
                    // Check the distance range of the target
                    switch (GetTargetRangeState())
                    {
                        // If target is within attack range and attack is not on cooldown, switch to Attacking
                        case TargetRangeState.AttackRange:
                            // Decison State(attack, move, block)
                            if (canAttack) //Decision 1 ( if canattack then attack)
                            {
                               
                                ChangeState(State.Attacking);
                                yield break;
                            }
                            else//DEcsion avalibel (Block, Move, DoNothing)
                            {
                                // Check if in a good attacking postion(not in fornt of the player) if in front of player ( block or find new pos)
                           
                                //Decison 2 ( respet pos)
                                ChangeState(State.ResetAttackPosition);

                                //if not infront of player then (move, do nothing)
                                //
                                
                                yield break;
                            }

                            //break;

                        // If target is not within attack range but still in sight range, switch to ResetAttackPosition
                        case TargetRangeState.SightRange:
                            //Debug.Log("Test 22222222");
                            ChangeState(State.ResetAttackPosition);
                            break;
                            //break;

                        // If target is out of both attack and sight range, switch to SearchingForTarget state and exit coroutine
                        case TargetRangeState.OutOfRange:
                            Debug.Log("Target Left Sight");
                            ChangeState(State.SearchingForTarget);
                            break;
                            //break;
                    }
                }
                else // If target is blocked by an object, switch to SearchingForTarget state and exit coroutine
                {
                    Debug.Log("Target Blocked");
                    ChangeState(State.SearchingForTarget);
                    yield break;
                }


            }
            else // If target is not in sight, switch to SearchingForTarget state and exit coroutine
            {



                Debug.Log("No Target In Sight");
                ChangeState(State.SearchingForTarget);
                yield break;

            }

            // Use FixedUpdate() for physics calculations
            yield return new WaitForFixedUpdate();
        }
    }

    public IEnumerator Attacking()
    {
        // Cache the transform and stats components to avoid multiple lookups
        Transform targetTransform = target.transform;
        CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

        // Ensure that the target and its stats component are not null
        if (target == null || targetStats == null)
        {
            ChangeState(State.SearchingForTarget);
            yield break;
        }

        // Face the target
        LookAtTarget(); //attack turn speed based on Spd

        // If the enemy hasn't attacked yet, attack the target
        if (canAttack)
        {
            Attack();
            ChangeState(State.InCombat);
            yield break;
        }

        yield return null;
    }

    void Attack()
    {
        //attack function that deals damage to the target
        combat.Attack(false);

        // Set the canAttack flag to true and invoke the ResetAttack method after a delay
        canAttack = false;//attack cooldown

        //Run cooldown functioin to rest canAttack
        Invoke(nameof(ResetAttack), timeBetweenAttacks); //fix this

        // Invoke the BattleConfidence method to increase the enemy's confidence in battle
        //Invoke(nameof(BattleConfidence), timeBetweenAttacks / 2);
    }

    private void ResetAttack()
    {
        canAttack  = true;
    }  

    //Decisonnnn (rest attack postion, defend...)
    public IEnumerator ResetAttackPosition()
    {    
       // Debug.Log("resetting attack position...");

        //Path     
        path = null; // initialize path to null
        Vector3 currentPosition = transform.position;
        walkPoint = new Vector3(0,0,0);
        walkPointSet = false;

        // Calculate the distance from the enemy to the target
        //float distanceFrom = Vector3.Distance(transform.position, target.position);

        // Check if there is a free attack position
        bool posFound = false;

        // Calculate the angle between the enemy's and target's "up" positions
        Vector3 targetUp = target.position + Vector3.up;
        Vector3 enemyUp = transform.position + Vector3.up;
        float angle = Vector3.Angle(targetUp - target.position, enemyUp - transform.position);
        //Vector3 pointToTarget = new Vector3(0,0,0);

        // Try to find a position within attack range to hit the target
        for (int i = 0; i < 10; i++)
        {
            // Find a random angle within the 90-degree range
            float randomAngle = Random.Range(90f, 280f);

            // Calculate the direction vector between the AI and the target
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // Calculate the rotation quaternion to rotate the direction vector within the 90-degree range
            Quaternion randomRotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

            // Rotate the direction vector within the 90-degree range
            Vector3 rotatedDirection = randomRotation * directionToTarget;

            // Calculate the new position for the AI to move to based on the rotated direction vector and the distance to the target
            Vector3 newPos = target.position + rotatedDirection * attackRange;

            // Check if the new position is valid
            if (CheckPosition(transform.position, newPos))
            {
                // Check if the enemy can see the player from the new position
               // Debug.Log("Found attack position!"); 

                // Set the new position as the walk point
                walkPoint = newPos;
                walkPointSet = true;
                agent.SetDestination(walkPoint); // Change this to a turn function

                posFound = true;
                break;  
            }            
        }

        // If a valid position could not be found, go back to searching for the target
        if (!posFound)
        {
            Debug.Log("Could not find attack position."); 
            ChangeState(State.SearchingForTarget); 
        }

        // While the enemy is in the ResetAttackPosition state
        while (CurrentState == State.ResetAttackPosition && posFound)
        {
             // Face the target
            LookAtTarget(); //attack turn speed based on Spd

            // Calculate the distance from the enemy to the walk point
            if (path != null)
            {
                Vector3 lastCorner = path.corners[path.corners.Length - 1];
                distanceToLastCorner = Vector3.Distance(transform.position, lastCorner);
                 float targetDistanceToLastCorner = Vector3.Distance(target.position, lastCorner);
                //Debug.Log("Remaining distance on path: " + distanceToLastCorner);

                // Check if the last corner has been reached
                if (distanceToLastCorner <= 0.05f && walkPointSet)
                {
                    //Debug.Log("Reached attack position!"); 
                    walkPointSet = false;

                    //If at walk Point see if now can attack, if not return back to combat decision
                    if (canAttack)
                    {
                        ChangeState(State.Attacking);
                        yield break;
                    }
                    else//fix add choosing state 
                    {
                        ChangeState(State.InCombat);
                        yield break;
                    }               
                }
            
                // Check if the walk point is within reach attack range of enemy
                if (targetDistanceToLastCorner > attackRange+ 2f)
                {  
                    // Set the walk point as reached and change to the InCombat state
                    //Debug.Log("Change attack position!"); 
                    //walkPointSet = false;
                    ChangeState(State.ResetAttackPosition);
                    yield break;
                }
            }

            yield return null;               
        }

        yield return null;    
    }

    public IEnumerator Dying()
    {  
        //Stop Movement
        //disable Ai, nd Stats , nd Navagent
        
        //agent.Setactive = fale;
        //stats.setactive = false;
        //This.Setactive = false
         yield return null;
    }

    public void Die()
    {
        Debug.Log("And so this is the end");
        ChangeState(State.Dead);
    }

//-----MOVEMENT----//
    private void UpdateAnimationSpeed()
    {
        float currentSpeed = agent.velocity.magnitude / agent.speed;
        currentSpeed = float.IsNaN(currentSpeed) || Mathf.Approximately(currentSpeed, 0f) ? 0f : currentSpeed;
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, 1f) * agent.speed;
        animator.SetFloat("speedPercent", currentSpeed);
    }

    IEnumerator UpdateSpeed(float newSpeed)
    {    
        float elapsedTime = 0f;
        //float currentSpeed = agent.velocity.magnitude / agent.speed;

        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.deltaTime;
            float lerpTime = Time.deltaTime / 0.1f;
            agent.speed = Mathf.Lerp(agent.speed, newSpeed , lerpTime);

            yield return null;
        }

        agent.speed = newSpeed; //Fix? 

    }
    public void EnableMovement()//fix 
    {
        //set ai movemnt to movementSpeed
        agent.speed = setSpeed;

        //set can move to flase
        //canMove = true;
    }

    public void DisableMovement()//fix 
    {
        //set ai movemnt to 0
        agent.speed = 0;

        //set can move to flase
        //canMove = false;
    }

    private IEnumerator TurnTowardsTarget()//Send in Speed and target.
    {
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        float elapsedTime = 0f;

        while (elapsedTime < turnSpeed)
        {
            elapsedTime += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, elapsedTime / turnSpeed);
            yield return null;
        }
    }

    // Function to look at the target's position using the coroutine
    private void LookAtTarget()
    {
        StartCoroutine(TurnTowardsTarget());
    }

    // Function to check if target is in sight
    private bool IsTargetInSight(out RaycastHit hitData)
    {
        Vector3 pos = transform.position + transform.up;
        Vector3 dir = (target.transform.position - transform.position).normalized;
        Ray ray = new Ray(pos, dir * distanceToTarget.magnitude);
        Debug.DrawRay(ray.origin, ray.direction * distanceToTarget.magnitude, Color.green);
        return Physics.Raycast(ray, out hitData, sightRange, ~ignoreLayers);
    }

    private bool IsTargetBlocked(RaycastHit hitData)
    {
        return hitData.transform != target;
    }

    // Function to get the distance range of the target
    private TargetRangeState GetTargetRangeState()
    {
        
        if (distanceToTarget.magnitude <= attackRange && distanceToTarget.magnitude < sightRange)
        {
            return TargetRangeState.AttackRange;
        }
        else if (distanceToTarget.magnitude > attackRange && distanceToTarget.magnitude < sightRange)
        {
            return TargetRangeState.SightRange;
        }
        else
        {
            return TargetRangeState.OutOfRange;
        }

    }
    
    public IEnumerator SearchWalkPoint()
    {
        // Log message to console
        Debug.Log("Searching for place to walk...");

        // Variable to track whether a valid walk point has been found
        //bool walkPointFound;

        // Loop to try to find a valid walk point
        for (int i = 0; i < 10; i++)
        {
            // Calculate a random point within range of the enemy's current position
            float randomZ = Random.Range(-sightRange + i, sightRange - i);
            float randomX = Random.Range(-sightRange + i, sightRange - i);
            Vector3 checkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            // Check if the new walk point is valid
            if (CheckPosition(transform.position, checkPoint))
            {
                // Log message to console
                Debug.Log("Valid walk point found!");

                // Set the new walk point as the target destination
                walkPointSet = true;
                //walkPointFound = true;
                walkPoint = checkPoint;
                agent.SetDestination(walkPoint);

                // Set walkPointFound to true and exit the loop
                break;
            }
        }

        yield return null;
    }

    public IEnumerator SearchingForTarget()
    {  
        if(target == null)
        {
            walkPointSet = false;
            ChangeState(State.Patrol);
            yield return null;
        }  

        Debug.Log("we looking");

        Vector3 wP = target.position + target.up;
        target = null;
        //bool foundTarget = false;
     
        //Search random pos
        //

        if (CheckPosition(transform.position, wP))
        {
            Debug.Log("Go to target");
            walkPoint = wP;
            agent.SetDestination(walkPoint); //Change To Turn Function
        }
        else
        {
            Debug.Log("cant move to player pos ");
            walkPointSet = false;
            ChangeState(State.Patrol);
        }

        while(CurrentState == State.SearchingForTarget)
        {
            distanceToTarget = transform.position - walkPoint;
         
            //Check at Point Before Target Is found
            if(distanceToTarget.magnitude <= 1)
            {  
                Debug.Log("Lost Target");
                walkPointSet = false;
                ChangeState(State.Patrol);
            }
            else
            {
                //Debug.Log("notttt At le point");
                yield return null;
            }
            
            TargetSearch();

            yield return null; 
        }
        
        yield return null;      
    }

    bool CheckPosition(Vector3 firstP, Vector3 targetP)
    {
        NavMeshHit navMeshHit;

        // Sample the target position on the NavMesh
        if (!NavMesh.SamplePosition(targetP, out navMeshHit, 2f, NavMesh.AllAreas))
        {
            // Return false if the target position is not on the NavMesh
            return false;
        }

        // Calculate the path from the first position to the target position
        NavMeshPath navMeshPath = new NavMeshPath();
        NavMesh.CalculatePath(firstP, navMeshHit.position, NavMesh.AllAreas, navMeshPath);

        // Check if the path is complete
        if (navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            // Return true if the target position is valid
            return true;
        }
        else
        {
            // Return false if the target position is invalid
            return false;
        }
    }

    void TargetSearch()
    {
        //Look For Target CheckTargtCollisons()
        Collider[] currentHits = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);

        if (target != null)
            ChangeState(State.InCombat);

        if (currentHits.Length != 0) 
        {
            //Check if can attack palye
            for(int i = 0; i < currentHits.Length; i++)
            {           
                distanceToTarget = transform.position - currentHits[i].gameObject.transform.position;
                Vector3 pos = transform.position + transform.up;
                Vector3 dir = (currentHits[i].gameObject.transform.position - transform.position).normalized;

                Ray ray = new Ray(pos, dir *distanceToTarget.magnitude);
                RaycastHit hitData;

                //Check Pos Function
                if(Physics.Raycast(ray, out hitData,sightRange))
                    if(hitData.transform.gameObject.tag == "Player")
                    {
                        //Check Pos Function
                        if(CheckPosition(transform.position, hitData.transform.position) || distanceToTarget.magnitude < attackRange)
                        {
                            //If found target = true;
                            Debug.Log("Target Found " + currentHits[i].gameObject.name);
                            target = currentHits[i].gameObject.transform;
                            ChangeState(State.InCombat);
                            break;

                        }
                        //else
                        //    yield return null; 
                    }
                    //else
                    //yield return null;  
            }
            //yield return null;
        } 
    }
    
}
