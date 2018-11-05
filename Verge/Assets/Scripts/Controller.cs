using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State { Grounded, Grappling, Aerial , Disposed};



public class Controller : MonoBehaviour {

	Vector2 Acceleration;

	State state;

	public TextMesh Test;
	public int speed, jHeight;

	Rigidbody rgdbdy;
	Animator anmtr;

	void Start () {



		Acceleration = new Vector2 (0.0f, 0.0f);
		//transform.position = should be set to levels start position


		rgdbdy = GetComponent<Rigidbody> (); 
		anmtr = GetComponentInChildren<Animator> ();

		state = State.Aerial;

	}
	
	                               
	void Update () {


		anmtr.SetFloat("xAcc",Input.GetAxis("Horizontal"));
		anmtr.SetFloat("yAcc", Input.GetAxis("Vertical"));
		anmtr.SetFloat ("xVel", rgdbdy.velocity.x);
		anmtr.SetFloat ("yVel", rgdbdy.velocity.y);
		anmtr.SetInteger ("State", (int)state);

		Acceleration = new Vector2(0.0f,0.0f);

		switch (state) {
		case State.Disposed: // doing somthing else and cannot be controlled
			break;
		case State.Aerial: // free falling
			break;
		case State.Grappling: //on the side of an object
			//Acceleration.x = Input.GetAxis ("Horizontal") * speed - (rgdbdy.velocity.x / 10);
			Acceleration.y = Input.GetAxis ("Vertical") * speed/4 - (rgdbdy.velocity.y / 10);

			break;

		case State.Grounded: // on top of an object
			if (Input.GetAxis ("Horizontal") != 0) 
				anmtr.SetBool ("Face", (Input.GetAxis ("Horizontal") > 0));



			if (anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Land +")) {
				Acceleration.x = (float) Mathf.Sqrt (anmtr.GetFloat ("xCol"));
				anmtr.SetFloat ("xCol", 0);
			} else if (anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Land -")) {
				Acceleration.x = -(float) Mathf.Sqrt (anmtr.GetFloat ("xCol"));
				anmtr.SetFloat ("xCol", 0);
			}


			if (anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Roll +")) {
				Acceleration.x = (float) Mathf.Sqrt (-anmtr.GetFloat ("xCol"));
				anmtr.SetFloat ("xCol", 0);
			} else if (anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Roll -")) {
				Acceleration.x = -(float) Mathf.Sqrt (anmtr.GetFloat ("xCol"));
				anmtr.SetFloat ("xCol", 0);
			}
			else Acceleration.x = Input.GetAxis ("Horizontal") * speed - (rgdbdy.velocity.x / 10);
			break;

		}
			

		rgdbdy.velocity+= new Vector3(Acceleration.x, Acceleration.y, 0.0f);


		Test.text = state.ToString ();
	}

	void OnGUI(){
		Event e = Event.current;
		if (e.isKey)
			switch (e.keyCode) {
			case KeyCode.Space:
				Jump ();
				break;

			
			}
	}



	void OnCollisionEnter(Collision c){
		anmtr.ResetTrigger ("Jump");
		anmtr.SetFloat ("yCol", c.relativeVelocity.y);
		anmtr.SetFloat ("xCol", c.relativeVelocity.x);


		if (c.contacts [0].normal.y > 0.1) {
			state = State.Grounded;
			rgdbdy.useGravity = true;
		}
		else if (c.contacts [0].normal.x > 0.5 || c.contacts [0].normal.x < -0.5){
			state = State.Grappling;
			rgdbdy.useGravity = false;
			rgdbdy.velocity = new Vector2 (0,0);

		}

			//state = State.Disposed;

	}
	void OnCollisionExit(Collision c){
		state = State.Aerial;
		rgdbdy.useGravity = true;

	}
	void OnCollisionStay(Collision c){
		if (c.contacts [0].normal.y > 0.1) {
			state = State.Grounded;
			rgdbdy.useGravity = true;
		}
		else if (c.contacts [0].normal.x > 0.5 || c.contacts [0].normal.x < -0.5) {
			if (anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Land +") ||
			    anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Land -") ||
			    anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Roll +") ||
			    anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Roll -")) {
				state = State.Grounded;
				rgdbdy.useGravity = true;
			} else {

				state = State.Grappling;
				rgdbdy.useGravity = false;
			}
		}

	}
	void Jump(){
		anmtr.SetTrigger ("Jump");
		switch (state) {
		case State.Grappling:
			if (anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Grapple +")) {
				anmtr.SetBool ("Face", false);
				rgdbdy.velocity += new Vector3 (jHeight * Input.GetAxis("Horizontal") - jHeight/2, jHeight* Input.GetAxis ("Vertical"), 0.0f);
			}
			if (anmtr.GetCurrentAnimatorStateInfo (0).IsName ("Grapple -")) {
				anmtr.SetBool ("Face", true);
				rgdbdy.velocity += new Vector3 (jHeight * Input.GetAxis("Horizontal") + jHeight/2,jHeight * Input.GetAxis ("Vertical"), 0.0f);
			}
			break;
		case State.Grounded:
			rgdbdy.velocity+= new Vector3(Input.GetAxis("Horizontal"), jHeight , 0.0f);
			break;
		Default:
			break;
		}
	}	
}
