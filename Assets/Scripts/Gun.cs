using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public string gunName;
    public int ammoCount, maxAmmo;
    public float damage, reloadSpeed;

    public void Reload(int _id)
    {
        Invoke("SetAmmo", reloadSpeed);
        ServerSend.Reload(_id, gunName);
    }

    public void SetAmmo()
    {
        ammoCount = maxAmmo;
    }
}
