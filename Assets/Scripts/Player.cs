using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f * 2;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f * 2;
    public float health;
    public float maxHealth = 100f;
    public int teamNum;
    public int selectedWeapon = 0;
    public GameObject primary;
    public GameObject secondary;
    public ParticleSystem blood;
    public ParticleSystem primaryMuzzleFlash;
    public ParticleSystem secondaryMuzzleFlash;

    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        primary.GetComponent<Gun>().ammoCount = primary.GetComponent<Gun>().maxAmmo;
        secondary.GetComponent<Gun>().ammoCount = secondary.GetComponent<Gun>().maxAmmo;

        ServerSend.CurrentAmmo(id, this);

        if (id % 2 == 0)
        {
            if (TeamDeathmatch.numOfTeam1 == Server.MaxPlayers/2)
            {
                teamNum = 1;
                TeamDeathmatch.numOfTeam2++;
            }
            else
            {
                teamNum = 0;
                TeamDeathmatch.numOfTeam1++;
            }
        }
        else
        {
            if (TeamDeathmatch.numOfTeam2 == Server.MaxPlayers / 2)
            {
                teamNum = 0;
                TeamDeathmatch.numOfTeam1++;
            }
            else
            {
                teamNum = 1;
                TeamDeathmatch.numOfTeam2++;
            }
        }

        inputs = new bool[9];
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
        
        //Change to primary
        if (inputs[5])
        {
            if (selectedWeapon == 1)
            {
                selectedWeapon = 0;
                secondary.SetActive(false);
                primary.SetActive(true);

                ServerSend.PlayerActiveWeapon(this);
            }
        }

        //Change to secondary
        if (inputs[6])
        {
            if(selectedWeapon == 0)
            {
                selectedWeapon = 1;
                primary.SetActive(false);
                secondary.SetActive(true);

                ServerSend.PlayerActiveWeapon(this);
            }
        }
        ServerSend.CurrentAmmo(id, this);
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;
        
        _moveDirection.y = yVelocity;

        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit hit, 1000000f))
        {
            Hitbox place = hit.transform.GetComponent<Hitbox>();
            if (place != null)
            {
                Player _hitPlayer = place.player;
                if (_hitPlayer != null)
                {
                    if (_hitPlayer.teamNum == teamNum) { return; }
                    if (primary.activeSelf)
                    {
                        Gun primaryGun = primary.GetComponent<Gun>();
                        CheckHitbox(_hitPlayer, primaryGun, place.place);
                    }
                    if (secondary.activeSelf)
                    {
                        Gun secondaryGun = secondary.GetComponent<Gun>();
                        CheckHitbox(_hitPlayer, secondaryGun, hit.transform.GetComponent<Hitbox>().place);
                    }
                    ServerSend.PlayBlood(new Vector3(hit.point.x, hit.point.y-4, hit.point.z), Quaternion.LookRotation(hit.normal));
                }
            }
        }
        if (primary.activeSelf)
        {
            Gun primaryGun = primary.GetComponent<Gun>();
            primaryGun.ammoCount--;
            if (primaryGun.ammoCount <= 0)
            {
                primaryGun.Reload(id);
            }
        }
        if (secondary.activeSelf)
        {
            Gun secondaryGun = secondary.GetComponent<Gun>();
            secondaryGun.ammoCount--;
            if (secondaryGun.ammoCount <= 0)
            {
                secondaryGun.Reload(id);
            }
        }
        ServerSend.MuzzleFlash(this);
    }
    private void CheckHitbox(Player _hitPlayer, Gun _gun, string _place)
    {
        switch (_place)
        {
            case "Head":
                _hitPlayer.TakeDamage(_gun.damage * 2);
                break;
            case "Body":
                _hitPlayer.TakeDamage(_gun.damage);
                break;
            case "Arm":
                _hitPlayer.TakeDamage(_gun.damage * .8f);
                break;
            case "Leg":
                _hitPlayer.TakeDamage(_gun.damage * .9f);
                break;
            default:
                _hitPlayer.TakeDamage(_gun.damage);
                break;
        }
    }
    public void TakeDamage(float _damage)
    {
        /*
        if (health <= 0f)
        {
            return;
        }*/

        health -= _damage;
        if (health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            if(teamNum == 0)
            {
                transform.position = Constants.team1Spawnpoints[Random.Range(0, 4)];
            }
            else
            {
                transform.position = Constants.team2Spawnpoints[Random.Range(0, 4)];
            }

            ServerSend.PlayerPosition(this);
            TeamDeathmatch.AddScore(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }
}