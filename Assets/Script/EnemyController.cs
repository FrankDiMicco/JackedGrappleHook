using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
[SerializeField] float moveSpeed = 1f; //How fast enemy walks
//[SerializeField] float detectSpeed = 2f; //How fast enemy moves when player is at detectable distance
[SerializeField] float attackDistance = 4f; //Distance at which enemy goes to attack animation/behavior
[SerializeField] float detectdistance = 6f; //Distance at which enemy detects you and will change directions
string facingDirection; //Will be "Left" or "Right"
Rigidbody2D myRigidbody;
CapsuleCollider2D myCapsuleCollider;
CircleCollider2D myCircleCollider;
BoxCollider2D myBoxCollider;
GameObject player; //To interact with player



public Vector2 xHitForce = new Vector2(-150,0);
public Vector2 yHitForce = new Vector2(0, 150);
bool grappleAttacked = false;
bool grappleAttacked2 = false;


//Enemy animiation states
Animator _animator;
const string ATTACK = "SkeletonAttack";
const string WALK = "SkeletonWalk";
const string HIT = "SkeletonHit";
const string DIE = "SkeletonDie";

//To see if enemy is touching ground (For Grounding and Flipping Sprite when edge reached)
public bool grounded = false;
LayerMask groundLayer;
public Vector2 groundOffset;  //To move center of Gizmo circle toward bottom
public float radius; // To adjust size of circle



    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myCapsuleCollider = GetComponent<CapsuleCollider2D>();
        myCircleCollider = GetComponent<CircleCollider2D>();
        myBoxCollider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        player = GameObject.Find("Player");
        groundLayer = LayerMask.GetMask("Ground"); 
    }

    private void OnDrawGizmos() {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere((Vector2)transform.position + groundOffset, radius);
   
}

    void Update() {
    
}


    void FixedUpdate()
    {
    grounded = Physics2D.OverlapCircle((Vector2)transform.position + groundOffset, radius, groundLayer);
                //Collider2D[] hitEnemies = Physics2D.OverlapCircleAll((Vector2)transform.position + groundOffset, radius, groundLayer);
    

        //If enemy is bumped while grappling
        
        if(grappleAttacked){
            float playerXVelocity = player.GetComponent<Rigidbody2D>().velocity.x;
            int directionFactor;
            if(playerXVelocity<0){
                directionFactor = -1;
            } else {
                directionFactor = 1;
            }
            xHitForce = new Vector2(playerXVelocity * playerXVelocity *directionFactor* 10,0f);
            //yHitForce = new Vector2(0f,150f);
            
            
            Vector2 grappleForceApplied = xHitForce + yHitForce;
            
            //assures a one time force is applied to enemy
            if(grappleAttacked2){
                //myRigidbody.AddForce(grappleForceApplied);
                //Debug.Log(myRigidbody.velocity);
                grappleAttacked2 = false;
            }

            

            //Make sure enemy has recovered from being hit before returning to normal velocity
            if(!grappleAttacked2){
                if(Mathf.Abs(myRigidbody.velocity.x) < .1f && Mathf.Abs(myRigidbody.velocity.y) < .1f){
                    grappleAttacked = false;
                }
            }
        }


        //Respawn enemy (testing purposes)
        if(transform.position.y < -100){
            myRigidbody.velocity = new Vector3(0,0,0);
            transform.position = new Vector2(0,0);
        }

        //Normal walk speed
        if(grounded && !grappleAttacked){
            myRigidbody.velocity = new Vector2(moveSpeed,0f); // moves enemy 
        }


        facingDirection = CalculateDirection(moveSpeed); //returns "Right" or "Left" for direction
        float distance = Vector3.Distance (transform.position, player.transform.position); // gets distance between enemy and player

        
        //Go to attack animation/behavior if within range
        if(distance<attackDistance && !grappleAttacked){
            _animator.Play(ATTACK);  //attack if within range
            //myRigidbody.velocity = new Vector2(0f,0f); //stop moving in a direction
            if(transform.position.x > player.transform.position.x && facingDirection == "Right"){
                FlipSprite();
            }
            if(transform.position.x < player.transform.position.x && facingDirection == "Left"){
                FlipSprite();
            }
        }


        //Go to walk animation/behavior if out of range
        if(distance>=attackDistance){
            _animator.Play(WALK);
            if(distance < detectdistance){
                if(transform.position.x > player.transform.position.x && facingDirection == "Right"){
                    FlipSprite();
                }
                if(transform.position.x < player.transform.position.x && facingDirection == "Left"){
                    FlipSprite();
                }
            }
        }

        //Increase fall speed
        if(!grounded){
            myRigidbody.gravityScale =2;
        }
        if(grounded){
            myRigidbody.gravityScale = 1;
        }






    }

    void OnTriggerExit2D(Collider2D other)     
    {
        if(_animator.GetCurrentAnimatorStateInfo(0).IsName(WALK) && other.gameObject.tag == "Ground"){

            FlipSprite();
        }

    
    }

    void FlipSprite(){

        if(facingDirection=="Right"){
            transform.localScale = new Vector2 (-1, 1);
        }
        if(facingDirection=="Left"){
            transform.localScale = new Vector2 (1, 1);
        }
        moveSpeed *= -1;

    }

    string CalculateDirection(float moveSpeed){
        if(moveSpeed >= 0){
            return "Right";
        } else {
            return "Left";
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag == "Player"){
            grappleAttacked = true;
            grappleAttacked2 = true;
 
        }
    }

}
