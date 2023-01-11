using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{

    Vector3 mousePosition; //position of mouse
    Vector3 worldPosition; //mousePosition converted to worldPosition format
    public Transform rotationPoint; //rotation point for which gun rotates
    public Transform gunExit; //point on gun where raycast,rope exit
    RaycastHit2D hit;
    Rigidbody2D playerRB;
    Rigidbody2D groundRB;
    SpringJoint2D playerSpringJoint;
    BoxCollider2D playerBC;
    public GameObject player;
    public float grappleForce = 10; //force at which player grapples
    LineRenderer lineRend;
    public float raycastDistance = 100f;
    public float lengthOfRope = 4;
    public float GrappleRetractSpeed = 0.25f;
    public float moveSpeed = 10;
    private bool grounded;
    public float jumpPower=10;

    //to see if player's sides are touching wall
    public bool onWall = false;
    LayerMask groundLayer;
    public Vector2 wallOffset;
    public float radius;

    //to see if players head is touching underside of ground
    public bool headTouchingGround;
    public Vector2 headOffset;
    public float headRadius;

    //to see if player is touching stuff (enemies) below
    public bool onTopOfEnemy;
    public Vector2 feetOffset;
    public float feetRadius;
    LayerMask enemyLayer;

    //point effector
    PointEffector2D playerPE;







    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
        lineRend = GetComponent<LineRenderer>();
        playerSpringJoint = GetComponent<SpringJoint2D>();
        playerBC = GetComponent<BoxCollider2D>();
        //player = FindObjectOfType<GameObject>();
        lineRend.enabled = false;
        playerSpringJoint.enabled = false;

        groundLayer = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("Enemy");
        playerPE = GetComponent<PointEffector2D>();
        playerPE.enabled = false;


    }


private void OnDrawGizmos() {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere((Vector2)transform.position + wallOffset, radius);
    Gizmos.DrawWireSphere((Vector2)transform.position + headOffset, headRadius);
    Gizmos.DrawWireSphere((Vector2)transform.position + feetOffset, feetRadius);

}


    void Update()
    {
        Controller();
        RotateGunToFollowMouse();
        CheckPlayerOutOfBounds();
        lineRend.SetPosition(0, gunExit.position);
        lineRend.sortingLayerName = "player";

        onWall = Physics2D.OverlapCircle((Vector2)transform.position + wallOffset, radius, groundLayer);
        headTouchingGround = Physics2D.OverlapCircle((Vector2)transform.position + headOffset, headRadius, groundLayer);
        onTopOfEnemy = Physics2D.OverlapCircle((Vector2)transform.position + feetOffset, feetRadius, enemyLayer);


    }

    void FixedUpdate() {


        float verMovement = Input.GetAxis("Vertical");//* jumpPower;
        float horMovement = Input.GetAxis("Horizontal");//* moveSpeed;

        if(verMovement > 0){
            if(grounded && !onWall && !headTouchingGround){ //if player is touching ground and pushes UP player will jump if not touching verticle wall
                playerRB.velocity = new Vector3(playerRB.velocity.x,playerRB.velocity.y + jumpPower,0);
            }
            if(!grounded || onWall){
                playerSpringJoint.distance = playerSpringJoint.distance - GrappleRetractSpeed; 
            }
         }
        
        if(grounded) // if on the ground, player can walk left or right
        {
            playerPE.enabled = false; //disable point effector when on the ground

            if(horMovement > 0)
            {
                playerRB.velocity = new Vector3(moveSpeed, playerRB.velocity.y, 0);
            }

            if(horMovement < 0)
            {
                 playerRB.velocity = new Vector3(-moveSpeed, playerRB.velocity.y, 0);
                //transform.Translate(-moveSpeed, 0, 0);
            }
        }


        if(onTopOfEnemy){
            Vector2 topKillForce = new Vector2(playerRB.velocity.x, 300);
            playerRB.AddForce(topKillForce);
        }


    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Ground"){
            grounded = true;
        }    
    }

    private void OnCollisionExit2D(Collision2D other) {
        if(other.gameObject.tag == "Ground"){
            grounded = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "Enemy"){
            Debug.Log(playerRB.velocity.x);
            if(!grounded && Mathf.Abs(playerRB.velocity.x) > 2){
                playerPE.enabled = true;
            }
        }
        
    }

    private void Controller(){
        
        if(Input.GetButtonDown("Fire1")){
            Grapple();
        }

        if(Input.GetButtonUp("Fire1")){  
            //GrappleMovePlayer();  
            ReleaseGrapple(); 
        }







    }

    private void RotateGunToFollowMouse(){
        mousePosition = Input.mousePosition; // gets mouse cursor coordinates
        worldPosition = Camera.main.ScreenToWorldPoint(mousePosition); // converts mouse coordinates to world point coordinates vector3
        Vector3 rotation = worldPosition - transform.position;
        float rotateZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg; // converts from radians to degrees
        rotationPoint.transform.rotation = Quaternion.Euler(0,0,rotateZ); //rotate via transform the degrees 
    }

    private void Grapple(){


        hit = Physics2D.Raycast(gunExit.position, worldPosition - gunExit.position, raycastDistance);

        if(hit && hit.collider.name != "Player" && hit.collider.tag != "Enemy"){
            lineRend.enabled = true; 
            playerSpringJoint.enabled = true;
            //Debug.Log(hit.collider.tag);


            lineRend.SetPosition(0, gunExit.position); //line renderer start point
            lineRend.SetPosition(1, hit.point);       // line renderer second point

            if(hit.distance<lengthOfRope){
                playerSpringJoint.distance = hit.distance;  //if grapple distance is to playform, make rope short
            } else{
                playerSpringJoint.distance = lengthOfRope;  //make distance joint length to full length of rope
            }

            playerSpringJoint.connectedBody = GameObject.Find(hit.collider.name).GetComponent<Rigidbody2D>();
            playerSpringJoint.connectedAnchor = hit.point;
        }
    }

    private void GrappleMovePlayer(){
      Vector3 direction = new Vector3(hit.point.x, hit.point.y, 0f) - transform.position;
      playerRB.velocity = new Vector2(direction.x, direction.y).normalized * grappleForce;
    }

    private void ReleaseGrapple(){
        lineRend.enabled = false;
        playerSpringJoint.enabled = false;
    }
 
    private void CheckPlayerOutOfBounds(){
        if (player.transform.position.y < -20){
            player.transform.position = new Vector3(0,0,0);
        }
    }
 
 
 
 
        //**********************************NOTES************************************************
        //Debug.DrawRay(gunExit.position, worldPosition - gunExit.position , Color.red);
        //laserLineRenderer.SetPosition( 0, targetPosition );
        //laserLineRenderer.SetPosition( 1, endPosition );
        //s2 = GameObject.Find ("cube").GetComponent<script2> ();



}
