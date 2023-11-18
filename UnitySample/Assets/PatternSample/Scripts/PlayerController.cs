using System.Collections;
using TMPro;
using UnityEngine;

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
    private bool hitobstacle = false;
    private Vector3 _targetPosition = Vector3.zero;

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
            if(input.click)
            {
                cursor.UpdateCursor();
                RaycastResult result = cursor.GetRaycastResult();
                if(result.hitted)
                {
                    Vector3 targetPos = new Vector3(result.targetPos.x, result.targetPos.y + parameters.height, result.targetPos.z);
                    StartMoveToTarget(targetPos);
                    Debug.Log($"Cursor Position : {result.targetPos}");
                }
            }
        }
    }

    private void FixedUpdate()
    {
        var manager = SampleManager.GetInstance();
        if (manager == null)
        {
            return;
        }

        Vector3 moveVector = _moveDirection * parameters.moveSpeed * manager.GetTimeScale();
        transform.position += moveVector;
    }

    private Coroutine StartMoveToTarget(Vector3 targetPosition)
    {
        StopMove();
        _targetPosition = targetPosition;
        _Process = StartCoroutine(CoMoveToTarget());
        return _Process;
    }

    private void StopMove()
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

            if(hitobstacle)
            {
                moveEnd = true;
            }

            Vector3 deltaPos = _targetPosition - transform.position;
            float distance = deltaPos.magnitude;
            if (distance < parameters.moveSpeed)
            {
                transform.position = _targetPosition;
                moveEnd = true;
            }
            else
            {
                Vector3 moveDirection = deltaPos.normalized;
                Vector3 moveVector = moveDirection * parameters.moveSpeed * manager.GetTimeScale();
                transform.position += moveVector;
            }

            yield return new WaitForFixedUpdate();
        }
        hitobstacle = false;
        _moveDirection = Vector3.zero;
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

}
