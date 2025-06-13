using UnityEngine;
using UnityEngine.UI;

public class Moeda : MonoBehaviour
{
    public static Moeda instance;

    [Header("Configuração")]
    public int moedas = 0;
    public Text textoMoedas; // Arraste o Text/UI do Canvas para cá

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AtualizarTextoMoedas();
    }

    public bool GastarMoedas(int quantidade)
    {
        if (moedas >= quantidade)
        {
            moedas -= quantidade;
            AtualizarTextoMoedas();
            return true;
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
            return false;
        }
    }

    public void AdicionarMoedas(int quantidade)
    {
        moedas += quantidade;
        AtualizarTextoMoedas();
    }

    void AtualizarTextoMoedas()
    {
        if (textoMoedas != null)
            textoMoedas.text = moedas.ToString();
    }

    // (Opcional) Use para salvar/carregar moedas entre sessões
    // PlayerPrefs pode ser usado para persistência simples:
    public void SalvarMoedas()
    {
        PlayerPrefs.SetInt("Moedas", moedas);
        PlayerPrefs.Save();
    }

    public void CarregarMoedas()
    {
        moedas = PlayerPrefs.GetInt("Moedas", 0);
        AtualizarTextoMoedas();
    }
}