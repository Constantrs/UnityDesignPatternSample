using UnityEngine;

public class ItemController : MonoBehaviour
{
    public float rotateSpeed = 1.0f;
    private float _EulerY = 0.0f;

    // Update is called once per frame
    void Update()
    {
        var manager = SampleManager.GetInstance();
        if (manager == null)
        {
            return;
        }

        _EulerY += rotateSpeed * manager.GetTimeScale();
        if (_EulerY > 360.0f)
        {
            _EulerY -= 360.0f;
        }

        transform.rotation = Quaternion.Euler(new Vector3(0.0f, _EulerY, 0.0f));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ItemDestory();
        }
    }

    private void ItemDestory()
    {
        Destroy(gameObject);
    }
}
