using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State { Grounded, Grappling, Aerial , Crouched };


public class Controller : MonoBehaviour {

    Vector2 Acceleration, cliffEdge;


    State state;

    public TextMesh Test;
    public int speed, jHeight;

    Collider clldr;
    Rigidbody rgdbdy;
    Animator anmtr;

    void Start() {
        Acceleration = new Vector2(0.0f, 0.0f);

        clldr = GetComponent<CapsuleCollider>();
        rgdbdy = GetComponent<Rigidbody>();
        anmtr = GetComponent<Animator>();

        state = State.Aerial;

    }

    void Update() {
        Test.text = cliffEdge.ToString();

        anmtr.SetFloat("xAcc", Input.GetAxis("Horizontal"));
        anmtr.SetFloat("yAcc", Input.GetAxis("Vertical"));
        anmtr.SetFloat("xVel", rgdbdy.velocity.x);
        anmtr.SetFloat("yVel", rgdbdy.velocity.y);
        anmtr.SetInteger("State", (int)state);

        Acceleration = new Vector2(0.0f, 0.0f);

        switch (state) {

            case State.Grappling:

                Acceleration.y = Input.GetAxis("Vertical") * (speed * 0.8f) - (rgdbdy.velocity.y );
                anmtr.SetBool("CornerGrab", true);

                if (rgdbdy.transform.position.y >= (cliffEdge.y - 3.0) && (Input.GetAxis("Vertical") > 0)) 
                    Acceleration.y = -((rgdbdy.velocity.y) / 10);

               else if (rgdbdy.transform.position.y > (cliffEdge.y - 2.5))
                    Acceleration.y = -((rgdbdy.velocity.y + speed) / 10);

                else anmtr.SetBool("CornerGrab", false);


                break;

            case State.Grounded:
                if (Input.GetAxis("Horizontal") != 0)
                    anmtr.SetBool("Face", (Input.GetAxis("Horizontal") > 0));

                if (anmtr.GetCurrentAnimatorStateInfo(0).IsName("Roll +")) {
                    Acceleration.x = (float)Mathf.Sqrt(-anmtr.GetFloat("xCol"));
                    anmtr.SetFloat("xCol", 0);
                } else if (anmtr.GetCurrentAnimatorStateInfo(0).IsName("Roll -")) {
                    Acceleration.x = -(float)Mathf.Sqrt(anmtr.GetFloat("xCol"));
                    anmtr.SetFloat("xCol", 0);
                }

                Acceleration.x = Input.GetAxis("Horizontal") * speed / 10 - (rgdbdy.velocity.x / 20);
                break;

            case State.Crouched:
                Acceleration.x = Input.GetAxis("Horizontal") * speed / 10 - (rgdbdy.velocity.x / 10);
                if (Input.GetAxis("Horizontal") != 0)
                    anmtr.SetBool("Face", (Input.GetAxis("Horizontal") > 0));
                break;
        }
        rgdbdy.velocity += new Vector3(Acceleration.x, Acceleration.y, 0.0f);

    }
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //                                                                          Input & General Event Listeners
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void OnGUI() {
        Event e = Event.current;
        if (e.isKey)
            switch (e.keyCode) {
                case KeyCode.Space:
                    Jump();
                    break;
            }
    }

    void Jump() {



        switch (state) {

            case State.Grappling:

                if (anmtr.GetBool("CornerGrab") && 
                    ((anmtr.GetBool("Face") && Input.GetAxis("Horizontal") > -0.5) || 
                    (!anmtr.GetBool("Face") && Input.GetAxis("Horizontal") < 0.5))) {                       // Character is @ top of wall and climbing over the corner

                    clldr.isTrigger = true;
                    anmtr.SetTrigger("Jump");

                    rgdbdy.velocity = anmtr.GetBool("Face") ? 
                        new Vector3(( speed/2) + Input.GetAxis("Horizontal") , ((cliffEdge.y - rgdbdy.transform.position.y) / 2) + (jHeight * 1.7f) , 0.0f): 
                        new Vector3((-speed/2) + Input.GetAxis("Horizontal") , ((cliffEdge.y - rgdbdy.transform.position.y) / 2) + (jHeight * 1.7f) , 0.0f);


                } else if ((anmtr.GetBool("Face") && Input.GetAxis("Horizontal") < 0.5) || 
                    (!anmtr.GetBool("Face") && Input.GetAxis("Horizontal") > -0.5)) {                       // Character is jumping off wall

                    rgdbdy.velocity += anmtr.GetBool("Face") ? new Vector3(0.5f, 0.0f, 0.0f) : new Vector3(-0.5f, 0.0f, 0.0f);
                    rgdbdy.velocity += new Vector3(jHeight * Input.GetAxis("Horizontal"), jHeight * Input.GetAxis("Vertical") + jHeight , 0.0f);

                    anmtr.SetBool("Face", !anmtr.GetBool("Face"));
                    anmtr.SetTrigger("Jump");
                }
                break;

            case State.Grounded:
                rgdbdy.velocity += new Vector3(Input.GetAxis("Horizontal"), jHeight, 0.0f);
                anmtr.SetTrigger("Jump");
                break;
        }
    }

