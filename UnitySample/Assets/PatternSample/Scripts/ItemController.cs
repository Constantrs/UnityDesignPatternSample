using System.Collections;
using UnityEngine;

public class UnlockAchievementMessage : NotifyMessage
{
    public int achievementId;

    public UnlockAchievementMessage(int achievementId)
    {
        this.achievementId = achievementId;
    }
}

public class ItemController : Subject
{
    public float rotateSpeed = 1.0f;
    public float fadeOutTime = 30.0f;

    private float _EulerY = 0.0f;
    private bool _Destory = false;

    private SampleManager manager => SampleManager.GetInstance();
    // Update is called once per frame
    void Update()
    {
        if (manager == null)
        {
            return;
        }

        _EulerY += rotateSpeed * manager.GetTimeScale();
        if (_EulerY > 360.0f)
        {
            _EulerY -= 360.0f;
        }

        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = _EulerY;
        transform.rotation = Quaternion.Euler(euler);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(!_Destory)
            {
                _Destory = true;
                StartCoroutine(CoDestory());
            }
        }
    }

    IEnumerator CoDestory()
    {
        bool fadeOutEnd = false;
        float timer = 0.0f;
        Vector3 originalScale = transform.localScale;
        NotifyOvservers(new UnlockAchievementMessage(0));
        while (!fadeOutEnd) 
        {
            if (manager == null)
            {
                fadeOutEnd = true;
            }

            if (timer >= fadeOutTime)
            {
                transform.localScale = Vector3.zero;
                fadeOutEnd = true;
            }
            else
            {
                float timeRate = Mathf.Clamp(timer / fadeOutTime, 0.0f, 1.0f);
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timeRate);
                timer += manager.GetTimeScale();
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
