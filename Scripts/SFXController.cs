using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    public int explosionPoolAmount;
    public GameObject explosionPrefab;
    private Queue<GameObject> explosions1;
    private Queue<GameObject> explosions2;
    private bool isExplosions1InUse = true;
    public static SFXController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        explosions1 = new Queue<GameObject>();
        explosions2 = new Queue<GameObject>();
        CreateExplosionPool();
    }

    private void CreateExplosionPool()
    {
        for (int i = 0; i < explosionPoolAmount; i++)
        {
            explosions1.Enqueue(Instantiate(explosionPrefab));
        }
    }

    public void PlayExplosionSFX(Vector3 position)
    {
        Queue<GameObject> usedQueue;
        Queue<GameObject> nextQueue;
        //Switch between Queues
        if (isExplosions1InUse && explosions1.Count < 1) isExplosions1InUse = false;
        else if (!isExplosions1InUse && explosions2.Count < 1) isExplosions1InUse = true;

        if (isExplosions1InUse)
        {
            usedQueue = explosions1;
            nextQueue = explosions2;
        }
        else
        {
            usedQueue = explosions2;
            nextQueue = explosions1;
        }

        //Take from pool, play, and go back to pool.
        GameObject explosion = usedQueue.Dequeue();
        explosion.transform.position = position;
        if (explosion.TryGetComponent(out ParticleSystem ps))
        {
            ps.Play();
        }
        nextQueue.Enqueue(explosion);
    }
}
