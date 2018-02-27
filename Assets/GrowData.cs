using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CreateAssetMenu(fileName = "Data", menuName = "Data/GrowData", order = 1)]
#endif
public class GrowData : ScriptableObject
{
    public Sprite toReplace;
    public Sprite replaceWith;
}
