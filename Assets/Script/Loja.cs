using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Necessário para o método FirstOrDefault

public class Loja : MonoBehaviour
{
    [Header("Referência ao Sistema de Moedas")]
    public Moedas moedas;

    [Header("Preços dos Giros")]
    public int preco1Giro = 50;
    public int preco10Giros = 450;

    [Header("Cartas Disponíveis no Pacote (Prefabs)")]
    // É importante que os Prefabs aqui tenham o componente 'Carta'
    public List<GameObject> cartasDoPacote = new List<GameObject>();

    [Header("Porcentagem de Chance por Raridade (%)")]
    [Range(0, 100)] public int chanceComum = 60;
    [Range(0, 100)] public int chanceRaro = 25;
    [Range(0, 100)] public int chanceEpico = 10;
    [Range(0, 100)] public int chanceLendario = 4;
    [Range(0, 100)] public int chanceMitico = 1;

    [Header("UI do Painel de Resultados")]
    public GameObject painelResultados;
    public Image imagemCartaUI;
    public Text textoNomeCarta;
    public Button botaoProximaCarta; // botão dentro do painel
    public Inventario inventario;

    [Header("Botões de Giro")]
    public Button botao1Giro;
    public Button botao10Giros;

    private List<GameObject> cartasParaMostrar = new List<GameObject>();
    private int indiceAtual = 0;

    private void Start()
    {
        // Liga botões de giro
        botao1Giro.onClick.AddListener(ComprarUmGiro);
        botao10Giros.onClick.AddListener(ComprarDezGiros);

        // Botão do painel para avançar cartas
        botaoProximaCarta.onClick.AddListener(ProximaCarta);

        // Começa invisível
        painelResultados.SetActive(false);
    }

    // -----------------------------
    // FUNÇÕES PARA COMPRAR GIROS
    // -----------------------------
    public void ComprarUmGiro()
    {
        if (!moedas.GastarMoedas(preco1Giro)) return;

        // O SortearCartaPorRaridade agora garante que uma carta será retornada (nunca null)
        GameObject carta = SortearCartaPorRaridade();

        // Verifica se o resultado não é null (embora não deva ser após a correção)
        if (carta != null)
        {
            cartasParaMostrar = new List<GameObject> { carta };
            indiceAtual = 0;
            MostrarCarta(indiceAtual);
            painelResultados.SetActive(true);
        }
    }

    public void ComprarDezGiros()
    {
        if (!moedas.GastarMoedas(preco10Giros)) return;

        cartasParaMostrar = new List<GameObject>();

        for (int i = 0; i < 10; i++)
        {
            // O SortearCartaPorRaridade agora garante que uma carta será retornada
            GameObject carta = SortearCartaPorRaridade();

            // Adiciona a carta sorteada (não precisamos mais checar por null aqui)
            cartasParaMostrar.Add(carta);
        }

        indiceAtual = 0;
        MostrarCarta(indiceAtual);
        painelResultados.SetActive(true);
    }

    // -----------------------------
    // SORTEIO DE CARTA POR RARIDADE (CORRIGIDO)
    // -----------------------------
    private GameObject SortearCartaPorRaridade()
    {
        int valor = Random.Range(1, 101);
        Raridade raridadeSorteada;

        // Determina a raridade sorteada com base nas chances
        if (valor <= chanceMitico) raridadeSorteada = Raridade.Mitico;
        else if (valor <= chanceMitico + chanceLendario) raridadeSorteada = Raridade.Lendario;
        else if (valor <= chanceMitico + chanceLendario + chanceEpico) raridadeSorteada = Raridade.Epico;
        else if (valor <= chanceMitico + chanceLendario + chanceEpico + chanceRaro) raridadeSorteada = Raridade.Raro;
        else raridadeSorteada = Raridade.Comum;

        // --- NOVO: Lógica de Garantia de Retorno de Carta ---

        // Converte a raridade sorteada para um valor numérico (Mitico=4, Lendario=3, etc.)
        int nivelRaridade = (int)raridadeSorteada;

        // Loop que tenta sortear a raridade atual e, se falhar, tenta uma raridade inferior
        while (nivelRaridade >= 0)
        {
            Raridade raridadeAtual = (Raridade)nivelRaridade;

            // Filtrar cartas que possuem a raridade atual
            List<GameObject> cartasFiltradas = cartasDoPacote
                .Where(c => c.GetComponent<Carta>()?.raridade == raridadeAtual)
                .ToList();

            if (cartasFiltradas.Count > 0)
            {
                // Encontrou cartas dessa raridade. Escolhe aleatoriamente e retorna.
                if (raridadeAtual != raridadeSorteada)
                {
                    Debug.LogWarning("Fallback: Nenhuma carta da raridade original " + raridadeSorteada +
                                     " encontrada. Retornando carta da raridade " + raridadeAtual + ".");
                }
                return cartasFiltradas[Random.Range(0, cartasFiltradas.Count)];
            }

            // Não encontrou cartas desta raridade, tenta a próxima raridade inferior
            nivelRaridade--;
        }

        // Se o loop terminar (nívelRaridade < 0), significa que a lista cartasDoPacote
        // está completamente vazia ou nenhuma carta tem o componente 'Carta'.
        Debug.LogError("ERRO FATAL: A lista 'cartasDoPacote' está vazia ou os Prefabs não possuem o componente 'Carta'.");
        return null; // Retorna null apenas como último recurso de falha.
    }

    // -----------------------------
    // MOSTRAR CARTA NO PAINEL
    // -----------------------------
    private void MostrarCarta(int indice)
    {
        if (indice >= cartasParaMostrar.Count)
        {
            FecharPainel();
            return;
        }

        GameObject cartaGO = cartasParaMostrar[indice];
        Carta cartaComp = cartaGO.GetComponent<Carta>();

        // Pega sprite direto do Prefab do GameObject
        imagemCartaUI.sprite = cartaGO.GetComponent<SpriteRenderer>().sprite;
        textoNomeCarta.text = cartaComp.nomeDaCarta;

        // Adiciona ao inventário
        inventario.AdicionarCarta(cartaComp);
    }

    // -----------------------------
    // AVANÇAR CARTA NO PAINEL
    // -----------------------------
    public void ProximaCarta()
    {
        indiceAtual++;
        MostrarCarta(indiceAtual);
    }

    // -----------------------------
    // FECHAR O PAINEL
    // -----------------------------
    private void FecharPainel()
    {
        painelResultados.SetActive(false);
        cartasParaMostrar.Clear();
        indiceAtual = 0;
    }
}