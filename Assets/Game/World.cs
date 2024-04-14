using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class World : MonoBehaviour
{
    public GridManager GridManager;

    private List<Unit> _units = new();
    
    public void RegisterUnit(Unit unit)
    {
        if (_units.Contains(unit))
        {
            Debug.LogError("Trying to register unit that has already been registered.");
            return;
        }

        _units.Add(unit);
    }

    public void UnregisterUnit(Unit unit)
    {
        if (!_units.Contains(unit))
        {
            Debug.LogError("Trying to unregister unit that has not been registered.");
            return;
        }

        _units.Remove(unit);
    }

    public ReadOnlyCollection<Unit> Units
    {
        get
        {
            return _units.AsReadOnly();
        }
    }


}
