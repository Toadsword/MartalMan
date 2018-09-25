using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestPlayerController : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public Transform gun;

    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().color = Color.blue;
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;
        var y = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }

        UpdateGunPosition();

        transform.Translate(x, y, 0);
    }

    private void UpdateGunPosition()
    {
        Vector3 v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        
        Vector2 lastAngle = new Vector2(transform.position.x - v3.x, transform.position.y - v3.y);
        float angle = Mathf.Atan2(lastAngle.y, lastAngle.x) * Mathf.Rad2Deg + 90.0f;
        gun.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    [Command]
    void CmdFire()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            gun.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * 6;

        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }
}
