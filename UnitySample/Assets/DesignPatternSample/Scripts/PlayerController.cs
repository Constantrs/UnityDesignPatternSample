using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DesignPatternSample
{

    [System.Serializable]
    public class CharacterParameters
    {
        // キャラ移動速度
        public float moveSpeed = 0.5f;
    }

    public class MoveCommand : ICommand
    {
        private const string CommandName = "MoveCommand";

        private PlayerController _Controller;
        private Vector3 _StartPos;
        private Vector3 _TartgetPos;
        private Quaternion _StartRot;

        public MoveCommand(PlayerController controller, Vector3 startPos, Vector3 targetPos, Quaternion startRot)
        {
            _Controller = controller;
            _StartPos = startPos;
            _TartgetPos = targetPos;
            _StartRot = startRot;
        }

        public override void Execute()
        {
            _Controller.StartMove(_TartgetPos);
        }

        public override void Undo()
        {
            _Controller.StartReverseMove(_StartPos, _StartRot);
        }

        public override string GetCommandName()
        {
            string str = CommandName;
            str += $" From : {_StartPos} ";
            str += $" To : {_TartgetPos} ";
            return str;
        }
    }

    public class PauseMessage : NotifyMessage
    {
        public bool paused;

        public PauseMessage(bool pause)
        {
            this.paused = pause;
        }
    }


    public class PlayerController : Subject
    {
        private static readonly int _MoveParamID;
 
        public CursorController cursor;
        public CharacterParameters parameters;
        private SampleSceneManager manager => SampleSceneManager.GetInstance();

        private Coroutine _Process = null;
        private bool isProcessing => _Process != null;

        private Animator animator;

        private CommandManager _commandManager = new CommandManager();

        static PlayerController()
        {
            // アニメーター用ハッシュ初期化
            _MoveParamID = Animator.StringToHash("isMoving");
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (manager == null)
            {
                return;
            }

            var input = manager.GetInput();
            if (input != null)
            {
                // 入力更新
                input.UpdateInput();

                // 一時停止
                if (input.pause)
                {
                    bool nextPauseFlag = !manager.GetPauseFlag();
                    animator.speed = nextPauseFlag ? 0.0f : 1.0f;
                    manager.SetPauseFlag(nextPauseFlag);
                    NotifyOvservers(new PauseMessage(nextPauseFlag));
                }

                if (input.undo)
                {
                    _commandManager.UndoCommand();
                }
                else if (manager.GetTimeMultiplier() != 0.0f)
                {
                    // クリック座標へ移動
                    if (input.leftClick || input.rightClick)
                    {
                        RaycastResult result = cursor.GetRaycastResult();
                        if (result.hitted)
                        {
                            cursor.PlayClickEffct(input.leftClick);
                            // 移動
                            if (input.leftClick)
                            {
                                cursor.PlayClickEffct(true);
                                ICommand moveCommand = new MoveCommand(this, transform.position, result.hitPosition, transform.rotation);
                                _commandManager.AddCommand(moveCommand);
                                //StartMove(result.hitPosition);
                            }
                            // 待機状態だけ移動
                            else
                            {
                                cursor.PlayClickEffct(false);
                                if (!isProcessing)
                                {
                                    ICommand moveCommand = new MoveCommand(this, transform.position, result.hitPosition, transform.rotation);
                                    _commandManager.AddCommand(moveCommand);
                                    //StartMove(result.hitPosition);
                                }
                            }
                            Debug.Log($"Cursor Position : {result.hitPosition}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// プレイヤー移動開始
        /// </summary>
        public Coroutine StartMove(Vector3 targetPosition)
        {
            StopMove();
            _Process = StartCoroutine(CoMove(targetPosition));
            return _Process;
        }

        /// <summary>
        /// プレイヤー逆行移動開始
        /// </summary>
        public Coroutine StartReverseMove(Vector3 originalPosition, Quaternion originalRotation)
        {
            StopMove();
            _Process = StartCoroutine(CoReverseMove(originalPosition, originalRotation));
            return _Process;
        }


        /// <summary>
        /// プレイヤー移動停止
        /// </summary>
        public void StopMove()
        {
            if (!isProcessing)
                return;

            animator.SetBool(_MoveParamID, false);
            StopCoroutine(_Process);
            _Process = null;
        }

        /// <summary>
        /// プレイヤー初期化
        /// </summary>
        private void Initialize()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// (コルーチン)プレイヤー移動
        /// </summary>
        IEnumerator CoMove(Vector3 targetPosition)
        {
            bool moveEnd = false;
            Vector3 startPosition = transform.position;
            float moveDistance = (targetPosition - startPosition).magnitude;
            float moveTime = moveDistance / parameters.moveSpeed;
            float timer = 0.0f;

            transform.LookAt(targetPosition);
            animator.SetBool(_MoveParamID, true);
            while (!moveEnd)
            {
                if (manager == null)
                {
                    moveEnd = true;
                }

                if (timer >= moveTime)
                {
                    transform.position = targetPosition;
                    moveEnd = true;
                }
                else
                {
                    float timeRate = Mathf.Clamp(timer / moveTime, 0.0f, 1.0f);
                    transform.position = Vector3.Lerp(startPosition, targetPosition, timeRate);
                    timer += manager.GetTimeMultiplier();
                }
                yield return null;
            }
            animator.SetBool(_MoveParamID, false);
            _Process = null;
        }

        /// <summary>
        /// (コルーチン)逆行移動開始
        /// </summary>
        IEnumerator CoReverseMove(Vector3 originalPosition, Quaternion originalRotation)
        {
            bool moveEnd = false;
            Vector3 startPosition = transform.position;
            float moveDistance = (originalPosition - startPosition).magnitude;
            float moveTime = moveDistance / parameters.moveSpeed;
            float timer = 0.0f;

            animator.SetBool(_MoveParamID, true);

            while (!moveEnd)
            {
                if (manager == null)
                {
                    moveEnd = true;
                }

                if (timer >= moveTime)
                {
                    transform.position = originalPosition;
                    moveEnd = true;
                }
                else
                {
                    float timeRate = Mathf.Clamp(timer / moveTime, 0.0f, 1.0f);
                    transform.position = Vector3.Lerp(startPosition, originalPosition, timeRate);
                    timer += manager.GetTimeMultiplier();
                }
                yield return null;
            }
            transform.rotation = originalRotation;
            animator.SetBool(_MoveParamID, false);
            _Process = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// コマンド名称表示
        /// </summary>
        public void DisplayCommandsName()
        {
            List<string> commands = _commandManager.GetCommandNameList();

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
            _controller = target as PlayerController;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_controller != null)
            {
                _foldout = EditorGUILayout.Foldout(_foldout, "Command List");

                if (_foldout)
                {
                    EditorGUI.indentLevel++;
                    _controller.DisplayCommandsName();
                }
            }
        }
    }
#endif
}
