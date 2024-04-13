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
    public TileData? target;

    public override string ToString() => $"<ExecutionContext: action={actionDefinition.Name}, target={target}>";

    public CardExecutionContext(CardActionDefinition actionDefinition, GridManager gridManager, Vector3 point)
    {
        this.actionDefinition = actionDefinition;
        this.gridManager = gridManager;

        Vector2Int coord = gridManager.ToWorldPositionTileCoordinate(point);
        this.target = gridManager.GetTileData(coord);
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
}

public class Card : MonoBehaviour
{
    public CardActionDefinition actionDefinition;

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
}
