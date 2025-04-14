using TMPro;
using UnityEngine;

public class Mana : MonoBehaviour
{
    [Header("Mana Inicial")]
    public int manaPlayerInicial = 100;
    public int manaOponenteInicial = 100;

    [Header("Mana Atual")]
    public int manaPlayer;
    public int manaOponente;

    [Header("UI de Mana")]
    public TextMeshProUGUI textoManaPlayer;
    public TextMeshProUGUI textoManaOponente;

    [Header("Configuração de Mana por Raridade")]
    public ManaPorRaridade[] manaPorRaridade;

    // Struct para definir a quantidade de mana por raridade
    [System.Serializable]
    public struct ManaPorRaridade
    {
        public Token.Raridade raridade;
        public int manaRecebida;
    }

    void Start()
    {
        manaPlayer = manaPlayerInicial;
        manaOponente = manaOponenteInicial;
        AtualizarManaUI();
    }

    public bool GastarManaPlayer(int valor)
    {
        if (manaPlayer >= valor)
        {
            manaPlayer -= valor;
            AtualizarManaUI();
            return true;
        }
        return false;
    }

    public bool GastarManaOponente(int valor)
    {
        if (manaOponente >= valor)
        {
            manaOponente -= valor;
            AtualizarManaUI();
            return true;
        }
        return false;
    }

    private void AtualizarManaUI()
    {
        textoManaPlayer.text = $"Mana: {manaPlayer}";
        textoManaOponente.text = $"Mana: {manaOponente}";
    }

    // Função que adiciona mana ao player ou ao oponente com base na raridade do token derrotado
    public void AdicionarManaPorRaridade(Token.Raridade raridadeToken, string tagDoVencedor)
    {
        int manaRecebida = 0;

        // Encontra a quantidade de mana configurada para a raridade do token
        foreach (var config in manaPorRaridade)
        {
            if (config.raridade == raridadeToken)
            {
                manaRecebida = config.manaRecebida;
                break;
            }
        }

        // A quem vencer vai receber a mana de acordo com a raridade
        if (tagDoVencedor == "Token Player") // Se quem venceu for o player, ele recebe mana
        {
            manaPlayer += manaRecebida;
        }
        else if (tagDoVencedor == "Token Oponente") // Se quem venceu for o oponente, ele recebe mana
        {
            manaOponente += manaRecebida;
        }

        AtualizarManaUI(); // Atualiza a UI de mana
    }
}