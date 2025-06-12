using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventarioUI : MonoBehaviour
{
    public Transform conteudoPainel; // O painel onde vão aparecer os tokens (ScrollView/Content)
    public GameObject prefabTokenSlot; // Prefab de um slot/ícone para mostrar o token (com uma Image ou texto)

    void Start()
    {
        AtualizarInventario();
    }

    public void AtualizarInventario()
    {
        foreach (Transform child in conteudoPainel)
            Destroy(child.gameObject);

        List<Token> tokens = Inventario.instance.ObterTokens();
        foreach (Token token in tokens)
        {
            GameObject slot = Instantiate(prefabTokenSlot, conteudoPainel);
            slot.GetComponentInChildren<Text>().text = token.nomeDoToken + " (" + token.raridade + ")";
            // Se quiser mostrar imagem, arraste o SpriteRenderer do token etc
        }
    }
}