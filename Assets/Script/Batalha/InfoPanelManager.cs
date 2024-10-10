using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour
{
    public GameObject panel;
    public Image image;
    public Text infoText;
    public Button closeButton;
    
    private void Start()
    {
        panel.SetActive(false);
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void ShowPanel(Sprite sprite, string text)
    {
        image.sprite = sprite;
        infoText.text = text;
        panel.SetActive(true);
    }

    private void ClosePanel()
    {
        panel.SetActive(false);
    }
}
