using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Jogo");
    }
    public void Creditos()
    {
        SceneManager.LoadScene("Creditos");
    }
    public void Artes()
    {
        SceneManager.LoadScene("Cartas");
    }
    public void Artes1()
    {
        SceneManager.LoadScene("Cartas2");
    }
    public void Artes2()
    {
        SceneManager.LoadScene("Cartas3");
    }
    public void MenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Sair() 
    {
        Application.Quit();
    }
}
