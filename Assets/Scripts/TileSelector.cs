using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum InteractionType
{
    None,
    CannotAfford,
    Destroy,
    BuildRoad
}

public class TileSelector : MonoBehaviour {

    public LayerMask m_mask;

    public List<Sprite> m_houses;
    public List<Sprite> m_roads;
    public List<Sprite> m_occupiable;
    public List<DestroyData> m_destroyData;
    public Sprite m_destination;
    public Color m_buildColor;
    public Color m_destroyColor;
    public Color m_cannotAffordColor;
    public List<GrowData> m_growHouses;

    public Text m_displayWealth;
    public Text m_displayCost;

    public float m_hoverAmount = 0.2f;

    private GameObject m_tileTarget;
    private Vector3 m_targetOriginalPos;
    private InteractionType m_targetInteraction;
    static public Level m_level;

    public int m_currentLevel = 1;

	// Use this for initialization
	void Start () {
        SceneManager.LoadScene("Level" + m_currentLevel, LoadSceneMode.Additive);
	}
	
	// Update is called once per frame
	void Update () {
        if (m_level == null || m_level.m_timer < Level.enterDelay) return;
        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
        {
            ResetLevel();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RestartGame();
            return;
        }
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, m_mask);
        if (hit.collider != null) // We hit a tile!
        {
            if (m_tileTarget != hit.collider.gameObject)
            {
                if (m_tileTarget)
                {
                    Deselect();
                }
                m_targetOriginalPos = hit.collider.gameObject.transform.position;
                GetComponents<AudioSource>()[0].Play();
            }
            m_tileTarget = hit.collider.gameObject;
            DetermineInteraction();
            Select();
        }

        else if (m_tileTarget != null)
        {
            Deselect();
            m_tileTarget = null;
        }


        if (Input.GetMouseButtonDown(0))
        {
            if (m_tileTarget != null)
            {
                Interact();
            }
        }

	}

    void Select()
    {
        if (m_targetInteraction == InteractionType.CannotAfford)
        {
            m_tileTarget.GetComponent<SpriteRenderer>().color = m_cannotAffordColor;
        }
        else if (m_targetInteraction == InteractionType.Destroy)
        {
            m_tileTarget.GetComponent<SpriteRenderer>().color = m_destroyColor;
        }
        else if (m_targetInteraction == InteractionType.BuildRoad)
        {
            m_tileTarget.GetComponent<SpriteRenderer>().color = m_buildColor;
        }
        m_tileTarget.GetComponent<PolygonCollider2D>().offset = new Vector2(0, .1f - m_hoverAmount);
        m_tileTarget.transform.position = m_targetOriginalPos + new Vector3(0, m_hoverAmount);
    }

    void Deselect()
    {
        m_tileTarget.GetComponent<SpriteRenderer>().color = Color.white;
        m_tileTarget.GetComponent<PolygonCollider2D>().offset = new Vector2(0, .1f);
        m_tileTarget.transform.position = m_targetOriginalPos;
        m_displayCost.text = "";
    }

    void DetermineInteraction()
    {
        m_displayCost.text = "-$10";
        if (m_level.Wealth >= 10)
        {
            m_targetInteraction = InteractionType.BuildRoad;
        }
        else
        {
            m_targetInteraction = InteractionType.CannotAfford;
        }
        if (m_level.m_destinations.Contains(m_tileTarget) ||
            m_roads.Contains(m_tileTarget.GetComponent<SpriteRenderer>().sprite))
        {
            m_targetInteraction = InteractionType.None;
            m_displayCost.text = "";
        }
        if (m_houses.Contains(m_tileTarget.GetComponent<SpriteRenderer>().sprite))
        {
            int removeCost = 0;
            foreach (var data in m_destroyData)
            {
                if (data.toReplace == m_tileTarget.GetComponent<SpriteRenderer>().sprite)
                {
                    removeCost = data.cost;
                    break;
                }
            }
            m_displayCost.text = "-$" + removeCost;
            if (m_level.Wealth >= removeCost)
            {
                m_targetInteraction = InteractionType.Destroy;
            } else
            {
                m_targetInteraction = InteractionType.CannotAfford;
            }
        }
    }

    void Interact()
    {
        if (m_targetInteraction == InteractionType.CannotAfford)
        {
            GetComponents<AudioSource>()[3].Play();
        }
        else if (m_targetInteraction == InteractionType.BuildRoad)
        {
            m_level.Wealth -= 10;
            PlaceRoad();
            m_level.GrowHouses();
            if (m_level.HasWon())
            {
                GetComponents<AudioSource>()[4].Play();
                Invoke("NextLevel", 1.0f);
            }
        }
        else if (m_targetInteraction == InteractionType.Destroy)
        {
            foreach (var data in m_destroyData)
            {
                if (data.toReplace == m_tileTarget.GetComponent<SpriteRenderer>().sprite)
                {
                    m_level.Wealth -= data.cost;
                    m_tileTarget.GetComponent<SpriteRenderer>().sprite = data.replaceWith;
                    var source = GetComponents<AudioSource>()[2];
                    source.clip = data.sound;
                    source.Play();
                }
            }
        }
    }

    void PlaceRoad()
    {
        GetComponents<AudioSource>()[1].Play();
        m_tileTarget.GetComponent<SpriteRenderer>().sprite = m_roads[AdjacentScore(m_tileTarget.GetComponent<IsometricTile>())];
        foreach (var neighbor in m_tileTarget.GetComponent<IsometricTile>().m_adjacent)
        {
            if (neighbor != null)
            {
                if (m_roads.Contains(neighbor.GetComponent<SpriteRenderer>().sprite))
                {
                    neighbor.GetComponent<SpriteRenderer>().sprite = m_roads[AdjacentScore(neighbor.GetComponent<IsometricTile>())];
                }
            }
        }
    }

    int AdjacentScore(IsometricTile tile)
    {
        int adjacent = 0;
        int i = 1;
        foreach (var neighbor in tile.m_adjacent)
        {
            if (neighbor != null)
            {
                if (m_roads.Contains(neighbor.GetComponent<SpriteRenderer>().sprite))
                {
                    adjacent += i;
                }
            }
            i *= 2;
        }
        return adjacent;
    }

    public List<GameObject> AdjacentRoads(IsometricTile tile)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (var neighbor in tile.m_adjacent)
        {
            if (neighbor != null)
            {
                if (m_roads.Contains(neighbor.GetComponent<SpriteRenderer>().sprite))
                {
                    list.Add(neighbor);
                }
            }
        }
        return list;
    }

    public void NextLevel()
    {
        SceneManager.UnloadSceneAsync("Level" + m_currentLevel);
        m_currentLevel++;
        if (m_currentLevel > 8) m_currentLevel = 1;
        SceneManager.LoadScene("Level" + m_currentLevel, LoadSceneMode.Additive);
    }

    public void ResetLevel()
    {
        GetComponents<AudioSource>()[5].Play();
        SceneManager.UnloadSceneAsync("Level" + m_currentLevel);
        SceneManager.LoadScene("Level" + m_currentLevel, LoadSceneMode.Additive);
    }

    public void RestartGame()
    {
        GameObject.Find("Music").GetComponent<AudioSource>().Play();
        SceneManager.UnloadSceneAsync("Level" + m_currentLevel);
        m_currentLevel = 1;
        SceneManager.LoadScene("Level" + m_currentLevel, LoadSceneMode.Additive);
    }
}
