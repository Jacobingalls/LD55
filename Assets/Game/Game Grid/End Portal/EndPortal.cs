using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PubSubListener))]
[RequireComponent(typeof(PubSubSender))]
public class EndPortal : MonoBehaviour
{
    [SerializeField][Range(1, 40)] private int _startingLife = 10;

    [Header("Visuals")]
    public Material DeathShader;

    [Range(0.0f, 5.0f)]
    public float DeathAnimationTime = 2.5f;


    public int CurrentLife
    {
        get
        {
            return _currentLife;
        }
        set
        {
            if (_currentLife == value)
            {
                return;
            }

            _currentLife = value;

            var progressBar = GetComponentInChildren<SpriteProgressBar>();
            if (progressBar != null)
            {
                progressBar.CurrentProgress = (float)_currentLife / _startingLife;
            }
        }
    }

    private int _currentLife;
    private GameLevel _gameLevel;

    public void Start()
    {
        _gameLevel = GetComponentInParent<GameLevel>();
        _currentLife = _startingLife;
        _gameLevel.RegisterEndPortal(this);
    }

    public void UnitDidEnterPortal()
    {
        CurrentLife -= 1;

        if (CurrentLife == 0)
        {
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
            StartCoroutine(DestroyPortalCoroutine());
        }
    }

    void PlayDeathAudio()
    {
        AudioManager.Instance.Play("EndPortal/Death",
            pitchMin: 0.9f, pitchMax: 1.1f,
            volumeMin: 0.25f, volumeMax: 0.25f,
            position: transform.position,
            minDistance: 10, maxDistance: 20);
    }

    public IEnumerator DestroyPortalCoroutine()
    {
        PlayDeathAudio();

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in spriteRenderers)
        {
            var dissolveEffect = sr.gameObject.AddComponent<DissolveEffect>();
            dissolveEffect.DissolveMaterial = DeathShader;
            dissolveEffect.DissolveAmount = 0;
            dissolveEffect.Duration = DeathAnimationTime;
            dissolveEffect.IsDissolving = true;
        }

        yield return new WaitForSeconds(DeathAnimationTime);

        Destroy(gameObject);

        var sender = GetComponent<PubSubSender>();
        sender.Publish("gameManager.showLose");
    }
}
