using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunCtrl : MonoBehaviour
{
    private int maxBullet = 3;  //최대 총알 수 
    private int nowBullet;  //현재 총알 수
    //public GameObject bulletPrefab;  //총알 프리팹
    public Image[] bulletImage;  //총알 개수 UI 이미지

    public float range = 100f;
    public float reloadTime = 3f; // 재장전 시간
    private bool isReloading = false;

    public Camera playerCam;

    void Start()
    {
        nowBullet = maxBullet;
    }

    void Update()
    {
        if (isReloading)
            return;

        if (nowBullet <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        nowBullet -= 1;

        RaycastHit hit;
        if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
        } 
        
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("재장전 중...");

        while(nowBullet < maxBullet)
        {
            yield return new WaitForSeconds(reloadTime / maxBullet);
            nowBullet++;
        }
        isReloading = false;
    }

    public void UpdateBulletIcon(/*int nowBullet*/)
    {
        for (int index = 0; index < 3; index++)
        {
            bulletImage[index].color = new Color(1, 1, 1, 0);
        }

        for (int index = 0; index < nowBullet; index++)
        {
            bulletImage[index].color = new Color(1, 1, 1, 1);
        }
    }

}
