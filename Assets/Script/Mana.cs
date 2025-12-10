using TMPro; // Certifique-se de que TMPro está sendo usado
using UnityEngine;

public class Mana : MonoBehaviour
{
    [Header("Mana Inicial")]
    public int manaPlayerInicial = 100;
    public int manaOponenteInicial = 100;
    // NOVO: Mana Divina Inicial
    public int manaDivinaPlayerInicial = 0;
    public int manaDivinaOponenteInicial = 0;


    [Header("Mana Atual")]
    public int manaPlayer;
    public int manaOponente;
    // NOVO: Mana Divina Atual
    public int manaDivinaPlayer;
    public int manaDivinaOponente;


    [Header("UI de Mana")]
    public TextMeshProUGUI textoManaPlayer;
    public TextMeshProUGUI textoManaOponente;
    // NOVO: UI de Mana Divina
    public TextMeshProUGUI textoManaDivinaPlayer;
    public TextMeshProUGUI textoManaDivinaOponente;


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
        // NOVO: Inicializa Mana Divina
        manaDivinaPlayer = manaDivinaPlayerInicial;
        manaDivinaOponente = manaDivinaOponenteInicial;
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

    // NOVO: Adicionar Mana Divina
    public void AdicionarManaDivina(int valor, bool isPlayer)
    {
        if (isPlayer)
        {
            manaDivinaPlayer += valor;
        }
        else
        {
            manaDivinaOponente += valor;
        }
        AtualizarManaUI();
    }

    // NOVO: Gastar Mana Divina
    public bool GastarManaDivina(int valor, bool isPlayer)
    {
        if (isPlayer)
        {
            if (manaDivinaPlayer >= valor)
            {
                manaDivinaPlayer -= valor;
                AtualizarManaUI();
                return true;
            }
        }
        else
        {
            if (manaDivinaOponente >= valor)
            {
                manaDivinaOponente -= valor;
                AtualizarManaUI();
                return true;
            }
        }
        return false;
    }

    private void AtualizarManaUI()
    {
        if (textoManaPlayer != null) textoManaPlayer.text = $"Mana: {manaPlayer}";
        if (textoManaOponente != null) textoManaOponente.text = $"Mana: {manaOponente}";
        // NOVO: Atualiza UI de Mana Divina
        if (textoManaDivinaPlayer != null) textoManaDivinaPlayer.text = $"Mana Divina: {manaDivinaPlayer}";
        if (textoManaDivinaOponente != null) textoManaDivinaOponente.text = $"Mana Divina: {manaDivinaOponente}";
    }

    public void AdicionarManaPorRaridade(Token.Raridade raridadeToken, string tagDoVencedor, float energiaBuffPercent)
    {
        int manaRecebidaBase = 0;

        foreach (var config in manaPorRaridade)
        {
            if (config.raridade == raridadeToken)
            {
                manaRecebidaBase = config.manaRecebida;
                break;
            }
        }

        // Aplica o buff de energia
        int manaRecebidaFinal = Mathf.RoundToInt(manaRecebidaBase * (1 + (energiaBuffPercent / 100f)));

        if (tagDoVencedor == "Token Player")
        {
            manaPlayer += manaRecebidaFinal;
        }
        else if (tagDoVencedor == "Token Oponente")
        {
            manaOponente += manaRecebidaFinal;
        }

        Debug.Log($"Adicionado {manaRecebidaFinal} de mana ({manaRecebidaBase} base + {energiaBuffPercent}% buff) para {tagDoVencedor}.");
        AtualizarManaUI();
    }
}