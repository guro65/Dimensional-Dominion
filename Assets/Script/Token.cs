using UnityEngine;

public class Token : MonoBehaviour
{
    [Header("Dados do Token")]
    public string nomeDoToken;
    public int dano;
    public int vida;
    public int mana;
    public Raridade raridade;
    public float chanceDeAparicao = 25f; // Porcentagem ajustável para a distribuição

    private Combate combateScript;

    private void Start()
    {
        // Tenta encontrar automaticamente o script Combate na cena
        combateScript = FindObjectOfType<Combate>();
    }

    private void OnMouseDown()
    {
        if (combateScript != null)
        {
            combateScript.SelecionarToken(gameObject);
        }
    }

    public enum Raridade
    {
        Comum,
        Incomum,
        Raro,
        Epico,
        Lendario,
        Mitico
    }
}

