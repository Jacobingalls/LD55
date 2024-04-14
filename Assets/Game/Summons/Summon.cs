using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PubSubSender))]
public class Summon : MonoBehaviour
{
    public SummonDefinition Definition;

    [HideInInspector]
    public GridManager GridManager;

    public string Name
    {
        get
        {
            return Definition.Name;
        }
    }

    private PubSubSender _pubSubSender;

    // Start is called before the first frame update
    void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
        if (GridManager == null)
        {
            // PANIK, try to find one
            GridManager = GameObject.FindObjectOfType<GridManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
