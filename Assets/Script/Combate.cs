using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Certifique-se de que TMPro está sendo usado para TextMeshProUGUI

public class Combate : MonoBehaviour
{
    private Mana manaScript;
    private Slots slotsScript;
    private Caixa caixaScript;
    private TurnManager turnManager;

    [Header("Prefabs de Tokens Disponíveis")]
    public List<GameObject> todosOsTokens;

    [Header("Canvas UI")]
    public GameObject painelDeAcoes; // Para o botão de habilidade especial
    public Button botaoHabilidadeEspecial;
    public GameObject painelDeDetalhes;
    public Button botaoFecharDetalhes; // Agora o único botão no painel de detalhes

    public TextMeshProUGUI textoDano;
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoRaridade;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoManaCusto;
    public Image imagemToken;

    [HideInInspector] public GameObject tokenAtualSelecionado;

    void Start()
    {
        manaScript = FindObjectOfType<Mana>();
        slotsScript = FindObjectOfType<Slots>();
        caixaScript = FindObjectOfType<Caixa>();
        turnManager = FindObjectOfType<TurnManager>();

        if (slotsScript == null || manaScript == null || caixaScript == null || turnManager == null)
        {
            Debug.LogError("Um dos scripts essenciais (Slots, Mana, Caixa ou TurnManager) não foi encontrado na cena.");
            enabled = false;
            return;
        }

        painelDeAcoes.SetActive(false);
        painelDeDetalhes.SetActive(false);

        // Inicializa 8 tokens para cada lado nas "mãos"
        GerarTokensIniciais(8);

        // Listener para o botão de habilidade especial
        botaoHabilidadeEspecial.onClick.AddListener(AtivarHabilidadeEspecial);
        botaoFecharDetalhes.onClick.AddListener(FecharDetalhes); // Certifica-se de que o listener está no lugar
    }

    // Chamado pelo TokenDragDrop para selecionar um token (para detalhes ou habilidade)
    public void SelecionarTokenParaUI(GameObject token)
    {
        tokenAtualSelecionado = token;
        Token tokenScript = tokenAtualSelecionado.GetComponent<Token>();

        // Só mostra painéis se o token estiver no tabuleiro
        if (tokenScript != null && tokenScript.PosicaoNoTab != Token.PosicaoTabuleiro.NaoNoTabuleiro)
        {
            MostrarDetalhesUI(); // Isso ativa o painelDeDetalhes

            // Lógica para mostrar o botão de habilidade especial (apenas para o player no seu turno)
            if (tokenAtualSelecionado.CompareTag("Token Player") && turnManager.turnoAtual == TurnManager.Turno.Player)
            {
                painelDeAcoes.SetActive(true);
                // Habilita o botão apenas se tiver mana suficiente e a habilidade não estiver já ativada
                botaoHabilidadeEspecial.interactable = (manaScript.manaPlayer >= tokenScript.custoManaEspecial && !tokenScript.habilidadeAtivada);
            }
            else
            {
                painelDeAcoes.SetActive(false); // Esconde se não for para habilidade (oponente ou turno errado)
            }
        }
        else
        {
            // Se o token não está no tabuleiro, fecha ambos os painéis
            painelDeDetalhes.SetActive(false);
            painelDeAcoes.SetActive(false);
        }
    }


    void GerarTokensIniciais(int quantidade)
    {
        for (int i = 0; i < quantidade; i++)
        {
            GerarEColocarTokenNaMao(true); // Para o player
            GerarEColocarTokenNaMao(false); // Para o oponente
        }
    }

