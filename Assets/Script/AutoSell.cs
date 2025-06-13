using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AutoSell : MonoBehaviour
{
    public static AutoSell instance;

    [Header("Referência da UI")]
    public GameObject painelAutoSell; // O painel principal do AutoSell
    public Button[] botoesRaridade;   // Seis botões: 0=Comum, 1=Incomum, ... 5=Mitico
    public Color corSelecionado = Color.blue;
    public Color corNormal = Color.white;

    // Armazena a seleção para cada raridade
    private bool[] autoSellRaridade = new bool[6];

    // Valores de venda automática para cada raridade
    private Dictionary<Token.Raridade, int> valoresVenda = new Dictionary<Token.Raridade, int>
    {
        { Token.Raridade.Comum, 5 },
        { Token.Raridade.Incomum, 10 },
        { Token.Raridade.Raro, 20 },
        { Token.Raridade.Epico, 45 },
        { Token.Raridade.Lendario, 75 },
        { Token.Raridade.Mitico, 100 }
    };

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        painelAutoSell.SetActive(false);

        for (int i = 0; i < botoesRaridade.Length; i++)
        {
            int idx = i; // closure
            botoesRaridade[i].onClick.AddListener(() => AlternarAutoSell(idx));
            AtualizarCorBotao(idx);
        }
    }

    public void AbrirPainel()
    {
        painelAutoSell.SetActive(true);
    }

    public void FecharPainel()
    {
        painelAutoSell.SetActive(false);
    }

    void AlternarAutoSell(int idx)
    {
        autoSellRaridade[idx] = !autoSellRaridade[idx];
        AtualizarCorBotao(idx);
    }

    void AtualizarCorBotao(int idx)
    {
        var colors = botoesRaridade[idx].colors;
        colors.normalColor = autoSellRaridade[idx] ? corSelecionado : corNormal;
        colors.selectedColor = colors.normalColor;
        botoesRaridade[idx].colors = colors;
    }

    public bool EstaMarcadoParaAutoSell(Token.Raridade raridade)
    {
        return autoSellRaridade[(int)raridade];
    }

    public int ValorVenda(Token.Raridade raridade)
    {
        return valoresVenda.ContainsKey(raridade) ? valoresVenda[raridade] : 0;
    }
}