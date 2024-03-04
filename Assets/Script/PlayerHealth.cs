using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Netcode;
using UnityEngine;


public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] RectTransform HealthUI;
    private GameObject gameOverUIRef;
    public Animator anim;
    public AudioSource deathSound;
    private void Start()
    {
        gameOverUIRef = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
    }
    private void OnEnable()
    {
        GetComponent<NetworkPlayerHP>().HP.OnValueChanged += HPChanged;
    }

    private void OnDisable()
    {
        GetComponent<NetworkPlayerHP>().HP.OnValueChanged -= HPChanged;
    }


    private void HPChanged(int previousValue, int newValue)
    {
        HealthUI.transform.localScale = new UnityEngine.Vector3(newValue / 100f, 1, 1);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc()
    {
        TakeDamageClientRPC();
        GetComponent<NetworkPlayerHP>().HP.Value -= 20;
        if (GetComponent<NetworkPlayerHP>().HP.Value <= 0)
        {
            DieAnimClientRPC();
        }
    }

    [ClientRpc]
    public void TakeDamageClientRPC()
    {
        StartCoroutine(TakeDmgCo());

    }

    IEnumerator TakeDmgCo()
    {
        yield return new WaitForSeconds(.5f);
        anim.SetBool("hurt", true);
        yield return new WaitForSeconds(1f);
        anim.SetBool("hurt", false);
    }

    [ClientRpc]
    private void DieAnimClientRPC()
    {
        anim.SetBool("death", true);
        StartCoroutine(DieAnimCo());
    }

    IEnumerator DieAnimCo()
    {
        yield return new WaitForSeconds(3f);
        gameOverUIRef.SetActive(true);
        Time.timeScale = 0f;

    }

    [ClientRpc]
    private void AwakeAnimClientRPC()
    {
        anim.SetBool("death", false);
        deathSound.Play();
    }

    [ServerRpc]
    public void OnRestoreGameStateServerRPC()
    {
        GetComponent<NetworkPlayerHP>().HP.Value = 100;
        AwakeAnimClientRPC();
    }

}
