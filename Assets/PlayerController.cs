using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Controls the player's avatar, a sphere.

// This code from the Unity "Roll a Ball" tutorial example.

public class PlayerController : MonoBehaviour
{
    // Public variables can be modified in the Unity editor without changing this script! Neat, huh?
    // Can use them in place of magic numbers!
    public float speed;
    public Text KeyText;
    public Text WinText;


    private Rigidbody rb;
    private int keyCount;
    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        WinText.text = "";
    }

    // Update is called once per frame
    void Update()
    {

        // This code is verbatim from the "roll a ball" example.

        // Set some local float variables equal to the value of our Horizontal and Vertical Inputs
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Create a Vector3 variable, and assign X and Z to feature our horizontal and vertical float variables above
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Add a physical force to our Player rigidbody using our 'movement' Vector3 above, 
        // multiplying it by 'speed' - our4 public player speed that appears in the inspector
        rb.AddForce(movement * speed);
    }

    // This code similar to code from the "roll a ball" unity tutorial.
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("on trigger enter!");
        // ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
        if (other.gameObject.CompareTag("Pick Up"))
        {
            Debug.Log("That is a pcik up!");
            // Make the other game object (the pick up) inactive, to make it disappear
            other.gameObject.SetActive(false);

            // Add one to the score variable 'count'
            keyCount++;
            KeyText.text = "Keys: " + keyCount;

            // Run the 'SetCountText()' function (see below)
            //SetCountText();
        }
        if (other.gameObject.CompareTag("Openable Door"))
        {
            Debug.Log("I can open this door!");
            if(keyCount > 0)
            {
                keyCount--;
                other.gameObject.SetActive(false);
                KeyText.text = "Keys: " + keyCount;
            }
        }
        if (other.gameObject.CompareTag("Win"))
        {
            WinText.text = "You win!";
            other.gameObject.SetActive(false);
        }
    }
}
