using UnityEngine;
using UnityEngine.EventSystems;

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

    public float snapDistance = 0.5f;
    private Transform currentHoveredSlot = null;
    private bool isDefeated = false;

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
        
        spriteRenderer.sortingOrder = 1;

        originalParent = transform.parent;
        originalPosition = transform.localPosition;
    }

    public void SetDefeated()
    {
        isDefeated = true;
        if (GetComponent<BoxCollider2D>() != null)
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    void OnMouseDown()
    {
        if (isDefeated) return;
        if (CompareTag("Token Oponente") || turnManager.turnoAtual != TurnManager.Turno.Player) return;

        if (tokenScript.PosicaoNoTab != Token.PosicaoTabuleiro.NaoNoTabuleiro)
        {
            return;
        }

        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        originalParent = transform.parent;
        originalPosition = transform.localPosition;

        transform.SetParent(null);
        spriteRenderer.sortingOrder = 10;
        combateScript.FecharDetalhes();
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);

        currentHoveredSlot = null;
        foreach (Transform slot in slotsScript.playerBoardSlots)
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

        if (isDragging)
        {
            isDragging = false;
            spriteRenderer.sortingOrder = 1;

            if (currentHoveredSlot != null && CompareTag("Token Player"))
            {
                if (manaScript.GastarManaPlayer(tokenScript.manaCusto))
                {
                    transform.SetParent(currentHoveredSlot);
                    transform.localPosition = Vector3.zero;
                    tokenScript.PosicaoNoTab = slotsScript.GetPosicaoNoTabuleiro(currentHoveredSlot, true); 
                    Debug.Log($"Token {tokenScript.nomeDoToken} jogado para o tabuleiro na posição: {tokenScript.PosicaoNoTab}");
                    turnManager.AdicionarTokenJogado(gameObject);
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
        else
        {
            if (tokenScript.PosicaoNoTab != Token.PosicaoTabuleiro.NaoNoTabuleiro)
            {
                combateScript.SelecionarTokenParaUI(gameObject);
            } else {
                Debug.Log($"Clicou em {tokenScript.nomeDoToken} na mão. Nenhuma ação de UI para tokens na mão.");
            }
        }

        currentHoveredSlot = null;
    }

    void RetornarParaMao()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.localPosition = originalPosition;
            tokenScript.PosicaoNoTab = Token.PosicaoTabuleiro.NaoNoTabuleiro;
            spriteRenderer.sortingOrder = 1;
            Debug.Log("Token retornou para a mão.");
        }
        else
        {
            Debug.LogError("Original parent is null, token cannot return to hand. Destroying token.");
            Destroy(gameObject);
        }
    }
}