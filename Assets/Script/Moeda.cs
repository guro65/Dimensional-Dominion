using UnityEngine;

public class Moeda : MonoBehaviour
{
    public static Moeda instance;
    public int moedas = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool GastarMoedas(int quantidade)
    {
        if (moedas >= quantidade)
        {
            moedas -= quantidade;
            return true;
        }
        return false;
    }

    public void AdicionarMoedas(int quantidade)
    {
        moedas += quantidade;
    }
}