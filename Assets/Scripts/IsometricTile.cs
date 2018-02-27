using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricTile : MonoBehaviour {

    private static List<Vector2> considerAdjacent = new List<Vector2> { new Vector2(-.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, -.5f), new Vector2(-.5f, -.5f) };
    public GameObject[] m_adjacent;
    static float m_tileWidth = 1.0f;
    static float m_tileHeight = 0.5f;
    public Vector3 m_spawnPos;

    private void Awake()
    {
        m_spawnPos = transform.position;
        m_adjacent = new GameObject[4];
    }

    private void OnDrawGizmos()
    {
        foreach (var offset in considerAdjacent)
        {
            Vector3 centerPos = transform.position;
            centerPos.y = centerPos.y + .70f;
            Vector3 drawPos = centerPos + new Vector3(offset.x * m_tileWidth, offset.y * m_tileHeight, 0);
            Gizmos.DrawLine(centerPos, drawPos);
        }
    }

    public void DetermineAdjacent()
    {
        for (int i = 0; i < considerAdjacent.Count; i++)
        {
            Vector3 offset = considerAdjacent[i];
            Vector3 centerPos = transform.position;
            centerPos.y = centerPos.y + .70f;
            RaycastHit2D hit = Physics2D.Raycast(centerPos + new Vector3(offset.x * m_tileWidth, offset.y * m_tileHeight, 0), Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.GetComponent<IsometricTile>() != null)
                {
                    m_adjacent[i] = (hit.collider.gameObject);
                }
            }
        }
    }
}
