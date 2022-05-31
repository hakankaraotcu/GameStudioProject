using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{

    private static TextManager textManager;

    private TextManager()
    {
        if (textManager == null)
        {
            textManager = this;
        }
    }

    public static TextManager GetInstance()
    {
        return textManager;
    }

    [SerializeField] private GameObject textPrefab;

    public void CreateText(Vector2 position, string text)
    {
        Text sct = Instantiate(textPrefab, transform).GetComponent<Text>();
        sct.transform.position = position;
    }
}
