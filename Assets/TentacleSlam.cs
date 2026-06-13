using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TentacleSlam : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float timeBeforeSpawn;
    [SerializeField] private float aimTime;
    [SerializeField] private float timeToAttack;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private bool shouldFollowPlayer = true;
    [SerializeField] private bool isShield = false;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private TriggerNotifier attackTrigger;
    [SerializeField] private List<GameObject> objectsToSpawn;

    [Header("Audio")]
    [SerializeField] private AudioClip spawnAudio;
    [SerializeField] private AudioClip attackAudio;

    [Header("Visuals")]
    [SerializeField] private Renderer tentacleRenderer;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color attackColor;

    public event Action OnSlamComplete;

    private bool isAttacking = false;
    private bool canAct = false;
    private float timer;
    private Transform target;
    private Material mat;

    private void Start()
    {
        if (tentacleRenderer != null)
        {
            mat = tentacleRenderer.material;
        }
    }

    private void OnEnable()
    {
        attackTrigger.TriggerEntered += OnTriggerEntered;
        AudioManager.Instance.PlaySound(spawnAudio);

        timer = 0f;
        isAttacking = false;

        StartCoroutine(Spawn());
    }

    private void OnDisable()
    {
        attackTrigger.TriggerEntered -= OnTriggerEntered;
    }

    private void Update()
    {
        if (!canAct) return;

        if (target == null)
        {
            target = LevelManager.Instance.Player.transform;
        }
        else if (!isAttacking)
        {
            timer += Time.deltaTime;

            if (timer < aimTime)
            {
                Vector3 lookPos = target.position;
                lookPos.y = transform.position.y;

                if (mat != null)
                {
                    mat.color = Color.Lerp(normalColor, attackColor, timer / aimTime);
                }

                if (shouldFollowPlayer) transform.LookAt(lookPos);
            }
            else
            {
                AudioManager.Instance.PlaySound(attackAudio);

                if (mat != null)
                {
                    mat.color = normalColor;
                }

                animator.SetTrigger("Attack");
                timer = 0;
            }
        }
    }


    private IEnumerator Spawn()
    {
        yield return new WaitForSeconds(timeBeforeSpawn);

        foreach (var obj in objectsToSpawn)
        {
            obj.SetActive(true);
        }

        canAct = true;
    }

    private void OnTriggerEntered(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();

        if (target is PlayerHealth)
        {
            target.Damage(attackDamage);
            attackTrigger.DisableHitbox();
        }
    }

    public void ToggleAttacking()
    {
        isAttacking = !isAttacking;

        if (!isAttacking)
        {
            OnSlamComplete?.Invoke();

            if (!isShield)
            {
                Destroy(gameObject);
            }
            else
            {
                canAct = false;
            }
        }
    }

    public void EnableHitbox()
    {
        attackTrigger.EnableHitbox();
    }

    public void DisableHitbox()
    {
        attackTrigger.DisableHitbox();
    }
}