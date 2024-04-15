using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public enum UnitType
{
    Paper = 0,
    Digital,
    Courier,
    Legal,
    Vehicle
}

[RequireComponent(typeof(PubSubSender))]
public class Unit : MonoBehaviour, IDamageable
{
    delegate void MoveCompletionHandler();

    [Header("Info")]
    public string Name = "Unit";
    public string Description = "Description of the unit.";
    public string FlavorText = "\"Flavor text for the unit.\"";
    public Sprite Icon;
    public Color Color = Color.blue;
    public UnitType Type;

    [Range(1, 100)]
    public int BaseMaxHealth = 10;

    [Range(0.1f, 10.0f)]
    public float BaseMoveSpeed = 5.0f;

    [Header("Visuals")]
    public Material DeathShader;
    public Material SuccessShader;
    public Material DamageShader;
    private SpriteProgressBar _healthBar;

    [Range(0.0f, 5.0f)]
    public float DamageAnimationPingPongTime = 0.25f;
    [Range(1, 5)]
    public int NumberOfPingPongs = 2;
    private bool _isBeingDamaged = false;

    public bool PlayDamageAnimation = true;

    [Range(0.0f, 5.0f)]
    public float DeathAnimationTime = 0.5f;

    public bool PlayDeathAnimation = true;

    [Range(0.0f, 5.0f)]
    public float SuccessAnimationTime = 0.25f;

    public bool PlaySuccessAnimation = true;

    [Header("Stats")]
    [SerializeField]  private int _health;

    private GameLevel _gameLevel;

    public float MoveSpeed
    {
        get
        {
            return BaseMoveSpeed;
        }
    }

    public int Health
    {
        get { return _health; }
        private set
        {
            if (_health == value) { return; }
            _health = value;
            _pubSubSender.Publish("unit.health.changed", _health);
            _pubSubSender.Publish("unit.resources.changed", _health);
        }
    }

    public bool Alive
    {
        get
        {
            return _alive;
        }
    }
    private bool _alive = true;

    private bool _invulnerable = false;

    public int MaxHealth { get { return BaseMaxHealth; } }

    private PubSubSender _pubSubSender;

    // Start is called before the first frame update
    void Awake()
    {
        _healthBar = GetComponentInChildren<SpriteProgressBar>();

        _pubSubSender = GetComponent<PubSubSender>();
        if (_gameLevel == null)
        {
            // PANIK, try to find one in our hierarchy
            _gameLevel = GetComponentInParent<GameLevel>();

            if (_gameLevel == null)
            {
                var levelManager = FindObjectOfType<LevelManager>();
                _gameLevel = levelManager.ActiveLevel;
            }
        }

        Health = MaxHealth;

        _gameLevel.RegisterUnit(this);
    }

