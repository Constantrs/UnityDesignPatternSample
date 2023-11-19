using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IObserver
{
    public Image _achievementsImage;
    public Text _achievementsText;

    [SerializeField] private Subject _player;

    // Start is called before the first frame update
    private void OnEnable()
    {
        _player.AddObserver(this);
    }

    private void OnDisable()
    {
        _player.RemoveOvserver(this);
    }

    public void OnNotify()
    {
        ShowAchievement("ASD");
    }

    private void ShowAchievement(string text)
    {
        _achievementsImage.enabled = true;
        _achievementsText.enabled = true;
        _achievementsText.text = text;
    }

    private void HideAchievement()
    {
        _achievementsImage.enabled = false;
        _achievementsText.enabled = false;
        _achievementsText.text = "";
    }
}
