using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour, IPointerDownHandler
{

    public GameObject cardCursorPrefab;
    Hand hand;

    // Start is called before the first frame update
    void Start()
    {
        hand = GameObject.FindFirstObjectByType<Hand>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject o = GameObject.Instantiate<GameObject>(cardCursorPrefab, hand.gameObject.transform);
        o.transform.position = gameObject.transform.position; // Set world pos, but keep transform in hand so it gets animated to the right place.
        o.transform.rotation = Quaternion.Euler(0, -180, 0); // Spawn it upside down.
    }
}
