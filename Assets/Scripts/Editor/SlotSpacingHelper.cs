// Run via: Tools → AgeDefense → Highlight Overlapping Building Slots
// Selects all BuildingSlot GameObjects in the scene so you can see them
// and manually move the ones that are too close together.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class SlotSpacingHelper
{
    const float MIN_DISTANCE = 2.5f;   // world-unit minimum between slot centres

    [MenuItem("Tools/AgeDefense/Highlight Overlapping Building Slots")]
    public static void HighlightOverlapping()
    {
        var slots = Object.FindObjectsByType<BuildingSlot>(FindObjectsSortMode.None);
        if (slots.Length == 0) { Debug.Log("[SlotHelper] No BuildingSlots found in scene."); return; }

        var tooClose = new List<GameObject>();
        for (int i = 0; i < slots.Length; i++)
        for (int j = i + 1; j < slots.Length; j++)
        {
            float d = Vector3.Distance(slots[i].transform.position, slots[j].transform.position);
            if (d < MIN_DISTANCE)
            {
                tooClose.Add(slots[i].gameObject);
                tooClose.Add(slots[j].gameObject);
                Debug.LogWarning($"[SlotHelper] '{slots[i].name}' and '{slots[j].name}' are {d:F2} units apart (min {MIN_DISTANCE}).");
            }
        }

        if (tooClose.Count == 0)
        {
            Debug.Log($"[SlotHelper] All {slots.Length} slots are properly spaced.");
            return;
        }

        // Select the overlapping slots so the user can move them
        Selection.objects = tooClose.ToArray();
        Debug.Log($"[SlotHelper] {tooClose.Count / 2} overlapping pair(s) selected in the hierarchy — move them apart.");
    }

    [MenuItem("Tools/AgeDefense/Select All Building Slots")]
    public static void SelectAll()
    {
        var slots = Object.FindObjectsByType<BuildingSlot>(FindObjectsSortMode.None);
        if (slots.Length == 0) { Debug.Log("[SlotHelper] No BuildingSlots found."); return; }
        var gos = System.Array.ConvertAll(slots, s => (Object)s.gameObject);
        Selection.objects = gos;
        Debug.Log($"[SlotHelper] Selected {slots.Length} BuildingSlots.");
    }
}
#endif
