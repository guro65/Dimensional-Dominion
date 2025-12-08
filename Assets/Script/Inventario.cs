using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq; // Necessário para o método OrderBy

public class Inventario : MonoBehaviour
{
    [Header("Cartas que o jogador possui")]
    public List<Carta> cartasPossuidas = new List<Carta>();

    [Header("UI do Inventário")]
    public GameObject painelInventario;

    [Header("Slots de Imagem para Exibir as Cartas")]
    // Esta lista será preenchida automaticamente
    public List<Image> slotsDeImagemUI = new List<Image>();

    public Button botaoAbrirInventario;
    public Button botaoFecharInventario;

    private const string SLOT_TAG = "SlotInventario";

    private void Start()
    {
        // --- 1. Busca Automática dos Slots ---
        AutoPreencherSlotsPorTag();

        // --- 2. Configuração de UI ---
        painelInventario.SetActive(false);

        if (botaoAbrirInventario != null)
        {
            botaoAbrirInventario.onClick.AddListener(AbrirInventario);
        }

        if (botaoFecharInventario != null)
        {
            botaoFecharInventario.onClick.AddListener(FecharInventario);
        }
    }

    private void AutoPreencherSlotsPorTag()
    {
        // Encontra todos os GameObjects na cena que possuem a Tag especificada
        GameObject[] slotGOs = GameObject.FindGameObjectsWithTag(SLOT_TAG);

        if (slotGOs.Length == 0)
        {
            Debug.LogWarning("Nenhum objeto com a Tag '" + SLOT_TAG + "' encontrado na cena. Verifique se a Tag foi criada e aplicada corretamente.");
            return;
        }

        // Ordena os slots pelo nome. Isso é CRÍTICO!
        // Se os seus slots se chamam Slot_01, Slot_02, Slot_03, a ordenação
        // garante que as cartas sejam preenchidas na ordem correta.
        var slotsOrdenados = slotGOs.OrderBy(g => g.name);

        // Limpa a lista existente e preenche com os componentes Image
        slotsDeImagemUI.Clear();
        foreach (GameObject slotGO in slotsOrdenados)
        {
            Image img = slotGO.GetComponent<Image>();
            if (img != null)
            {
                slotsDeImagemUI.Add(img);
            }
            else
            {
                Debug.LogWarning("Objeto '" + slotGO.name + "' possui a tag '" + SLOT_TAG + "', mas não possui o componente Image.");
            }
        }

        Debug.Log("Inventário: " + slotsDeImagemUI.Count + " slots encontrados e preenchidos automaticamente.");
    }

    // O restante do script (AdicionarCarta, AbrirInventario, FecharInventario, PopularInventarioUI)
    // permanece o mesmo que a versão anterior, pois a lista 'slotsDeImagemUI' já está preenchida.

    public void AdicionarCarta(Carta carta)
    {
        cartasPossuidas.Add(carta);
        Debug.Log("Carta adicionada ao inventário: " + carta.nomeDaCarta);
    }

    public void AbrirInventario()
    {
        painelInventario.SetActive(true);
        PopularInventarioUI();
    }

    public void FecharInventario()
    {
        painelInventario.SetActive(false);
    }

    private void PopularInventarioUI()
    {
        // 1. Limpa todos os slots de imagem primeiro
        foreach (Image slot in slotsDeImagemUI)
        {
            slot.sprite = null;
            slot.gameObject.SetActive(false);
        }

        // 2. Preenche os slots com as cartas possuídas
        for (int i = 0; i < cartasPossuidas.Count; i++)
        {
            if (i >= slotsDeImagemUI.Count)
            {
                Debug.LogWarning("Mais cartas possuídas (" + cartasPossuidas.Count + ") do que slots de imagem no inventário (" + slotsDeImagemUI.Count + ").");
                break;
            }

            Carta cartaData = cartasPossuidas[i];
            Image slotImage = slotsDeImagemUI[i];

            SpriteRenderer spriteRenderer = cartaData.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                slotImage.sprite = spriteRenderer.sprite;
                slotImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Carta " + cartaData.nomeDaCarta + " não possui SpriteRenderer ou Sprite válido.");
                slotImage.gameObject.SetActive(false);
            }
        }
    }
}