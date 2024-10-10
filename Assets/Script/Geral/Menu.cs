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

    public void Artes3()
    {
        SceneManager.LoadScene("Cartas4");
    }
    public void MenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }
    public void SelecionarBoss()
    {
        SceneManager.LoadScene("Fases");
    }
    public void Sair() 
    {
        Application.Quit();
    }
}
