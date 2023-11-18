using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterController : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    private Vector3 _inputDirection = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        var manager = SampleManager.GetInstance();
        if (manager == null)
        {
            return;
        }

        _inputDirection = Vector3.zero;
        var input = manager.GetPlayerInput();
        if (input != null)
        {
            input.UpdateInput();
            _inputDirection = new Vector3(input.horizontal, 0.0f, input.vertical).normalized;
        }
    }

    private void FixedUpdate()
    {
        var manager = SampleManager.GetInstance();
        if (manager == null)
        {
            return;
        }

        Vector3 moveDirection = _inputDirection;
        Vector3 moveVector = moveDirection * moveSpeed * manager.GetTimeScale();
        transform.position += moveVector;
    }

    IEnumerator OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            Vector3 hitPos = other.ClosestPointOnBounds(this.transform.position);
            Vector3 hitDir = (transform.position - hitPos).normalized;
            transform.position += hitDir * moveSpeed;
        }
        yield return new WaitForFixedUpdate();
    }

}
