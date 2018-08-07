using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHandler : NetworkBehaviour
{
    [SerializeField]
    float move_speed = 1;
    [SerializeField]
    float rotate_speed = 10;

    private void Update()
    {
        if (!isLocalPlayer) return;

        transform.position += Input.GetAxis("Vertical") * transform.forward * move_speed * Time.deltaTime;
        transform.eulerAngles += Input.GetAxis("Horizontal") * transform.up * rotate_speed * Time.deltaTime;
    }
}