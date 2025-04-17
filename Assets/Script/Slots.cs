using System.Collections.Generic;
using UnityEngine;

public class Slots : MonoBehaviour
{
    public List<Transform> playerSlots;
    public List<Transform> oponenteSlots;

    public bool PlayerSlotDisponivel()
    {
        foreach (Transform slot in playerSlots)
        {
            if (SlotEstaLivre(slot))
                return true;
        }
        return false;
    }

    public bool OponenteSlotDisponivel()
    {
        foreach (Transform slot in oponenteSlots)
        {
            if (SlotEstaLivre(slot))
                return true;
        }
        return false;
    }

    public bool ColocarTokenNoSlot(GameObject token, bool doPlayer)
    {
        List<Transform> lista = doPlayer ? playerSlots : oponenteSlots;

        foreach (Transform slot in lista)
        {
            if (SlotEstaLivre(slot))
            {
                token.transform.SetParent(slot);
                token.transform.localPosition = Vector3.zero;
                return true;
            }
        }

        return false; // Nenhum slot livre
    }

    private bool SlotEstaLivre(Transform slot)
    {
        if (slot.childCount == 0)
            return true;

        // Verifica se o token dentro do slot está morto (ou foi destruído)
        Token tokenNoSlot = slot.GetComponentInChildren<Token>();
        if (tokenNoSlot == null || !tokenNoSlot.estaVivo)
            return true;

        return false;
    }
}
