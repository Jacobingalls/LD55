using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;


public class DamageEffect : MonoBehaviour
{
    public Material DamageMaterial;
    public delegate void DamageComplete(DamageEffect dissolveEffect);

    [Range(0, 5.0f)]
    public float SinglePingPongDuration = 0.25f;
    public float DamageAmount;

    public bool DestroyOnCompletion;
    public bool IsDamaging;
    public int PingPongCount;
    private int _currentPingPong;

    public DamageComplete CompletionHandler;

    private SpriteRenderer _sr;

    private float _t;

    private Material _cachedMaterial;

    // Start is called before the first frame update
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _cachedMaterial = _sr.material;
        _sr.material = DamageMaterial;
    }

    private void OnDestroy()
    {
        Stop();
    }

    public void Reset()
    {
        IsDamaging = false;
        _t = 0;
        _currentPingPong = 0;
    }

    public void Stop()
    {
        if (!IsDamaging)
        {
            return;
        }

        if (_sr != null)
        {
            _sr.material = _cachedMaterial;
        }

        IsDamaging = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDamaging)
        {
            _t += Time.deltaTime;
            float pct = _t / SinglePingPongDuration;
            float dissolveAmount = pct;

            if (pct > 0.5f)
            {
                dissolveAmount = 1.0f - pct;
            }

            _sr.material.SetFloat("_DissolveAmount", dissolveAmount);

            if (pct > 1.0f)
            {
                if (_currentPingPong == PingPongCount - 1)
                {
                    IsDamaging = false;
                    if (CompletionHandler != null)
                    {
                        CompletionHandler(this);
                    }

                    if (DestroyOnCompletion)
                    {
                        Stop();
                        Destroy(this);
                    }
                    else
                    {
                        enabled = false;
                    }
                }
                else
                {
                    _currentPingPong += 1;
                    _t = 0.0f;
                }
            }
        }
    }
}
