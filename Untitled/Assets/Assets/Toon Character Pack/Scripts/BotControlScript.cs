using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Require these components when using this script
[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]

public class BotControlScript : MonoBehaviour
{
	public float h;
	public float v;


	public float animSpeed = 1.5f;            // a public setting for overall animator animation speed
	public float lookSmoother = 3f;            // a smoothing setting for camera motion

	public VirtualJoystick joystick;

	private Animator anim;                     // a reference to the animator on the character
	private AnimatorStateInfo currentBaseState;         // a reference to the current state of the animator, used for base layer
	private AnimatorStateInfo layer2CurrentState;   // a reference to the current state of the animator, used for layer 2
	private CapsuleCollider col;               // a reference to the capsule collider of the character


	static int idleState = Animator.StringToHash("Base Layer.Idle");   
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");         // these integers are references to our animator's states
	static int jumpState = Animator.StringToHash("Base Layer.Jump");            // and are used to check state for various actions to occur
	static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");      // within our FixedUpdate() function below
	static int fallState = Animator.StringToHash("Base Layer.Fall");
	static int rollState = Animator.StringToHash("Base Layer.Roll");
	static int waveState = Animator.StringToHash("Layer2.Wave");


	public float accel;
	public float accelVal;
	public float maxAccel=300f;

	public int jumpTime;
	public int jumpTimeVal;

	public Image leftHeart;
	public Image middleHeart;
	public Image rightHeart;

	public Image BonusHeart;

	public int MujeokTime;
	int MujeokTimeVal;

	public Transform cam;
	Vector3 offset;

	public float flashSpeed=5f;
	public Color flashColour = new Color (1f, 0f, 0f, 0.1f);
	public Image damageImg;
	bool damaged = false;

	public bool isMoving=false;
	public bool addOnce1=false;
	public bool addOnce2=false;
	public bool addOnce3=false;
	public GameObject EnemyMan;
	void Start ()
	{
		// initialising reference variables
		BonusHeart.enabled=false;
		anim = gameObject.GetComponent<Animator>();                 
		col = gameObject.GetComponent<CapsuleCollider>();            
		if(anim.layerCount ==2)
			anim.SetLayerWeight(1, 1);
		MujeokTimeVal = 0;

		maxAccel = 300f;
	}

	public void attack(){
		if (MujeokTimeVal == 0)
		{
			Heart();
			MujeokTimeVal=MujeokTime;
		}

	}

	public static bool death;

	public bool isdead()
	{
		return death;
	}

	public void die()
	{
		if (death == false) {
			anim.SetTrigger ("Death");
			damaged = true;
			GetComponents<AudioSource> ()[0].Play ();
			death = true;
		}
	}
	public void enableBonusHeart()
	{
		BonusHeart.enabled = true;
	}

	public void Heart(){
		if(death==false){

			damaged = true;
			GetComponents<AudioSource> ()[0].Play ();
			if (BonusHeart.enabled == false) {
				if (leftHeart)
					Destroy (leftHeart);
				else if (middleHeart)
					Destroy (middleHeart);
				else if (!death) {
					Destroy (rightHeart);
					anim.SetTrigger ("Death");
					death = true;
				}
			} else if(BonusHeart.enabled==true) {
				if (leftHeart)
					Destroy (leftHeart);
				else if (middleHeart)
					Destroy (middleHeart);
				else if (rightHeart)
					Destroy (rightHeart);
				else if (!death) {
					Destroy (BonusHeart);
					anim.SetTrigger ("Death");
					death = true;

				}
			}
		}
	}


	public int QuestNum;

	float SetAbsoluteValue(float a)
	{
		if (a > 0)
			return a;
		else
			return -a;

	}
	//int index=0;
	float DirectionPoint=0f;


	void FixedUpdate ()
	{
		/*----------------------------give buff after time---------------------------------------------------------*/
		if (EnemyMan.GetComponent<TimeManager> ().playtime > 60&&addOnce1==false) {
			maxAccel+=40f;
			addOnce1 = true;
		}

		if (EnemyMan.GetComponent<TimeManager> ().playtime > 120&&addOnce2==false) {
			maxAccel+=40f;
			addOnce2 = true;
		}

		if (EnemyMan.GetComponent<TimeManager> ().playtime > 180&&addOnce3==false) {
			maxAccel+=40f;
			addOnce3 = true;
		}
		/*------------------------------------------------------------------------------------------------------------*/
		if (damaged) {

			damageImg.color = flashColour;
		} else {
			damageImg.color = Color.Lerp (damageImg.color, Color.clear, flashSpeed * Time.deltaTime);
		}
		damaged = false;
		if(MujeokTimeVal>0)
			MujeokTimeVal--;

		if (jumpTimeVal > 0)
			jumpTimeVal--;


		//h = Input.GetAxis("Horizontal");            // setup h variable as our horizontal input axis
		//v = Input.GetAxis("Vertical");            // setup v variables as our vertical input axis
		h = joystick.Horizontal();
		v = joystick.Vertical();

		print(" "+ h+" "+v+"\n");

		anim.speed = animSpeed;                        // set the speed of our animator to the public variable 'animSpeed'
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);   // set our currentState variable to the current state of the Base Layer (0) of animation

