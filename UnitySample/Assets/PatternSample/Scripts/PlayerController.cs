using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PauseMessage : NotifyMessage
{
    public bool paused;

    public PauseMessage(bool pause)
    {
        this.paused = pause;
    }
}


[System.Serializable]
public class CharacterParameters
{
    public float moveSpeed = 0.5f;
}

public class PlayerController : Subject
{
    public CursorController cursor;
    public CharacterParameters parameters;


    private Coroutine _Process = null;
    private bool isProcessing => _Process != null;
    private SampleManager manager => SampleManager.GetInstance();

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _targetPosition = Vector3.zero;

    private Animator animator;
    private int _IdieHash;
    private int _RunHash;
    private int _IdieParamID;

    private CommandManager _commandManager = new CommandManager();

    private void Awake()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager == null)
        {
            return;
        }

        var input = manager.GetPlayerInput();
        if (input != null)
        {
            input.UpdateInput();
            if(input.pause) 
            {
                bool nextPauseFlag = !manager.pause;
                animator.speed = nextPauseFlag ? 0.0f : 1.0f;
                NotifyOvservers(new PauseMessage(nextPauseFlag));
                manager.pause = nextPauseFlag;
            }

            if(manager.GetTimeScale() != 0.0f && (input.leftClick || input.rightClick) )
            {
                cursor.CalculateRaycast();
                RaycastResult result = cursor.GetRaycastResult();
                if(result.hitted)
                {
                    Vector3 targetPos = new Vector3(result.targetPos.x, result.targetPos.y, result.targetPos.z);

                    if(input.leftClick)
                    {
                        cursor.PlayForceClickEffct();
                        ICommand moveCommand = new MoveCommand(this, transform.position, targetPos);
                        _commandManager.AddCommand(moveCommand);
                    }
                    else
                    {
                        cursor.PlayClickEffct();
                        if(!isProcessing)
                        {
                            ICommand moveCommand = new MoveCommand(this, transform.position, targetPos);
                            _commandManager.AddCommand(moveCommand);
                        }
                    }
                    //StartMoveToTarget(targetPos);
                    Debug.Log($"Cursor Position : {result.targetPos}");
                }
            }
        }
    }
    public Coroutine StartMoveToTarget(Vector3 targetPosition)
    {
        StopMove();
        _startPosition = transform.position;
        _targetPosition = targetPosition;
        _Process = StartCoroutine(CoMoveToTarget());
        return _Process;
    }

    public void StopMove()
    {
        if (!isProcessing)
            return;

        animator.SetBool(_IdieParamID, false);
        StopCoroutine(_Process);
        _Process = null;
    }

    private void Initialize()
    {
        cursor.Initialize();

        animator = GetComponent<Animator>();
        if (animator != null )
        {
            _IdieHash = Animator.StringToHash("Idie");
            _RunHash = Animator.StringToHash("Run");
            _IdieParamID = Animator.StringToHash("isMoving");
        }
    }
    IEnumerator CoMoveToTarget()
    {
        bool moveEnd = false;
        float moveDistance = (_targetPosition - _startPosition).magnitude;
        float moveTime = moveDistance / parameters.moveSpeed;
        float timer = 0.0f;

        transform.LookAt(_targetPosition);
        animator.SetBool(_IdieParamID, true);

        while (!moveEnd)
        {
            if (manager == null)
            {
                moveEnd = true;
            }

            if (timer >= moveTime)
            {
                transform.position = _targetPosition;
                moveEnd = true;
            }
            else
            {
                float timeRate = Mathf.Clamp(timer / moveTime, 0.0f, 1.0f);
                transform.position = Vector3.Lerp(_startPosition, _targetPosition, timeRate);
                timer += manager.GetTimeScale();
            }
            yield return null;
        }
        animator.SetBool(_IdieParamID, false);
        _Process = null;
    }

    //IEnumerator OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.layer == parameters.obstacleLayerMask)
    //    {
    //        Vector3 hitPos = other.ClosestPointOnBounds(this.transform.position);
    //        Vector3 hitDir = (transform.position - hitPos).normalized;
    //        transform.position += hitDir * parameters.moveSpeed;
    //    }
    //    yield return new WaitForFixedUpdate();
    //}

#if UNITY_EDITOR
    public void DisplayCommand()
    {
        List<string> commands = _commandManager.GetCommandListStr();

        foreach (string command in commands)
        {
            EditorGUILayout.LabelField(command);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerController))]
public class PlayerControllerInspector : Editor
{
    private PlayerController _controller = null;
    private bool _foldout = false;

    void OnEnable()
    {
        // AnyClassNameコンポーネントを取得
        _controller = target as PlayerController;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(_controller != null)
        {
            _foldout = EditorGUILayout.Foldout(_foldout, "Command List");

            if (_foldout)
            {
                EditorGUI.indentLevel++;
                _controller.DisplayCommand();
            }
        }
    }
}
#endif