    void OnAnimatorIK() {
        switch (state) {
            case State.Grappling:

                if (anmtr.GetIKPosition(AvatarIKGoal.RightHand).y >= cliffEdge.y) {                 // Character's left hand is grabing the top of the wall
                    anmtr.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.5f);
                    anmtr.SetIKPosition(AvatarIKGoal.RightHand, new Vector3(cliffEdge.x, cliffEdge.y - 0.0f, anmtr.GetIKPosition(AvatarIKGoal.RightHand).z));

                } else anmtr.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);


                if (anmtr.GetIKPosition(AvatarIKGoal.LeftHand).y >= cliffEdge.y) {                  // Character's left hand is grabing the top of the wall
                    anmtr.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.5f);
                    anmtr.SetIKPosition(AvatarIKGoal.LeftHand, new Vector3(cliffEdge.x, cliffEdge.y - 0.0f, anmtr.GetIKPosition(AvatarIKGoal.LeftHand).z));

                } else  anmtr.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);

            break;
        }
    }
    




        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //                                                                          Collisions && Triggers
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void OnTriggerExit(Collider c){
        clldr.isTrigger = false;
        }


        void OnCollisionEnter(Collision c) {

        for (int i = 0; i < c.contacts.Length; i++) {

            anmtr.ResetTrigger("Jump");
            anmtr.SetFloat("yCol", c.relativeVelocity.y);
            anmtr.SetFloat("xCol", c.relativeVelocity.x);

            switch (state) {
            case State.Aerial:
                if (c.contacts[i].normal.y > 0.1) {
                    state = State.Grounded;
                    rgdbdy.useGravity = true;

                } else if (i == 0 && (c.contacts[i].normal.x > 0.5 || c.contacts[0].normal.x < -0.5)) {
                    state = State.Grappling;
                    rgdbdy.useGravity = false;
                    rgdbdy.velocity = new Vector2(0, 0);
                    cliffEdge = new Vector2( c.contacts[i].point.x, c.contacts[i].otherCollider.bounds.max.y );
                   }
            break;
            }
        }
        
    }

    void OnCollisionStay(Collision c) {


        for (int i = 0; i < c.contacts.Length; i++) {

            switch (state){

            case State.Grappling:

                if (c.contacts[i].normal.y > 0.1) {
                    state = State.Grounded;
                    rgdbdy.useGravity = true;

                } else cliffEdge = new Vector2( c.contacts[i].point.x, c.contacts[i].otherCollider.bounds.max.y );
            break;

            case State.Aerial:
                if (c.contacts[0].normal.y > 0.1) {
                    state = State.Grounded;
                    rgdbdy.useGravity = true;

                } else if (i == 0 && (c.contacts[i].normal.x > 0.5 || c.contacts[0].normal.x < -0.5)) {
                    state = State.Grappling;
                    rgdbdy.useGravity = false;
                    rgdbdy.velocity = new Vector2(0, 0);
                }
            break;

            case State.Grounded:
                 if (Input.GetAxis("Vertical") < -.01)
                        state = State.Crouched;
            break;

            case State.Crouched:
                 if (Input.GetAxis("Vertical") > -.01)
                      state = State.Grounded;
            break;
            }
        }
    }

    void OnCollisionExit(Collision c){

        state = State.Aerial;
        rgdbdy.useGravity = true;
    }

}
