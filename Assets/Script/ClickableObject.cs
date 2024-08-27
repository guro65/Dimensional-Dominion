using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    public Sprite imageToShow;
    public string textToshow;
    public InfoPanelManager infoPanelManager;
    
    private void OnMouseDown()
    {
        if(infoPanelManager != null)
        {
            infoPanelManager.ShowPanel(imageToShow, textToshow);
        }
        
    }
}
