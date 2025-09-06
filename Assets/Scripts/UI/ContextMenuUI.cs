using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ContextMenuUI : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonParent;

    private List<GameObject> currentButtons = new List<GameObject>();

    private void Awake()
    {
        Hide();
    }

    public void Show(Vector2 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ClearButtons()
    {
        foreach (var button in currentButtons)
        {
            Destroy(button);
        }
        currentButtons.Clear();
    }

    public void AddButton(string label, Action onClickAction)
    {
        GameObject buttonGO = Instantiate(buttonPrefab, buttonParent);
        buttonGO.GetComponentInChildren<TMPro.TMP_Text>().text = label;
        buttonGO.GetComponent<Button>().onClick.AddListener(() => {
            onClickAction();
            Hide();
        });
        currentButtons.Add(buttonGO);
    }
}
