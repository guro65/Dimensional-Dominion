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
            Debug.LogError("Um dos scripts (Combate, Mana ou Slots) n�o foi encontrado na cena.");
            enabled = false;
        }
    }

    void OnMouseDown()
    {
        if (combateScript != null && manaScript != null && slotsScript != null)
        {
            TentarComprarTokenPlayer();
        }
    }

    public void TentarComprarTokenPlayer()
    {
        if (manaScript.manaPlayer >= precoCompra && slotsScript.PlayerSlotDisponivel())
        {
            manaScript.GastarManaPlayer(precoCompra);
            GerarEColocarToken(true);
            precoCompra *= 2; // Duplica o pre�o ap�s a compra
        }
        else if (!slotsScript.PlayerSlotDisponivel())
        {
            Debug.Log("N�o h� espa�o dispon�vel para comprar um novo token.");
        }
        else
        {
            Debug.Log("Mana insuficiente para comprar o token.");
        }
    }

    void GerarEColocarToken(bool paraPlayer)
    {
        if (combateScript != null)
        {
            GameObject tokenPrefab = combateScript.EscolherTokenPorChance();
            if (tokenPrefab != null)
            {
                Transform slotVazio = null;
                List<Transform> slots = paraPlayer ? slotsScript.playerSlots : slotsScript.oponenteSlots;

                foreach (Transform slot in slots)
                {
                    if (slot.childCount == 0)
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
                    tokenInstanciado.AddComponent<BoxCollider2D>();

                    Token tokenScript = tokenInstanciado.GetComponent<Token>();
                    if (tokenScript != null)
                    {
                        tokenScript.gameObject.tag = paraPlayer ? "Token Player" : "Token Oponente";
                    }
                }
                else
                {
                    Debug.Log("N�o h� slots vazios para colocar o novo token.");
                    // Se n�o houver espa�o, a mana n�o � gasta e o pre�o n�o aumenta (j� tratado em TentarComprarTokenPlayer)
                }
            }
            else
            {
                Debug.LogError("Falha ao escolher um token para comprar.");
            }
        }
    }

    // Fun��o para ser chamada pelo script Combate para o oponente comprar tokens
    public void OponenteTentarComprarToken()
    {
        if (manaScript.manaOponente >= precoCompra && slotsScript.OponenteSlotDisponivel())
        {
            manaScript.GastarManaOponente(precoCompra);
            GerarEColocarToken(false);
            precoCompra *= 2; // Duplica o pre�o para o oponente tamb�m
        }
        // Oponente n�o faz nada se n�o tiver mana ou slot dispon�vel
    }

    // Reseta o pre�o de compra (pode ser �til em algum momento do jogo)
    public void ResetarPrecoCompra()
    {
        precoCompra = 80;
    }
}