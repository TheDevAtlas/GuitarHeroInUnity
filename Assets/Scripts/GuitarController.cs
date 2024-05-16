
using UnityEngine;

public class GuitarController : MonoBehaviour
{
    public bool green;
    public bool red;
    public bool yellow;
    public bool blue;
    public bool orange;

    public Renderer g;
    public Renderer r;
    public Renderer y;
    public Renderer b;
    public Renderer o;

    public Material[] mats;
    public Material dmat;

    // Strum values: +1 for up-strum, -1 for down-strum, 0 for neutral //
    public float strum;

    private Input input;

    public bool freshStrum = false;
    int total = 24;

    void Awake()
    {
        input = new Input();

        // Color buttons //
        input.Gameplay.Green.performed += ctx => green = true;
        input.Gameplay.Green.canceled += ctx => green = false;
        input.Gameplay.Red.performed += ctx => red = true;
        input.Gameplay.Red.canceled += ctx => red = false;
        input.Gameplay.Yellow.performed += ctx => yellow = true;
        input.Gameplay.Yellow.canceled += ctx => yellow = false;
        input.Gameplay.Blue.performed += ctx => blue = true;
        input.Gameplay.Blue.canceled += ctx => blue = false;
        input.Gameplay.Orange.performed += ctx => orange = true;
        input.Gameplay.Orange.canceled += ctx => orange = false;

        // Strum bar //
        input.Gameplay.Strum.performed += ctx => Strum(ctx.ReadValue<float>());
        input.Gameplay.Strum.canceled += ctx => strum = 0;
    }

    public void Strum(float value)
    {
        if (strum == 0) // Strum is fresh if previous state was neutral
        {
            
            freshStrum = true;
        }
        strum = value;
    }

    private void Update()
    {
        if(freshStrum == true)
        {
            total--;
            if (total < 0)
            {
                total = 24;
                freshStrum = false;
            }
        }

        if (green)
        {
            g.material = mats[0];
        }
        else
        {
            g.material = dmat;
        }

        if (red)
        {
            r.material = mats[1];
        }
        else
        {
            r.material = dmat;
        }

        if (yellow)
        {
            y.material = mats[2];
        }
        else
        {
            y.material = dmat;
        }

        if (blue)
        {
            b.material = mats[3];
        }
        else
        {
            b.material = dmat;
        }

        if (orange)
        {
            o.material = mats[4];
        }
        else
        {
            o.material = dmat;
        }
    }

    void OnEnable()
    {
        input.Gameplay.Enable();
    }

    void OnDisable()
    {
        input.Gameplay.Disable();
    }
}