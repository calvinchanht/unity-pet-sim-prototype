using UnityEngine;
using TMPro;
using PetSimLite.Zone;

namespace PetSimLite.UI
{
    /// <summary>
    /// Minimal listener that logs roll results to a TMP text field.
    /// </summary>
    public class EggRollLog : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private int maxLines = 5;

        private readonly System.Collections.Generic.Queue<string> _lines = new System.Collections.Generic.Queue<string>();

        private void OnEnable()
        {
            EggRoller.EggRolled += OnEggRolled;
        }

        private void OnDisable()
        {
            EggRoller.EggRolled -= OnEggRolled;
        }

        private void OnEggRolled(EggRollResult result)
        {
            if (result.Pet == null) return;

            string line = $"Got {result.Pet.DisplayName} ({result.Pet.Rarity})";
            _lines.Enqueue(line);

            while (_lines.Count > maxLines)
            {
                _lines.Dequeue();
            }

            if (logText != null)
            {
                logText.text = string.Join("\n", _lines);
            }
        }
    }
}
