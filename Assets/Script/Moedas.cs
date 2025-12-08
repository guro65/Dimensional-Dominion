using UnityEngine;

public class Moedas : MonoBehaviour
{
    [Header("Quantidade de Moedas do Jogador")]
    public int quantidadeDeMoedas = 0;

    // Função para adicionar moedas
    public void AdicionarMoedas(int valor)
    {
        quantidadeDeMoedas += valor;
        Debug.Log("Moedas adicionadas: " + valor + " | Total: " + quantidadeDeMoedas);
    }

    // Função para gastar moedas
    public bool GastarMoedas(int valor)
    {
        if (quantidadeDeMoedas >= valor)
        {
            quantidadeDeMoedas -= valor;
            Debug.Log("Moedas gastas: " + valor + " | Total: " + quantidadeDeMoedas);
            return true;
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
            return false;
        }
    }
}
