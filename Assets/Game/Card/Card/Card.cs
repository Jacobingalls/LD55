using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public struct CardExecutionContext
{
    public CardActionDefinition actionDefinition;
    public GridManager gridManager;

    public Vector3 position;
    public TileData? target;
    public TileConfig? tileConfig;

    public override string ToString() => $"<ExecutionContext: action={actionDefinition.Name}, target={target}>";

    public CardExecutionContext(CardActionDefinition actionDefinition, GridManager gridManager, Vector3 point)
    {
        this.actionDefinition = actionDefinition;
        this.gridManager = gridManager;
        this.position = point;

        Vector2Int coord = gridManager.ToWorldPositionTileCoordinate(point);
        this.target = gridManager.GetTileData(coord);
        this.tileConfig = gridManager.GetTileConfig(coord);
    }

    // ignoreExistingEntities is to that we can use one function to decide if this is a buildable area, but already occupied.
    public bool ValidPlacementIgnoringExistingEntities(bool ignoreExistingEntities)
    {
        switch (actionDefinition.Target)
        {
            case CardActionTarget.EmptyBuildableArea:
                if (ignoreExistingEntities) {
                    return walkable || summonable;
                } else {
                    return summonable && empty;
                }
            case CardActionTarget.GameState:
                return true;
            default:
                return false;
        }
        return false;
    }

    public bool walkable
    {
        get
        {
            if (tileConfig is TileConfig c)
            {
                return c.walkable;
            }
            return false;
        }
    }

    public bool summonable
    {
        get
        {
            if (tileConfig is TileConfig c)
            {
                return c.summonable;
            }
            return false;
        }
    }

    public bool empty
    {
        get
        {
            if (target is TileData d)
            {
                return !gridManager.HasSummon(d.Position);
            }
            return true;
        }
    }

    public bool Validate()
    {
        var self = this;
        return actionDefinition.Behaviors.TrueForAll(b => b.CanExecute(self));
    }

    public void Execute()
    {
        if (Validate())
        {
            foreach (var behavior in actionDefinition.Behaviors)
            {
                behavior.Execute(this);
            }
        }
    }

    public bool clicksToGrid
    {
        get
        {
            switch (actionDefinition.Target)
            {
                case CardActionTarget.EmptyBuildableArea:
                    return true;
                default:
                    return false;
            }
        }
    }
}

public class Card : MonoBehaviour
{
    public CardActionDefinition actionDefinition;
    public Deck deck;

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI flavorText;
    public Image image;

    public void Start()
    {
        title.text = actionDefinition.Name;
        description.text = actionDefinition.Description;
        flavorText.text = actionDefinition.FlavorText;
        image.sprite = actionDefinition.Icon;
    }

    public bool Validate(CardExecutionContext context)
    {
        return context.Validate();
    }

    public void Execute(CardExecutionContext context)
    {
        context.Execute();
    }

    public void ReturnToDeck()
    {
        deck.AddCardToDiscard(actionDefinition);
    }
}
