using UnityEngine;
using UnityEngine.Terminal;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject, IWithTerminalFeatureSettings
{
    [field: SerializeField]
    public TerminalSettings TerminalSettings { get; set; }
}
