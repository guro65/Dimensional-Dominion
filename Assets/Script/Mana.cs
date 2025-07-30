using TMPro; // Certifique-se de que TMPro está sendo usado
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
        if (textoManaPlayer != null) textoManaPlayer.text = $"Mana: {manaPlayer}";
        if (textoManaOponente != null) textoManaOponente.text = $"Mana: {manaOponente}";
    }

    public void AdicionarManaPorRaridade(Token.Raridade raridadeToken, string tagDoVencedor)
    {
        int manaRecebida = 0;

        foreach (var config in manaPorRaridade)
        {
            if (config.raridade == raridadeToken)
            {
                manaRecebida = config.manaRecebida;
                break;
            }
        }

        if (tagDoVencedor == "Token Player")
        {
            manaPlayer += manaRecebida;
        }
        else if (tagDoVencedor == "Token Oponente")
        {
            manaOponente += manaRecebida;
        }

        AtualizarManaUI();
    }
}