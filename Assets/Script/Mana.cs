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
}
