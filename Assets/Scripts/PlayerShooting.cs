using StarterAssets;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public enum playerState
{
    Normal,
    Aiming,
    Shooting,
    Dead
}
public class PlayerShooting : NetworkBehaviour
{

    [SerializeField] UIManager uiManager;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private AudioClip _spawnClip;
    [SerializeField] private float _projectileSpeed = 700;
    [SerializeField] private float _cooldown = 0.5f;
    [SerializeField] private Transform _spawner;
    ThirdPersonController _thirdPersonController;
    [Header("¡È√Ù∂»")]
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] Transform aimingTarget;


    private float _lastFired = float.MinValue;
    [SerializeField] private bool _fired;

    [SerializeField] NetworkObjectPool m_ObjectPool;
    StarterAssetsInputs starterAssetsInputs;
    [SerializeField] LayerMask checkLayer;
    [SerializeField] LayerMask ownerLayer;

    [SerializeField] AudioClip reloadVoiceClip;
    [Range(0, 1)] public float reloadVoiceAudioVolume = 0.5f;

    [SerializeField] RigBuilder rig;
    public NetworkVariable<playerState> currenState = new NetworkVariable<playerState>(playerState.Normal, NetworkVariableReadPermission.Everyone);
    Animator anim;

    [Header("Weapon Detail")]
    [SerializeField] Weapon weapon;
    public int maxAmmo;
    public int currentAmmoCarried;
    public int maxAmmoInMag;
    public int currentAmmoInMag;
    public bool isReloading;

    private void Awake()
    {
        m_ObjectPool = FindObjectOfType<NetworkObjectPool>();
        _thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        anim = GetComponent<Animator>();
        uiManager = FindObjectOfType<UIManager>();
        currenState.Value = playerState.Normal;
    }

    private void OnEnable()
    {
        isReloading = false;
        InitWeapon();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.layer = 9;
        }
    }

    private void Update()
    {
        if (currenState.Value == playerState.Aiming || currenState.Value == playerState.Shooting)
        {
            rig.enabled = true;
        }
        else
        {
            rig.enabled = false;
        }

        if (!IsOwner || currenState.Value == playerState.Dead) return;

        Vector3 targetPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, checkLayer))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = ray.GetPoint(75f);
        }

        aimingTarget.position = targetPosition;
        if (starterAssetsInputs.reload)
        {
            Reload();
        }
        if (starterAssetsInputs.aim)
        {
            _thirdPersonController.aimVirtualCamera.gameObject.SetActive(true);
            _thirdPersonController.SetSensitivity(aimSensitivity);
            _thirdPersonController.SetRotateOnMove(false);
            //anim.SetLayerWeight(1, Mathf.Lerp(anim.GetLayerWeight(1),1f,Time.deltaTime*10f));
            Vector3 worldAimTarget = targetPosition;
            worldAimTarget += anim.rootRotation.eulerAngles;
            worldAimTarget.y = transform.position.y;
            //worldAimTarget.x += 40;
            Vector3 aimDirection = worldAimTarget - transform.position;


            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            //rig.enabled = true;
        }
        else
        {
            _thirdPersonController.aimVirtualCamera.gameObject.SetActive(false);
            _thirdPersonController.SetSensitivity(normalSensitivity);
            _thirdPersonController.SetRotateOnMove(true);


            //anim.SetLayerWeight(1, Mathf.Lerp(anim.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            //rig.enabled = false;
        }
        if (starterAssetsInputs.shoot && _lastFired + _cooldown < Time.time&&!isReloading)
        {

            _lastFired = Time.time;
            var dir = targetPosition - _spawner.position;
            if (currentAmmoInMag > 0)
            {
                currentAmmoInMag--;
                SpawnBulletServerRpc(dir);

                uiManager?.UpdateAmmoStatus(currentAmmoInMag, currentAmmoCarried);
            }
            else
            {
                Reload();
            }


        }

        UpdateState();
    }


    private void Reload()
    {
        if (isReloading == false)//MoreJudge
        {
            rig.enabled = false;
            UpdateStateServerRpc(playerState.Normal);
            anim.CrossFade("Reload", 0.1f);

            isReloading = true;
        }
    }

    void UpdateState()
    {
        if (isReloading)
            return;
        if (starterAssetsInputs.shoot)
        {
            if (currenState.Value != playerState.Shooting)
            {
                anim.CrossFade("Shooting", 0.1f);
                UpdateStateServerRpc(playerState.Shooting);
            }
        }
        else if (starterAssetsInputs.aim)
        {
            if (currenState.Value != playerState.Aiming && currenState.Value != playerState.Dead && currenState.Value != playerState.Shooting)
            {
                anim.CrossFade("Aiming", 0.1f);
                //currenState.Value = playerState.Aiming;
                UpdateStateServerRpc(playerState.Aiming);

            }

            else if (currenState.Value == playerState.Dead)
            {

            }
        }
        else if (currenState.Value != playerState.Normal)
        {
            UpdateStateServerRpc(playerState.Normal);
            anim.CrossFade("Empty", 0.1f);
        }

    }
    [ServerRpc(RequireOwnership = false)]
    private void UpdateStateServerRpc(playerState newState)
    {
        //var clientWithDamaged = NetworkManager.Singleton.clien
        currenState.Value = newState;
    }

    public void OnReloading()
    {
        AudioSource.PlayClipAtPoint(reloadVoiceClip, transform.TransformPoint(_spawner.position), reloadVoiceAudioVolume);
    }

    public void ReloadingCompelete()
    {
        isReloading = false;
        if (starterAssetsInputs.aim && !starterAssetsInputs.shoot)
        {
            UpdateStateServerRpc(playerState.Aiming);
            anim.CrossFade("Aiming", 0.1f);
        }
        else if (starterAssetsInputs.shoot)
        {
            UpdateStateServerRpc(playerState.Shooting);
            anim.CrossFade("Shooting", 0.1f);
        }
        else
        {
            UpdateStateServerRpc(playerState.Normal);
            anim.CrossFade("Empty", 0.1f);
        }
        if (currentAmmoCarried >= maxAmmoInMag)
        {
            currentAmmoCarried = currentAmmoCarried - (maxAmmoInMag - currentAmmoInMag);
            currentAmmoInMag = maxAmmoInMag;

        }
        else
        {
            currentAmmoInMag = currentAmmoCarried;
            currentAmmoCarried = 0;
        }
        uiManager?.UpdateAmmoStatus(currentAmmoInMag, currentAmmoCarried);

    }

    [ServerRpc]
    private void SpawnBulletServerRpc(Vector3 dir)
    {
        //NetworkObject ballInstance = Instantiate(_projectile, _spawner.position, Quaternion.identity);
        GameObject bullet = m_ObjectPool.GetNetworkObject(_projectile).gameObject;
        bullet.transform.position = _spawner.position;
        bullet.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        bullet.GetComponent<Projectile>().Init(OwnerClientId);


    }



    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (_fired) GUILayout.Label("FIRED LOCALLY");

        GUILayout.EndArea();
    }

    /// <summary>
    /// If you want to test lag locally, go into the "NetworkButtons" script and uncomment the artificial lag
    /// </summary>
    /// <returns></returns>
    private IEnumerator ToggleLagIndicator()
    {
        _fired = true;
        yield return new WaitForSeconds(0.2f);
        _fired = false;
    }

    public void InitWeapon()
    {
        maxAmmo = weapon.maxAmmo;
        currentAmmoCarried = maxAmmo;
        maxAmmoInMag = weapon.ammoInMag;
        currentAmmoInMag = maxAmmoInMag;
        uiManager?.UpdateAmmoStatus(currentAmmoInMag, currentAmmoCarried);
    }

}
