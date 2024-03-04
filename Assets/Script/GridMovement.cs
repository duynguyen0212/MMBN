using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;
using Unity.Mathematics;

public class GridMovement : NetworkBehaviour
{

    public float moveDistance = 0.3f; // Adjust the distance as needed
    public float speed = 5f; // Adjust the speed as needed
    public bool isInAction = false;
    public GameObject shield;
    public Animator anim;
    public GameObject slashingParticle;
    public Buster busterShooting;
    public Saber saberSlashing;
    private int player;
    private GameObject shieldCli, effectCli;
    public AudioSource busterSound, shieldSound, saberSound;

    //Spawn player to the battle stage
    private void Start()
    {
        if (IsHost)
        {
            player = 0;
            Vector3 spawnPoint = new Vector3(13.2f, 0.25f, -3.3f);
            transform.position = spawnPoint;
        }
        else if (IsClient)
        {
            player = 1;
            transform.Rotate(0, 180, 0);
            Vector3 spawnPoint = new Vector3(3.3f, 0.25f, -3.3f);
            transform.position = spawnPoint;
        }
    }
    void Update()
    {
        // check surrounding tile to see if the player can move. They can only move on their side
        if (!IsOwner) return;
        bool canMoveUp = CheckTile(Vector3.forward);
        bool canMoveDown = CheckTile(Vector3.back);
        bool canMoveLeft = CheckTile(Vector3.left);
        bool canMoveRight = CheckTile(Vector3.right);
        //Attack and shield handling
        if (isInAction == false)
        {
            // Check for arrow key presses and move the player
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                if (canMoveRight)
                    Move(Vector3.right);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                if (canMoveLeft)
                    Move(Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (canMoveDown)
                    Move(Vector3.back);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (canMoveUp)
                    Move(Vector3.forward);
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                StartCoroutine(SkillCo("shield"));
                PlaySoundServerRPC("shield");

            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                StartCoroutine(SkillCo("saber"));
                PlaySoundServerRPC("saber");
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                StartCoroutine(SkillCo("buster"));
                PlaySoundServerRPC("buster");
            }

        }
    }

    //Player has 3 skill: saber, shield and buster
    IEnumerator SkillCo(string skillName)
    {
        isInAction = true;
        if (skillName == "saber")
        {
            anim.SetBool(skillName, true);
            saberSlashing.Slash();
            yield return new WaitForSeconds(.7f);
            SpawnEffectServerRpc();
            yield return new WaitForSeconds(.3f);
            DespawnEffectServerRpc();
            anim.SetBool(skillName, false);
        }

        else if (skillName == "buster")
        {
            anim.SetBool(skillName, true);
            busterShooting.Shoot();
            yield return new WaitForSeconds(1f);
            anim.SetBool(skillName, false);
        }

        else if (skillName == "shield")
        {
            SpawnShieldServerRpc();
            anim.SetBool(skillName, true);
            yield return new WaitForSeconds(1f);
            anim.SetBool(skillName, false);
            DespawnShieldServerRpc();
        }

        yield return new WaitForSeconds(.2f);
        isInAction = false;
    }


    //Server will spawn shield whenever player click shield
    [ServerRpc]
    void SpawnShieldServerRpc()
    {
        Vector3 playerPos = new Vector3(transform.position.x, transform.position.y + 2.3f, transform.position.z);
        // Instantiate the weapon prefab for the local player
        shieldCli = Instantiate(shield, playerPos, Quaternion.identity);
        shieldCli.GetComponent<NetworkObject>().Spawn(true);
    }

    [ServerRpc]
    void DespawnShieldServerRpc()
    {
        Destroy(shieldCli.gameObject);
    }

    //Server will spawn slashing effect
    [ServerRpc]
    void SpawnEffectServerRpc()
    {
        Vector3 playerPos;
        if (OwnerClientId == 0)
        {
            playerPos = new Vector3(transform.position.x - 3.5f, transform.position.y + 1.74f, transform.position.z + 1.5f);
        }
        else if (OwnerClientId == 1)
        {
            playerPos = new Vector3(transform.position.x + 3.5f, transform.position.y + 1.74f, transform.position.z - 2.5f);

        }
        else
            playerPos = Vector3.zero;

        effectCli = Instantiate(slashingParticle, playerPos, Quaternion.identity);
        effectCli.transform.Rotate(96, 0, 0);
        effectCli.GetComponent<NetworkObject>().Spawn(true);

    }

    [ServerRpc]
    void DespawnEffectServerRpc()
    {
        Destroy(effectCli);
    }

    void Move(Vector3 direction)
    {
        // Calculate the target position
        Vector3 targetPosition = transform.position + direction * moveDistance;

        // Move the object towards the target position
        transform.position = targetPosition;
    }

    [ServerRpc]
    private void PlaySoundServerRPC(string soundName)
    {
        PlaySoundClientRPC(soundName);
    }

    [ClientRpc]
    private void PlaySoundClientRPC(string soundName)
    {
        if (soundName == "buster")
        {
            StartCoroutine(delaySoundCo(1));
        }
        else if (soundName == "saber")
        {
            StartCoroutine(delaySoundCo(2));
        }
        else
        {
            shieldSound.Play();
        }
    }

    IEnumerator delaySoundCo(int num){
        yield return new WaitForSeconds(.4f);
        if(num == 1){
            busterSound.Play();
        }
        else{
            saberSound.Play();
        } 

    }

    bool CheckTile(Vector3 dir)
    {
        // Raycast to check the tag of the next tile of the character movement
        Ray ray = new Ray(transform.position + dir * moveDistance, Vector3.down);
        RaycastHit hit;
        bool flag = false;

        if (Physics.Raycast(ray, out hit, 1f))
        {
            string tileTag = hit.collider.gameObject.tag;

            if (tileTag == "RedField")
            {
                if (player == 0)
                    flag = false;
                else
                    flag = true;
            }

            else if (tileTag == "BlueField")
                if (player == 0)
                    flag = true;
                else
                    flag = false;

            else
                flag = false;

        }


        return flag;
    }
}
