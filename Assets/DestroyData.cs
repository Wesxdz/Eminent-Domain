using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CreateAssetMenu(fileName = "Data", menuName = "Data/DestroyData", order = 2)]
#endif
public class DestroyData : ScriptableObject
{
    public int cost;
    public Sprite toReplace;
    public Sprite replaceWith;
    public AudioClip sound;
}