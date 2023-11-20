using UnityEngine;
public class InputManager
{
    public float horizontal { get; private set; }
    public float vertical { get; private set; }

    public bool leftClick { get; private set; }

    public bool rightClick { get; private set; }

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
            leftClick = Input.GetMouseButtonDown(0);
            rightClick = Input.GetMouseButtonDown(1);
        }
        else
        {
            horizontal = 0.0f;
            vertical = 0.0f;
            leftClick = false;
            rightClick = false;
        }
    }

    public void SetEnable(bool flag)
    {
        _Enable= flag;
    }
}