    void GerarEColocarTokenNaMao(bool paraPlayer)
    {
        List<Transform> slots = paraPlayer ? slotsScript.playerHandSlots : slotsScript.oponenteHandSlots;
        string tagDoDono = paraPlayer ? "Token Player" : "Token Oponente";
        Transform slotVazio = null;

        foreach (Transform slot in slots)
        {
            if (slotsScript.SlotEstaLivre(slot)) // Usa a nova verificação de slot livre
            {
                slotVazio = slot;
                break;
            }
        }

        if (slotVazio != null)
        {
            GameObject tokenPrefab = EscolherTokenPorChance();
            if (tokenPrefab != null)
            {
                GameObject tokenInstanciado = Instantiate(tokenPrefab, slotVazio.position, Quaternion.identity);
                tokenInstanciado.transform.SetParent(slotVazio);
                tokenInstanciado.transform.localPosition = Vector3.zero;
                tokenInstanciado.tag = tagDoDono;

                Token tokenScript = tokenInstanciado.GetComponent<Token>();
                if (tokenScript != null)
                {
                    tokenScript.gameObject.tag = tagDoDono; // Garante a tag no script
                }
                // Adiciona o TokenDragDrop se não tiver (muito importante!)
                if (tokenInstanciado.GetComponent<TokenDragDrop>() == null)
                {
                    tokenInstanciado.AddComponent<TokenDragDrop>();
                }
            }
        } else {
            Debug.LogWarning($"Não há slots vazios na mão do {(paraPlayer ? "Player" : "Oponente")} para gerar token inicial.");
        }
    }

    public GameObject EscolherTokenPorChance()
    {
        float totalChance = 0f;
        foreach (GameObject token in todosOsTokens)
        {
            Token dados = token.GetComponent<Token>();
            totalChance += dados.chanceDeAparicao;
        }

        float randomValue = Random.Range(0f, totalChance);
        float acumulado = 0f;

        foreach (GameObject token in todosOsTokens)
        {
            Token dados = token.GetComponent<Token>();
            acumulado += dados.chanceDeAparicao;
            if (randomValue <= acumulado)
                return token;
        }
        return null; // Caso não encontre nenhum token (improvável se totalChance > 0)
    }

    void MostrarDetalhesUI()
    {
        if (tokenAtualSelecionado != null)
        {
            Token dados = tokenAtualSelecionado.GetComponent<Token>();
            if (dados != null)
            {
                textoNome.text = dados.nomeDoToken;
                textoDano.text = $"Dano: {dados.dano}";
                textoVida.text = $"Vida: {dados.vida}";
                textoManaCusto.text = $"Custo: {dados.manaCusto}"; // Agora é o custo para jogar
                textoRaridade.text = $"Raridade: {dados.raridade}";

                SpriteRenderer spriteRenderer = tokenAtualSelecionado.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    imagemToken.sprite = spriteRenderer.sprite;

                painelDeDetalhes.SetActive(true);
            }
            else
            {
                Debug.LogError("O token selecionado não possui o componente 'Token'.");
                painelDeDetalhes.SetActive(false);
            }
        }
        else
        {
            painelDeDetalhes.SetActive(false);
        }
    }

    public void FecharDetalhes()
    {
        painelDeDetalhes.SetActive(false);
        painelDeAcoes.SetActive(false); // Fecha o painel de ações junto
        tokenAtualSelecionado = null;
    }

    void AtivarHabilidadeEspecial()
    {
        if (tokenAtualSelecionado != null && tokenAtualSelecionado.CompareTag("Token Player"))
        {
            Token tokenScript = tokenAtualSelecionado.GetComponent<Token>();
            if (tokenScript != null)
            {
                if (manaScript.GastarManaPlayer(tokenScript.custoManaEspecial))
                {
                    tokenScript.habilidadeAtivada = true; // Apenas marca a flag
                    Debug.Log($"Habilidade especial de {tokenScript.nomeDoToken} ativada! O dano será aplicado no próximo ataque.");
                    painelDeAcoes.SetActive(false);
                    FecharDetalhes();
                }
                else
                {
                    Debug.Log("Mana insuficiente para usar a habilidade especial.");
                }
            }
        }
    }
}