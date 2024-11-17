using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RubyController : MonoBehaviour
{
    public float speed = 4;
    public InputAction moveAction;
    
    public int maxHealth = 5;
    public float timeInvincible = 2.0f;
    public Transform respawnPosition;
    public ParticleSystem hitParticle;

    public GameObject projectilePrefab;
    public InputAction launchAction;

    public AudioClip hitSound;
    public AudioClip shootingSound;

    public int health
    {
        get { return currentHealth; }
    }
    
    public InputAction dialogAction;
    
    Rigidbody2D rigidbody2d;
    Vector2 currentInput;
    
    int currentHealth;
    float invincibleTimer;
    bool isInvincible;
    
    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);
    
    AudioSource audioSource;
    
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        moveAction.Enable();        
        
        invincibleTimer = -1.0f;
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        launchAction.Enable();
        dialogAction.Enable();

        launchAction.performed += LaunchProjectile;
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }
        
        Vector2 move = moveAction.ReadValue<Vector2>();
        
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        currentInput = move;
        
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        
        if (dialogAction.WasPressedThisFrame())
        {
            var hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, 1 << LayerMask.NameToLayer("NPC"));
            var character = hit.collider?.GetComponent<NonPlayerCharacter>();
            if (character is not null)
            {
                UIHandler.instance.DisplayDialog();
            }
        }
 
    }

    void FixedUpdate()
    {
        var position = rigidbody2d.position;
        
        position = position + currentInput * (speed * Time.deltaTime);
        
        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        { 
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            
            animator.SetTrigger("Hit");
            audioSource.PlayOneShot(hitSound);

            Instantiate(hitParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
        
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        if(currentHealth == 0)
            Respawn();
        
        UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth);
    }
    
    void Respawn()
    {
        ChangeHealth(maxHealth);
        transform.position = respawnPosition.position;
    }
    
    void LaunchProjectile(InputAction.CallbackContext context)
    {
        var projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        var projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);
        
        animator.SetTrigger("Launch");
        audioSource.PlayOneShot(shootingSound);
    }
    
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
