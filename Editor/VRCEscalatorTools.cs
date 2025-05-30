using UnityEditor;
using UnityEngine;

namespace Asriels.Escalators.Toolkit
{
    public class VRCEscalatorTools : MonoBehaviour
    {
        [MenuItem("GameObject/VRChat/Escalator")]
        static void AddPrefab()
        {
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/dev.misutaaasriel.vrcescalator/Samples/Escalator Utility.prefab", typeof(GameObject));

            if (!prefab)
                return;

            Transform currentSelection = Selection.activeTransform;

            switch (currentSelection == null)
            {
                case true:
                    Instantiate(prefab).name = prefab.name;
                    break;
                default:
                    Instantiate(prefab, currentSelection).name = prefab.name;
                    break;
            }
        }
    }
}