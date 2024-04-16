using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonAttackAbility : SummonAbility
{
    public List<GameObject> ProjectilePrefabs = new();


    [Header("Visuals")]
    public bool RandomProjectileHeading = true;
    [Range(0, -180)]
    public float RandomAngleMin = -50;
    [Range(0, 180)]
    public float RandomAngleMax = 50;


    public bool SpawnJitter = true;
    public Vector3 JitterMin = new Vector3(-0.1f, -0.1f, 0.0f);
    public Vector3 JitterMax = new Vector3(0.1f, 0.1f, 0.0f);

    [Header("Volley")]
    [Range(1, 10)]
    public int NumProjectilesInVolley = 1;
    [Range(0.0f, 1.0f)]
    public float TimeBetweenProjectilesInVolley = 0.250f;

    override public void Update()
    {
        base.Update();
    }

    public override bool CanExecute(ExecutionContext context)
    {
        return !OnCooldown() && !_busy;
    }

    public override bool Execute(ExecutionContext context)
    {
        _busy = true;
        StartCoroutine(SpawnProjectiles(context));

        return true;
    }

    private IEnumerator SpawnProjectiles(ExecutionContext context)
    {
        var t = 0.0f;
        var firedProjectiles = 0;
        while (firedProjectiles < NumProjectilesInVolley)
        {
            t += Time.deltaTime;

            if (t >= TimeBetweenProjectilesInVolley)
            {
                foreach (var projectilePrefab in ProjectilePrefabs)
                {
                    var projectileGO = Instantiate(projectilePrefab);
                    projectileGO.transform.parent = transform;
                    projectileGO.transform.position = context.originPosition;

                    if (SpawnJitter)
                    {
                        projectileGO.transform.position += new Vector3(
                            UnityEngine.Random.Range(JitterMin.x, JitterMin.y),
                            UnityEngine.Random.Range(JitterMax.x, JitterMax.y),
                            0.0f
                        );
                    }

                    if (RandomProjectileHeading)
                    {
                        projectileGO.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(RandomAngleMin, RandomAngleMax));
                    }

                    projectileGO.GetComponent<Projectile>().SetTarget(context.target);
                }

                t -= TimeBetweenProjectilesInVolley;
                firedProjectiles += 1;
            }

            yield return null;
        }

        _busy = false;
        _currentCooldown = Cooldown;
    }
}
