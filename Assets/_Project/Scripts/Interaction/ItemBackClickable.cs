using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBackClickable : MonoBehaviour
{
    [SerializeField] float checkTime = 1.2f;

    void OnMouseDown()
    {
        StartCoroutine(Check());
    }

    IEnumerator Check()
    {
        var p = PlayerAnimState.Current;
        if (p == null) yield break;

        p.StartCheck();
        yield return new WaitForSeconds(checkTime);
        p.StopCheck();
    }
}