		/*-------------------check if moving---------------------*/
		if (v==0 && h==0) {
			isMoving = false;
		} else {
			isMoving = true;
		}
		/*---------------------------------------------------------*/
		//Speed 議곗젙

		if (!isMoving)
			anim.SetFloat("Speed", 0);
		else if (SetAbsoluteValue(v) < SetAbsoluteValue(h))
			anim.SetFloat("Speed", SetAbsoluteValue(h));
		else
			anim.SetFloat("Speed", SetAbsoluteValue(v));

		if (h != 0 || v != 0) {
			if (anim.GetFloat ("Speed") != 0 && accelVal < maxAccel) {
				accelVal *= 1.01f;
			} 
			gameObject.GetComponent<Rigidbody> ().AddForce (transform.forward * accelVal);
		}
		else 
			accelVal = accel;



		//Direction 議곗젙
		if (SetAbsoluteValue(h) > 0.001)
			anim.SetFloat("Direction", h);


		if(anim.layerCount ==2)      
			layer2CurrentState = anim.GetCurrentAnimatorStateInfo(1);   // set our layer2CurrentState variable to the current state of the second Layer (1) of animation


		// STANDARD JUMPING

		// if we are currently in a state called Locomotion, then allow Jump input (Space) to set the Jump bool parameter in the Animator to true
		if (currentBaseState.fullPathHash == locoState)
		{
			if(Input.GetButtonDown("Jump")&&jumpTimeVal==0)
			{
				jumpTimeVal=jumpTime;
				anim.SetBool("Jump", true);
				GetComponent<Rigidbody>().AddRelativeForce(Vector3.up*300);
			}
		}

		// if we are in the jumping state... 
		else if(currentBaseState.fullPathHash == jumpState)
		{
			//  ..and not still in transition..
			if(!anim.IsInTransition(0))
			{            
				// reset the Jump bool so we can jump again, and so that the state does not loop 
				anim.SetBool("Jump", false);
			}

			// Raycast down from the center of the character.. 
			Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
			RaycastHit hitInfo = new RaycastHit();

			if (Physics.Raycast(ray, out hitInfo))
			{
				// ..if distance to the ground is more than 1.75, use Match Target
				if (hitInfo.distance > 1.75f)
				{

					// MatchTarget allows us to take over animation and smoothly transition our character towards a location - the hit point from the ray.
					// Here we're telling the Root of the character to only be influenced on the Y axis (MatchTargetWeightMask) and only occur between 0.35 and 0.5
					// of the timeline of our animation clip
					anim.MatchTarget(hitInfo.point, Quaternion.identity, AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(0, 1, 0), 0), 0.35f, 0.5f);
				}
			}
		}


		// JUMP DOWN AND ROLL 

		// if we are jumping down, set our Collider's Y position to the float curve from the animation clip - 
		// this is a slight lowering so that the collider hits the floor as the character extends his legs
		else if (currentBaseState.fullPathHash == jumpDownState)
		{
			col.center = new Vector3(0, anim.GetFloat("ColliderY"), 0);
		}

		// if we are falling, set our Grounded boolean to true when our character's root 
		// position is less that 0.6, this allows us to transition from fall into roll and run
		// we then set the Collider's Height equal to the float curve from the animation clip
		else if (currentBaseState.fullPathHash == fallState)
		{
			col.height = anim.GetFloat("ColliderHeight");
		}

		// if we are in the roll state and not in transition, set Collider Height to the float curve from the animation clip 
		// this ensures we are in a short spherical capsule height during the roll, so we can smash through the lower
		// boxes, and then extends the collider as we come out of the roll
		// we also moderate the Y position of the collider using another of these curves on line 128
		else if (currentBaseState.fullPathHash == rollState)
		{
			if(!anim.IsInTransition(0))
			{
				col.center = new Vector3(0, anim.GetFloat("ColliderY"), 0);

			}
		}
		// IDLE

		// check if we are at idle, if so, let us Wave!
		else if (currentBaseState.fullPathHash == idleState)
		{
			if(Input.GetButtonUp("Jump"))
			{
				anim.SetBool("Wave", true);
			}
		}
		// if we enter the waving state, reset the bool to let us wave again in future
		if(layer2CurrentState.fullPathHash == waveState)
		{
			anim.SetBool("Wave", false);
		}
	}
	/*
   public bool Switcher=true;
   void OnTriggerEnter(Collider other){
      if (other.gameObject.CompareTag ("InDoor")&&Switcher==true) {
         

         Switcher = false;
      }
   }
   void OnTriggerExit(Collider other){
      if (other.gameObject.CompareTag ("InDoor")&&Switcher==false) {
         

         Switcher = true;
      }

   }
*/
}