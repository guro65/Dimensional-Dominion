using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Necessário para .OrderBy

public class Slots : MonoBehaviour
{
    [Header("Slots da Mão")]
    public List<Transform> playerHandSlots;
    public List<Transform> oponenteHandSlots;

    [Header("Slots do Tabuleiro (Configurados Manualmente)")]
    // Atribua os slots da frente e de trás separadamente no Inspector
    public List<Transform> playerFrontBoardSlots;
    public List<Transform> playerBackBoardSlots;
    public List<Transform> oponenteFrontBoardSlots;
    public List<Transform> oponenteBackBoardSlots;

    // Estas listas serão preenchidas automaticamente no Awake
    [HideInInspector] public List<Transform> playerBoardSlots;
    [HideInInspector] public List<Transform> oponenteBoardSlots;

    void Awake()
    {
        // Concatena as listas de frente e trás para formar as listas gerais de tabuleiro
        playerBoardSlots = new List<Transform>(playerFrontBoardSlots);
        playerBoardSlots.AddRange(playerBackBoardSlots);

        oponenteBoardSlots = new List<Transform>(oponenteFrontBoardSlots);
        oponenteBoardSlots.AddRange(oponenteBackBoardSlots);
    }

    public bool PlayerHandSlotDisponivel()
    {
        foreach (Transform slot in playerHandSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool OponenteHandSlotDisponivel()
    {
        foreach (Transform slot in oponenteHandSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool PlayerBoardSlotDisponivel()
    {
        foreach (Transform slot in playerBoardSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool OponenteBoardSlotDisponivel()
    {
        foreach (Transform slot in oponenteBoardSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool SlotEstaLivre(Transform slot)
    {
        if (slot == null) return false;

        if (slot.childCount == 0) return true;

        Token tokenNoSlot = slot.GetComponentInChildren<Token>();
        return tokenNoSlot == null || !tokenNoSlot.estaVivo;
    }

    // Retorna a posição do token no tabuleiro (Frente/Tras) com base no slot
    public Token.PosicaoTabuleiro GetPosicaoNoTabuleiro(Transform slotTransform, bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerFrontBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Frente;
            if (playerBackBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Tras;
        }
        else // is Oponente
        {
            if (oponenteFrontBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Frente;
            if (oponenteBackBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Tras;
        }
        return Token.PosicaoTabuleiro.NaoNoTabuleiro;
    }

    // Retorna todos os tokens ativos em slots do tabuleiro para um dado jogador
    public List<Token> GetTokensNoTabuleiro(bool isPlayer)
    {
        List<Token> tokensNoTab = new List<Token>();
        List<Transform> slots = isPlayer ? playerBoardSlots : oponenteBoardSlots;

        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
            {
                Token token = slot.GetComponentInChildren<Token>();
                if (token != null && token.estaVivo)
                {
                    tokensNoTab.Add(token);
                }
            }
        }
        return tokensNoTab;
    }

    // Retorna todos os slots vazios do tabuleiro para um dado jogador
    public List<Transform> GetSlotsVaziosNoTabuleiro(bool isPlayer)
    {
        List<Transform> vazios = new List<Transform>();
        List<Transform> slots = isPlayer ? playerBoardSlots : oponenteBoardSlots;

        foreach (Transform slot in slots)
        {
            if (SlotEstaLivre(slot))
            {
                vazios.Add(slot);
            }
        }
        return vazios;
    }

    // Retorna o slot da frente correspondente a um slot de trás, ou vice-versa (na mesma coluna X)
    public Transform GetSlotCorrespondenteNaColuna(Transform currentSlot, bool isPlayer, Token.PosicaoTabuleiro targetPosicao)
    {
        List<Transform> boardSlotsToSearch = isPlayer ? playerBoardSlots : oponenteBoardSlots;

        foreach (Transform slot in boardSlotsToSearch)
        {
            // Verifica se o slot pertence à linha de destino E tem aproximadamente a mesma posição X (mesma coluna)
            if (GetPosicaoNoTabuleiro(slot, isPlayer) == targetPosicao)
            {
                if (Mathf.Abs(slot.position.x - currentSlot.position.x) < 0.1f) // Usa uma pequena margem de erro
                {
                    return slot;
                }
            }
        }
        return null;
    }

    // Funções para IA do oponente
    public Transform GetPrimeiroSlotVazioFrente(bool isPlayer)
    {
        List<Transform> slotsFrente = isPlayer ? playerFrontBoardSlots : oponenteFrontBoardSlots;
        slotsFrente = slotsFrente.OrderBy(s => s.position.x).ToList(); // Ordena por X para achar o mais à esquerda

        foreach (Transform slot in slotsFrente)
        {
            if (SlotEstaLivre(slot))
            {
                return slot;
            }
        }
        return null;
    }

    public Transform GetPrimeiroSlotVazioTras(bool isPlayer)
    {
        List<Transform> slotsTras = isPlayer ? playerBackBoardSlots : oponenteBackBoardSlots;
        slotsTras = slotsTras.OrderBy(s => s.position.x).ToList(); // Ordena por X para achar o mais à esquerda

        foreach (Transform slot in slotsTras)
        {
            if (SlotEstaLivre(slot))
            {
                return slot;
            }
        }
        return null;
    }
}