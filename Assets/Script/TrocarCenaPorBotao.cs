using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrocaCenaPorBotao : MonoBehaviour
{
    [Header("Nome da cena para trocar ao clicar")]
    public string nomeCena;

    void Start()
    {
        // Se o script estiver em um botão, adiciona automaticamente o evento
        Button botao = GetComponent<Button>();
        if (botao != null)
        {
            botao.onClick.AddListener(TrocarCena);
        }
    }

    public void TrocarCena()
    {
        if (!string.IsNullOrEmpty(nomeCena))
        {
            SceneManager.LoadScene(nomeCena);
        }
        else
        {
            Debug.LogWarning("Nome da cena não definido no botão: " + gameObject.name);
        }
    }
}