    private void OnDestroy()
    {
        _gameLevel.UnregisterUnit(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(List<Vector2Int> path)
    {
        if (moving) { return; }
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        moving = true;
        MoveCompletionHandler handler = () =>
        {
            _invulnerable = true;
            Succeed();
        };

        StartCoroutine(MoveAlongPath(new List<Vector2Int>(path), handler));
    }

    bool moving = false;

    private bool positionIsCloseEnoughToTarget(Vector3 destination)
    {
        return Vector3.Distance(transform.position, destination) < 0.015f;
    }

    IEnumerator MoveAlongPath(List<Vector2Int> path, MoveCompletionHandler completionHandler)
    {
        Vector2Int startNode = path.First();
        path.RemoveAt(0);

        Vector3 startPosition = _gameLevel.GridManager.TileCoordinateToWorldPosition(startNode);

        startNode = path.First();
        path.RemoveAt(0);
        Vector3 nextPosition = _gameLevel.GridManager.TileCoordinateToWorldPosition(startNode);

        var t = 0.0f;
        var speed = MoveSpeed; // meters per second
        var timeToWalkAcrossTile = 1 / speed;

        while (!positionIsCloseEnoughToTarget(nextPosition) || path.Count > 0)
        {
            t += Time.deltaTime;
            float progress = t / timeToWalkAcrossTile;

            var direction = new Vector3(nextPosition.x - startPosition.x, nextPosition.y - startPosition.y, 0.0f).normalized;
            transform.position = new Vector3(startPosition.x, startPosition.y, transform.position.z) + (direction * progress); // ew, becky. ew.

            if (t > timeToWalkAcrossTile)
            {
                if (path.Count > 0)
                {
                    t = 0.0f;
                    startPosition = nextPosition;
                    nextPosition = _gameLevel.GridManager.TileCoordinateToWorldPosition(path.First());
                    path.RemoveAt(0);
                }
            }

            yield return null;
        }

        moving = false;
        transform.position = nextPosition;

        completionHandler();
    }

    public float DamageMultiplier(IDamageSource damageSource)
    {
        const float normalMultiplier = 1.0f;
        const float weakMultiplier = 1.51f;
        const float resistantMultiplier = 0.49f;

        switch (damageSource.GetDamageType())
        {
            case DamageType.Electricity:
                return Type == UnitType.Digital ? weakMultiplier : normalMultiplier;
            case DamageType.Explosive:
                return Type == UnitType.Vehicle ? weakMultiplier : normalMultiplier;
            case DamageType.Ink:
                return Type == UnitType.Paper ? weakMultiplier : normalMultiplier;
            case DamageType.Legal:
                return Type == UnitType.Legal ? weakMultiplier : normalMultiplier;
            case DamageType.Dog:
                return Type == UnitType.Courier ? weakMultiplier : normalMultiplier;
            default:
                return normalMultiplier;
        }
    }

    public void Damage(IDamageSource damageSource)
    {
        if (_invulnerable || Health <= 0)
        {
            return;
        }

        var oldHealth = Health;

        var trueDamage = Mathf.RoundToInt(damageSource.GetDamageAmount() * DamageMultiplier(damageSource));

        Health = Mathf.Max(Health - trueDamage, 0);

        if (_healthBar != null)
        {
            _healthBar.CurrentProgress = ((float)Health / (float)MaxHealth);
        }

        PlayHurtAudio();

        if (Health <= 0)
        {
            Kill();
        }
        else
        {
            StartCoroutine(DamageCoroutine());
        }
    }

    private IEnumerator DamageCoroutine()
    {
        _isBeingDamaged = true;

        if (!PlayDamageAnimation)
        {
            yield break;
        }

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in spriteRenderers)
        {
            if (sr.gameObject.TryGetComponent<DamageEffect>(out var dissolveEffect))
            {
                dissolveEffect.Reset();
            }
            else
            {
                dissolveEffect = sr.gameObject.AddComponent<DamageEffect>();
                dissolveEffect.CompletionHandler = (DamageEffect de) => {
                    _isBeingDamaged = false;
                };
            }
            dissolveEffect.DestroyOnCompletion = true;
            dissolveEffect.DamageMaterial = DamageShader;
            dissolveEffect.DamageAmount = 0;
            dissolveEffect.SinglePingPongDuration = DamageAnimationPingPongTime;
            dissolveEffect.IsDamaging = true;
            dissolveEffect.PingPongCount = NumberOfPingPongs;
        }
    }

    void PlayDeathAudio()
    {
        AudioManager.Instance.Play("Units/Death",
            pitchMin: 0.9f, pitchMax: 1.1f,
            volumeMin: 0.25f, volumeMax: 0.25f,
            position: transform.position,
            minDistance: 10, maxDistance: 20);
    }

    void PlayHurtAudio()
    {
        AudioManager.Instance.Play("Units/Hit",
            pitchMin: 0.9f, pitchMax: 1.1f,
            volumeMin: 1.0f, volumeMax: 1.0f,
            position: transform.position,
            minDistance: 10, maxDistance: 20);
    }

    public void Kill()
    {
        if (_invulnerable)
        {
            return;
        }

        if (!_alive)
        {
            return;
        }

        _alive = false;
        Health = 0;

        Destroy(GetComponent<Targetable>());

        GetComponent<PubSubSender>().Publish("unit.slain");

        StartCoroutine(DeathCoroutine());
    }

    private IEnumerator DeathCoroutine()
    {
        PlayDeathAudio();

        if (PlayDeathAnimation == false)
        {
            _pubSubSender.Publish("entity.died", this);
            Destroy(gameObject);
            yield break;
        }

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in spriteRenderers)
        {
            if (sr.gameObject.TryGetComponent<DamageEffect>(out var damageEffect))
            {
                damageEffect.Stop();
                Destroy(damageEffect);
            }

            var dissolveEffect = sr.gameObject.AddComponent<DissolveEffect>();
            dissolveEffect.DissolveMaterial = DeathShader;
            dissolveEffect.DissolveAmount = 0;
            dissolveEffect.Duration = DeathAnimationTime;
            dissolveEffect.IsDissolving = true;
        }

        yield return new WaitForSeconds(DeathAnimationTime);

        _pubSubSender.Publish("unit.died", this);

        Destroy(gameObject);
    }

    public void Succeed()
    {
        GetComponent<PubSubSender>().Publish("unit.reached.goal");

        StartCoroutine(SuccessCoroutine());
    }

    void PlaySuccessAudio()
    {
        AudioManager.Instance.Play("Units/EnterPortal",
            pitchMin: 0.6f, pitchMax: 1.2f,
            volumeMin: 0.25f, volumeMax: 0.25f,
            position: transform.position,
            minDistance: 10, maxDistance: 20);
    }

    private IEnumerator SuccessCoroutine()
    {
        PlaySuccessAudio();

        if (PlaySuccessAnimation == false)
        {
            Destroy(gameObject);
            yield break;
        }

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in spriteRenderers)
        {
            var dissolveEffect = sr.gameObject.AddComponent<DissolveEffect>();
            dissolveEffect.DissolveMaterial = SuccessShader;
            dissolveEffect.DissolveAmount = 0;
            dissolveEffect.Duration = SuccessAnimationTime;
            dissolveEffect.IsDissolving = true;
        }

        yield return new WaitForSeconds(SuccessAnimationTime);

        Destroy(gameObject);

    }
}
