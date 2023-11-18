using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InputManager
{
    public float horizontal { get; private set; }
    public float vertical { get; private set; }

    public bool click { get; private set; }

    private bool _Enable = false;

    public InputManager()
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
            click = Input.GetMouseButtonDown(0);
        }
        else
        {
            horizontal = 0.0f;
            vertical = 0.0f;
            click = false;
        }
    }

    public void SetEnable(bool flag)
    {
        _Enable= flag;
    }
}
