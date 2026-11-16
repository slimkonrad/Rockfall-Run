using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects a UI Slider to the AudioManager. It shows the saved volume when the menu opens
/// and updates (and saves) the volume whenever the slider moves.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private enum Channel { Music, Sfx }

    [SerializeField] private Channel channel = Channel.Music;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Read the saved value directly, since the AudioManager may apply it in the same frame.
        string key = channel == Channel.Music ? AudioManager.MusicVolumeKey : AudioManager.SfxVolumeKey;
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(key, 0.8f));
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (channel == Channel.Music)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        else
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
    }
}
