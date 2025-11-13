using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "FootstepMaterialDatabase", menuName = "Audio/Footstep Material Database")]
public class FootstepMaterialDatabase : ScriptableObject
{
    //Table de correspondance Material unity -> Switch Wwise
    [System.Serializable]
    public class MaterialMapping
    {
        [Tooltip("Le Material Unity (peut être null si tu utilises l'auto-détection)")]
        public Material material;
        [Tooltip("Le Switch Wwise pour le type de surface")]
        public AK.Wwise.Switch surfaceMaterial;
        [Tooltip("Le Switch Wwise pour la condition de surface")]
        public AK.Wwise.Switch surfaceCondition;
        [Tooltip("Mots-clés pour l'auto-détection (ex: 'concrete', 'cement', 'beton')")]
        public List<string> keywords = new List<string>();
    }

    [Header("Material Mappings")]
    [Tooltip("Liste des correspondances Material -> Wwise Switches")] // à remplir dans Scriptable
    public List<MaterialMapping> mappings = new List<MaterialMapping>();

    [Header("Auto-Detection")]
    [Tooltip("Activer la détection automatique par nom de material")]
    public bool enableAutoDetection = true;

    public MaterialMapping GetMapping(Material unityMaterial)
    {
        if (unityMaterial == null)
            return null;

        foreach (MaterialMapping WwiseMaterialMapping in mappings) //Pour chaque élément de la liste, 
        {
            if (WwiseMaterialMapping.material == unityMaterial) //Si matériau du Scriptable = matériau détecté,
                return WwiseMaterialMapping; //Renvoie le MaterialMapping correspondant
        }

        // Auto-détection par nom/keywords -> Moins opti mais plus flexible
        if (enableAutoDetection)
        {
            string matName = unityMaterial.name.ToLower(); //Nom du matériau de la scène -> converti en minuscules

            foreach (MaterialMapping WwiseMaterialMapping in mappings) // Pour chaque élément de la liste,
            {
                // Check les mots clés définis dans la liste "keywords"
                // .Any = true si le nom du matériau est contenu dans au moins un mot clé
                // Regarde si la chaîne de caractères d'un des mots clés est contenue dans le nom du matériau (.Contains)
                if (WwiseMaterialMapping.keywords.Any(keyword => matName.Contains(keyword.ToLower())))
                {
                    return WwiseMaterialMapping; // Renvoie le MaterialMapping correspondant
                }
            }
        }
        return null;
    }

#if UNITY_EDITOR //DEBUG à checker
    [ContextMenu("Debug - List All Mappings")]
    private void DebugListMappings()
    {
        Debug.Log($"=== Footstep Material Database: {mappings.Count} mappings ===");
        foreach (var mapping in mappings)
        {
            string matName = mapping.material != null ? mapping.material.name : "AUTO-DETECT";
            string keywords = mapping.keywords.Count > 0 ? $"[{string.Join(", ", mapping.keywords)}]" : "";
            Debug.Log($"  {matName} {keywords} -> Material: {mapping.surfaceMaterial?.Name ?? "None"}, Condition: {mapping.surfaceCondition?.Name ?? "None"}");
        }
    }
#endif
}