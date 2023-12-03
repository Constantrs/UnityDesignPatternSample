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

    public class PlayerController : MonoBehaviour
    {
        private static readonly int _MoveParamID;
 
        public CursorController cursor;
        public CharacterParameters parameters;

        private SampleSceneManager manager => SampleSceneManager.GetInstance();

        private Coroutine _Process = null;
        private bool isProcessing => _Process != null;

        private Vector3 _defaultPostion = Vector3.zero;
        private Vector3 _startPosition = Vector3.zero;
        private Vector3 _targetPosition = Vector3.zero;

        private Animator animator;

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
                }

                if (manager.GetTimeMultiplier() != 0.0f)
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
                                StartMove(result.hitPosition);
                            }
                            // 待機状態だけ移動
                            else
                            {
                                cursor.PlayClickEffct(false);
                                if (!isProcessing)
                                {
                                    StartMove(result.hitPosition);
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
            _startPosition = transform.position;
            _targetPosition = targetPosition;
            _Process = StartCoroutine(CoMove());
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
            _defaultPostion = transform.position;
        }

        /// <summary>
        /// (コルーチン)プレイヤー移動
        /// </summary>
        IEnumerator CoMove()
        {
            bool moveEnd = false;
            float moveDistance = (_targetPosition - _startPosition).magnitude;
            float moveTime = moveDistance / parameters.moveSpeed;
            float timer = 0.0f;

            transform.LookAt(_targetPosition);
            animator.SetBool(_MoveParamID, true);
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
                    timer += manager.GetTimeMultiplier();
                }
                yield return null;
            }
            animator.SetBool(_MoveParamID, false);
            _Process = null;
        }
    }
}
