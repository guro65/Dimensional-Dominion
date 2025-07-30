using UnityEngine;
using UnityEngine.EventSystems; // Necessário para detecção de UI

public class TokenDragDrop : MonoBehaviour
{
    private Vector3 offset;
    private Transform originalParent;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private Slots slotsScript;
    private Combate combateScript;
    private Mana manaScript;
    private Token tokenScript;
    private SpriteRenderer spriteRenderer;
    private TurnManager turnManager;

    public float snapDistance = 0.5f; // Distância para o token "grudar" no slot
    private Transform currentHoveredSlot = null; // Slot que está sendo sobrevoado
    private bool isDefeated = false; // Flag para impedir drag/drop após a derrota

    void Start()
    {
        slotsScript = FindObjectOfType<Slots>();
        combateScript = FindObjectOfType<Combate>();
        manaScript = FindObjectOfType<Mana>();
        tokenScript = GetComponent<Token>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        turnManager = FindObjectOfType<TurnManager>();

        if (slotsScript == null || combateScript == null || manaScript == null || tokenScript == null || spriteRenderer == null || turnManager == null)
        {
            Debug.LogError("Um script essencial não foi encontrado para TokenDragDrop em " + name + ". Verifique se todos os managers estão na cena.");
            enabled = false;
            return;
        }
        
        // Garante que a ordem de renderização inicial é 1
        spriteRenderer.sortingOrder = 1;

        originalParent = transform.parent;
        originalPosition = transform.localPosition;
    }

    public void SetDefeated()
    {
        isDefeated = true;
        // Opcional: desativar o collider para não interagir mais
        if (GetComponent<BoxCollider2D>() != null)
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    void OnMouseDown()
    {
        if (isDefeated) return;
        if (CompareTag("Token Oponente") || turnManager.turnoAtual != TurnManager.Turno.Player) return; // Só player pode arrastar ou selecionar no seu turno

        // Se o token está no tabuleiro, não inicia o drag no OnMouseDown, apenas prepara para uma possível seleção
        if (tokenScript.PosicaoNoTab != Token.PosicaoTabuleiro.NaoNoTabuleiro)
        {
            // Não inicia isDragging aqui se for para seleção. O OnMouseUp tratará o clique.
            return;
        }

        // Se o token está na mão, inicia o drag
        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        originalParent = transform.parent; // Salva o pai original (slot da mão)
        originalPosition = transform.localPosition; // Salva a posição relativa ao pai

        // Tira o token do seu pai temporariamente para arrastar livremente
        transform.SetParent(null);
        spriteRenderer.sortingOrder = 10; // Coloca o token arrastando na frente de outros
        combateScript.FecharDetalhes(); // Fecha qualquer UI aberta ao iniciar o drag
    }

    void OnMouseDrag()
    {
        if (!isDragging) return; // Só permite drag se isDragging foi setado em OnMouseDown (ou seja, veio da mão)

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);

        currentHoveredSlot = null;
        foreach (Transform slot in slotsScript.playerBoardSlots) // Usa a lista geral de slots do tabuleiro
        {
            if (slotsScript.SlotEstaLivre(slot))
            {
                if (Vector3.Distance(transform.position, slot.position) < snapDistance)
                {
                    currentHoveredSlot = slot;
                    break;
                }
            }
        }
    }

    void OnMouseUp()
    {
        if (isDefeated) return;

        if (isDragging) // Se estava arrastando (veio da mão)
        {
            isDragging = false;
            spriteRenderer.sortingOrder = 1; // Volta a ordem de renderização normal (1)

            if (currentHoveredSlot != null && CompareTag("Token Player"))
            {
                // Tenta jogar a carta no slot
                if (manaScript.GastarManaPlayer(tokenScript.manaCusto))
                {
                    transform.SetParent(currentHoveredSlot);
                    transform.localPosition = Vector3.zero;
                    // Atualiza a posição do token no script Token com base no slot
                    tokenScript.PosicaoNoTab = slotsScript.GetPosicaoNoTabuleiro(currentHoveredSlot, true); 
                    Debug.Log($"Token {tokenScript.nomeDoToken} jogado para o tabuleiro na posição: {tokenScript.PosicaoNoTab}");
                    turnManager.AdicionarTokenJogado(gameObject); // Avisa o TurnManager que um token foi jogado
                }
                else
                {
                    Debug.Log("Mana insuficiente para jogar este token.");
                    RetornarParaMao();
                }
            }
            else
            {
                RetornarParaMao();
            }
        }
        else // Se não estava arrastando, significa que foi um clique simples
        {
            // Abre o painel de detalhes SOMENTE se o token estiver no tabuleiro
            if (tokenScript.PosicaoNoTab != Token.PosicaoTabuleiro.NaoNoTabuleiro)
            {
                combateScript.SelecionarTokenParaUI(gameObject);
            } else {
                // Se o token está na mão e não houve drag, não faz nada
                Debug.Log($"Clicou em {tokenScript.nomeDoToken} na mão. Nenhuma ação de UI para tokens na mão.");
            }
        }

        currentHoveredSlot = null; // Reseta o slot sobrevoado
        // combateScript.FecharDetalhes(); // Pode ser fechado por SelecionarTokenParaUI ou se retornar para mão
    }

    void RetornarParaMao()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.localPosition = originalPosition;
            tokenScript.PosicaoNoTab = Token.PosicaoTabuleiro.NaoNoTabuleiro; // Garante que não está no tabuleiro
            spriteRenderer.sortingOrder = 1; // Garante ordem 1 ao retornar para a mão
            Debug.Log("Token retornou para a mão.");
        }
        else
        {
            // Se originalParent é nulo (pode acontecer se o token foi gerado sem um pai, ou por algum erro)
            Debug.LogError("Original parent is null, token cannot return to hand. Destroying token.");
            Destroy(gameObject); // Ou outra lógica de tratamento de erro
        }
    }
}