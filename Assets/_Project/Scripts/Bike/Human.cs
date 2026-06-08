
using UnityEngine;






public class Human : MonoBehaviour
{

    public CrashDetector crashDetector;

    public HingeJoint2D bodyHingeJoint;

    public HingeJoint2D handHingeJoint;

    public HingeJoint2D legHingeJoint;

    public HingeJoint2D upLegHingeJoint;

    public Rigidbody2D head;





    public void Kill()
    {
        head.gravityScale = 1f;
        bodyHingeJoint.GetComponent<Rigidbody2D>().gravityScale = 1f;


        handHingeJoint.enabled = false;
        legHingeJoint.enabled = false;
        upLegHingeJoint.enabled = false;
    }
}
