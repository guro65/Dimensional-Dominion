using System.Collections.Generic;
using UnityEngine;

public class Caixa : MonoBehaviour
{
    public int precoCompra = 80;
    private Combate combateScript;
    private Mana manaScript;
    private Slots slotsScript;

    void Start()
    {
        combateScript = FindObjectOfType<Combate>();
        manaScript = FindObjectOfType<Mana>();
        slotsScript = FindObjectOfType<Slots>();

        if (combateScript == null || manaScript == null || slotsScript == null)
        {
            Debug.LogError("Um dos scripts (Combate, Mana ou Slots) não foi encontrado na cena.");
            enabled = false;
        }
    }

    void OnMouseDown() // Pode ser substituído por um Button UI ou evento
    {
        if (combateScript != null && manaScript != null && slotsScript != null)
        {
            TentarComprarTokenPlayer();
        }
    }

    public void TentarComprarTokenPlayer()
    {
        if (slotsScript.PlayerHandSlotDisponivel())
        {
            if (manaScript.manaPlayer >= precoCompra)
            {
                manaScript.GastarManaPlayer(precoCompra);
                GerarEColocarTokenNaMao(true);
                precoCompra *= 2; // Duplica o preço após a compra
                Debug.Log($"Player comprou um token. Novo preço: {precoCompra}");
            }
            else
            {
                Debug.Log("Mana insuficiente para comprar o token.");
            }
        }
        else
        {
            Debug.Log("Não há espaço disponível na mão do player para comprar um novo token.");
        }
    }

    void GerarEColocarTokenNaMao(bool paraPlayer)
    {
        if (combateScript != null)
        {
            GameObject tokenPrefab = combateScript.EscolherTokenPorChance();
            if (tokenPrefab != null)
            {
                Transform slotVazio = null;
                List<Transform> slots = paraPlayer ? slotsScript.playerHandSlots : slotsScript.oponenteHandSlots;

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
                    GameObject tokenInstanciado = Instantiate(tokenPrefab, slotVazio.position, Quaternion.identity);
                    tokenInstanciado.transform.SetParent(slotVazio);
                    tokenInstanciado.transform.localPosition = Vector3.zero;
                    tokenInstanciado.tag = paraPlayer ? "Token Player" : "Token Oponente";

                    Token tokenScript = tokenInstanciado.GetComponent<Token>();
                    if (tokenScript != null)
                    {
                        tokenScript.gameObject.tag = paraPlayer ? "Token Player" : "Token Oponente";
                    }
                    // Adiciona o TokenDragDrop (essencial para arrastar da mão)
                    if (tokenInstanciado.GetComponent<TokenDragDrop>() == null)
                    {
                        tokenInstanciado.AddComponent<TokenDragDrop>();
                    }
                }
                else
                {
                    Debug.LogWarning("Não há slots vazios na mão para colocar o novo token. (verificação prévia falhou ou token já foi colocado)");
                }
            }
            else
            {
                Debug.LogError("Falha ao escolher um token para comprar.");
            }
        }
    }

    public void OponenteTentarComprarToken()
    {
        if (slotsScript.OponenteHandSlotDisponivel())
        {
            if (manaScript.manaOponente >= precoCompra)
            {
                manaScript.GastarManaOponente(precoCompra);
                GerarEColocarTokenNaMao(false);
                precoCompra *= 2;
                Debug.Log($"Oponente comprou um token. Novo preço: {precoCompra}");
            }
        }
    }

    public void ResetarPrecoCompra()
    {
        precoCompra = 80;
    }
}