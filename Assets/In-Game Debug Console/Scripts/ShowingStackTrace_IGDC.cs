using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowingStackTrace_IGDC : MonoBehaviour
{
    [SerializeField] private Text _text;

    public void Show(string stackTrace)
    {
        gameObject.SetActive(true);
        _text.text = stackTrace;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}