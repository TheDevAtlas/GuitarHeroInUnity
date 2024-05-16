using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public float speed;
    public Material mat;

    public Renderer[] model;

    private void Start()
    {
        foreach(Renderer renderer in model)
        {
            renderer.material = mat;
        }
    }

    private void Update()
    {
        transform.Translate(new Vector3(speed * Time.deltaTime, 0f, 0f));

        if(transform.position.z < -5f)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Keys")
        {
            GuitarController guitarController = GameObject.FindGameObjectsWithTag("Guitar")[0].GetComponent<GuitarController>();
            print("Inside");
            if (guitarController != null && guitarController.freshStrum)
            {
                
                // Destroy the note if the strum was fresh
                Destroy(gameObject);
                
            }
        }
    }
}
