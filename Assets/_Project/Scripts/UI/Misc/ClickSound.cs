using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class ClickSound : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioClip clip;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.8f;
    [SerializeField] private AudioSource audioSource;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void Play()
    {
        OnPointerClick(null);
    }
}
