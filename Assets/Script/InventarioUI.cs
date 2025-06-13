using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventarioUI : MonoBehaviour
{
    [Header("Referências da UI")]
    public GameObject painelInventario;           // O painel do inventário (deixe desativado por padrão)
    public Transform conteudo;                    // O container onde os slots de token serão instanciados
    public GameObject prefabSlotToken;            // Prefab do slot (só precisa de um componente Image)

    public Button botaoAbrirInventario;           // Botão para abrir o inventário
    public Button botaoFecharInventario;          // Botão para fechar o inventário

    [Header("Configurações de slot")]
    public Vector2 tamanhoSlot = new Vector2(80, 80); // Tamanho dos ícones no inventário

    void Start()
    {
        painelInventario.SetActive(false);

        if (botaoAbrirInventario != null)
            botaoAbrirInventario.onClick.AddListener(AbrirInventario);

        if (botaoFecharInventario != null)
            botaoFecharInventario.onClick.AddListener(FecharInventario);
    }

    void AbrirInventario()
    {
        painelInventario.SetActive(true);
        AtualizarInventario();
    }

    void FecharInventario()
    {
        painelInventario.SetActive(false);
        // Limpa os slots instanciados
        foreach (Transform filho in conteudo)
        {
            Destroy(filho.gameObject);
        }
    }

    void AtualizarInventario()
    {
        // Limpa antes de adicionar para evitar duplicatas
        foreach (Transform filho in conteudo)
        {
            Destroy(filho.gameObject);
        }

        if (Inventario.instance == null)
        {
            Debug.LogWarning("Inventario.instance não encontrado!");
            return;
        }

        List<Token> tokens = Inventario.instance.ObterTokens();
        foreach (Token token in tokens)
        {
            GameObject slot = Instantiate(prefabSlotToken, conteudo);

            // Pega o sprite do SpriteRenderer do prefab de token
            SpriteRenderer sr = token.GetComponent<SpriteRenderer>();
            if (sr != null && slot.GetComponent<Image>() != null)
            {
                slot.GetComponent<Image>().sprite = sr.sprite;
            }

            // Ajusta o tamanho do slot apenas no inventário
            RectTransform rt = slot.GetComponent<RectTransform>();
            if (rt != null)
                rt.sizeDelta = tamanhoSlot;
        }
    }
}