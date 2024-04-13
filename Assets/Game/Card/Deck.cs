using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using info.jacobingalls.jamkit;

public class Deck : MonoBehaviour, IPointerDownHandler
{

    public GameObject cardCursorPrefab;
    Hand hand;

    float timeUntilNextSpawn = 0f;
    float minTimeBetweenSpawns = 0.1f;
    int numberOfCardsWantingToBeSpawned = 0;

    // Start is called before the first frame update
    void Start()
    {
        hand = GameObject.FindFirstObjectByType<Hand>();
    }

    // Update is called once per frame
    void Update()
    {
        timeUntilNextSpawn = Mathf.Max(0, timeUntilNextSpawn - Time.deltaTime);
        if (timeUntilNextSpawn == 0 && numberOfCardsWantingToBeSpawned > 0)
        {
            numberOfCardsWantingToBeSpawned -= 1;
            SpawnCard();
            timeUntilNextSpawn = minTimeBetweenSpawns;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DrawCard();
    }

    public void DrawCard()
    {
        numberOfCardsWantingToBeSpawned += 1;
    }

    public void DrawCards(int number)
    {
        numberOfCardsWantingToBeSpawned += Mathf.Max(0, number);
    }

    public void DrawCardsEvent(PubSubListenerEvent e)
    {
        DrawCards((int)e.value);
    }

    public GameObject SpawnCard()
    {
        // TODO: Pick card to draw.
        GameObject o = GameObject.Instantiate<GameObject>(cardCursorPrefab, hand.gameObject.transform);
        o.transform.position = gameObject.transform.position; // Set world pos, but keep transform in hand so it gets animated to the right place.
        o.transform.rotation = Quaternion.Euler(0, -180, 0); // Spawn it upside down.
        return o;
    }
}
