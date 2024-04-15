
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveManager))]
public class WaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var waveManager = target as WaveManager;
        
        if(GUILayout.Button("Start Next Wave"))
        {
            waveManager.StartNextWave();
        }
    }
}
