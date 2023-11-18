using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerInput
{
    public float horizontal { get; private set; }
    public float vertical { get; private set; }

    private bool _Enable = false;

    public PlayerInput()
    {
        _Enable = true;
    }

    // Start is called before the first frame update
    public void UpdateInput()
    {
        if (_Enable)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }
        else
        {
            horizontal = 0.0f;
            vertical = 0.0f;
        }
    }

    public void SetEnable(bool flag)
    {
        _Enable= flag;
    }
}
