using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour {

    public int m_startingWealth = 100;
    private int m_wealth;
    public int Wealth {
        get { return m_wealth; }
        set
        {
            m_wealth = value;
            m_tileSelector.m_displayWealth.text = "$" + m_wealth;
        }
    }
    // Each time after a road is placed, check if the game is won
    // All destinations must be connected somehow
    public List<GameObject> m_destinations;
    public TileSelector m_tileSelector;
    private IsometricTile[] m_tiles;
    public float m_timer;
    static public float enterDelay = 1.0f;
    static float ySeparateDelay = 30.0f;
    static float xSeparateDelay = 50.0f;

    private void Start()
    {
        TileSelector.m_level = this;
        m_tileSelector = FindObjectOfType<TileSelector>();
        m_tiles = GetComponentsInChildren<IsometricTile>();
        foreach (IsometricTile tile in m_tiles)
        {
            tile.DetermineAdjacent();
            if (tile.gameObject.GetComponent<SpriteRenderer>().sprite == m_tileSelector.m_destination)
            {
                m_destinations.Add(tile.gameObject);
            }
        }
        Wealth = m_startingWealth;
        // When the tiles are first created they should elastic interpolate into position!
        foreach (IsometricTile tile in m_tiles)
        {
            tile.gameObject.transform.position = new Vector3(tile.gameObject.transform.position.x, tile.gameObject.transform.position.y * ySeparateDelay + tile.gameObject.transform.position.x * xSeparateDelay, tile.gameObject.transform.position.z);
        }
    }

  

    private void Update()
    {
        if (m_timer < enterDelay)
        {
            m_timer += Time.deltaTime;
            float t = Interpolation.CircularInOut(m_timer/enterDelay);
            foreach (IsometricTile tile in m_tiles)
            {
                tile.gameObject.transform.position = Vector3.LerpUnclamped(tile.gameObject.transform.position, tile.m_spawnPos, t);
            }
        }
    }

    public bool HasWon()
    {
        // Does the first destination have path to every other destination?
        GameObject origin = m_destinations[0];
        for (int i = 1; i < m_destinations.Count; i++)
        {
            GameObject destination = m_destinations[i];
            Stack<GameObject> path = new Stack<GameObject>();
            path.Push(origin);
            bool found = false;
            FindPath(origin, destination, ref path, ref found);
            if (!found)
            {
                return false;
            }
        }
        return true;
    }

    // Use DFS, because it's easy to implement :D
    void FindPath(GameObject origin, GameObject destination, ref Stack<GameObject> path, ref bool found)
    {
        if (found) return;
        foreach (GameObject neighbor in origin.GetComponent<IsometricTile>().m_adjacent)
        {
            if (neighbor != null)
            {
                if (neighbor == destination)
                {
                    path.Push(destination);
                    found = true;
                    return;
                }
                else if (neighbor.GetComponent<SpriteRenderer>().sprite == m_tileSelector.m_destination && !path.Contains(neighbor))
                {
                    path.Push(origin);
                    FindPath(neighbor, destination, ref path, ref found);
                }
            }
        }
        foreach (GameObject road in m_tileSelector.AdjacentRoads(origin.GetComponent<IsometricTile>()))
        {
            if (!path.Contains(road))
            {
                path.Push(origin);
                FindPath(road, destination, ref path, ref found);
            }
        }
        path.Pop();
    }

    public void GrowHouses()
    {
        // If there are any houses next to roads grow them and return
        bool foundGrow = false;
        var tiles = GetComponentsInChildren<IsometricTile>();
        foreach (var tile in tiles)
        {
            Sprite sprite = tile.gameObject.GetComponent<SpriteRenderer>().sprite;
            foreach (GrowData grow in m_tileSelector.m_growHouses)
            {
                if (grow.toReplace == sprite) // This house could potentially grow
                {
                    if (m_tileSelector.AdjacentRoads(tile).Count > 0)
                    {
                        tile.gameObject.GetComponent<SpriteRenderer>().sprite = grow.replaceWith;
                        foundGrow = true;
                    }
                }
            }
        }
        if (foundGrow) return;
        // Otherwise place houses on any occupiable tiles adjacent to a road
        foreach (var tile in tiles)
        {
            if (m_tileSelector.m_roads.Contains(tile.gameObject.GetComponent<SpriteRenderer>().sprite))
            {
                foreach (var neighbor in tile.m_adjacent)
                {
                    if (neighbor != null && m_tileSelector.m_occupiable.Contains(neighbor.GetComponent<SpriteRenderer>().sprite))
                    {
                        neighbor.GetComponent<SpriteRenderer>().sprite = m_tileSelector.m_houses[0];
                    }
                }
            }
        }
    }
}
