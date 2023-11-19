using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class CharacterParameters
{
    public float height = 1.0f;
    public float moveSpeed = 0.5f;
}

public class PlayerController : MonoBehaviour
{
    public CursorController cursor;
    public CharacterParameters parameters;

    private Vector3 _moveDirection = Vector3.zero;

    private Coroutine _Process = null;
    private bool isProcessing => _Process != null;

    private Vector3 _targetPosition = Vector3.zero;
    private CommandManager _commandManager = new CommandManager();

    private void Awake()
    {
        cursor.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        var manager = SampleManager.GetInstance();
        if (manager == null)
        {
            return;
        }

        var input = manager.GetPlayerInput();
        if (input != null)
        {
            input.UpdateInput();
            if(input.leftClick || input.rightClick)
            {
                cursor.UpdateCursor();
                RaycastResult result = cursor.GetRaycastResult();
                if(result.hitted)
                {
                    Vector3 targetPos = new Vector3(result.targetPos.x, result.targetPos.y + parameters.height, result.targetPos.z);

                    if(input.leftClick)
                    {
                        cursor.PlayForceClickEffct();
                        ICommand moveCommand = new MoveCommand(this, targetPos);
                        _commandManager.AddCommand(moveCommand);
                    }
                    else
                    {
                        cursor.PlayClickEffct();
                        if(!isProcessing)
                        {
                            ICommand moveCommand = new MoveCommand(this, targetPos);
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
        _targetPosition = targetPosition;
        _Process = StartCoroutine(CoMoveToTarget());
        return _Process;
    }

    public void StopMove()
    {
        if (!isProcessing)
            return;

        StopCoroutine(_Process);
        _Process = null;
    }

    IEnumerator CoMoveToTarget()
    {
        bool moveEnd = false;
        while (!moveEnd)
        {
            var manager = SampleManager.GetInstance();
            if (manager == null)
            {
                moveEnd = true;
            }

            if (!moveEnd)
            {
                Vector3 deltaPos = _targetPosition - transform.position;
                float distance = deltaPos.magnitude;
                if (distance < parameters.moveSpeed)
                {
                    transform.position = _targetPosition;
                    moveEnd = true;
                }
                else
                {
                    _moveDirection = deltaPos.normalized;
                    Vector3 moveVector = _moveDirection * parameters.moveSpeed * manager.GetTimeScale();
                    transform.position += moveVector;
                }

                yield return new WaitForFixedUpdate();
            }
        }
        _moveDirection = Vector3.zero;
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