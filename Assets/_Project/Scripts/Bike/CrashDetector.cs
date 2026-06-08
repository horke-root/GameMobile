using System;
using UnityEngine;





public class CrashDetector : MonoBehaviour
{

    public Action onCrash;

    public string groundTag = "Ground";




    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag) || collision.gameObject.CompareTag("Dirt"))
        {
            onCrash?.Invoke();
        }
    }

}
