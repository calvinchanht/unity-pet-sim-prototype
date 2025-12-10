using UnityEditor;

namespace PetSimLite.Editor
{
    /// <summary>
    /// Automatically runs CSV importer when entering Play mode to keep generated assets in sync.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoCSVImportOnPlay
    {
        static AutoCSVImportOnPlay()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                CSVImporter.ImportAll();
            }
        }
    }
}
