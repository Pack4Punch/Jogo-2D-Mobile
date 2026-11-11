using UnityEngine;

    public class Player : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 60f;

        [Header("Jump")]
        [SerializeField] private float jumpSpeed = 5f;
        [SerializeField] private float delayBeforeDoubleJump = 0.15f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private Rigidbody2D rb;

        private bool moveLeft;
        private bool moveRight;

        private bool isGrounded;
        private bool canDoubleJump;

        private bool facingRight = true;

        private float targetSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // Se o Animator não for informado no Inspector,
            // tenta encontrar automaticamente neste objeto ou nos filhos.
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void Update()
        {
            UpdateAnimations();
        }

        private void HandleMovement()
        {
            // Define a velocidade desejada
            if (moveLeft)
            {
                targetSpeed = -speed;
            }
            else if (moveRight)
            {
                targetSpeed = speed;
            }
            else
            {
                targetSpeed = 0f;
            }

            // Escolhe aceleração ou desaceleração
            float rate = Mathf.Abs(targetSpeed) > 0.01f
                ? acceleration
                : deceleration;

            // Movimento horizontal suave
            float newVelocityX = Mathf.MoveTowards(
                rb.velocity.x,
                targetSpeed,
                rate * Time.fixedDeltaTime
            );

            rb.velocity = new Vector2(newVelocityX, rb.velocity.y);

            // Só vira o personagem quando realmente estiver se movendo
            if (newVelocityX > 0.1f && !facingRight)
            {
                Flip();
            }
            else if (newVelocityX < -0.1f && facingRight)
            {
                Flip();
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null)
                return;

            // Velocidade horizontal absoluta.
            // Idle = 0
            // Movimento = maior que 0
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));

            // Está no chão?
            animator.SetBool("Grounded", isGrounded);

            // Velocidade vertical.
            // Positivo = subindo
            // Negativo = caindo
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
        }

        #region Mobile Controls

        public void PointerDownLeft()
        {
            moveLeft = true;
        }

        public void PointerUpLeft()
        {
            moveLeft = false;
        }

        public void PointerDownRight()
        {
            moveRight = true;
        }

        public void PointerUpRight()
        {
            moveRight = false;
        }

        #endregion

        #region Jump

        public void JumpButton()
        {
            if (isGrounded)
            {
                PerformJump();

                isGrounded = false;

                // Permite o double jump depois de um pequeno atraso
                CancelInvoke(nameof(EnableDoubleJump));
                Invoke(nameof(EnableDoubleJump), delayBeforeDoubleJump);
            }
            else if (canDoubleJump)
            {
                PerformJump();

                canDoubleJump = false;
            }
        }

        private void PerformJump()
        {
            // Mantém a velocidade horizontal
            // e altera apenas a velocidade vertical.
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);

            // A animação será controlada automaticamente
            // pelo VerticalVelocity + Grounded.
        }

        private void EnableDoubleJump()
        {
            if (!isGrounded)
            {
                canDoubleJump = true;
            }
        }

        #endregion

        #region Collision

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Ground"))
                return;

            // Verifica se realmente estamos pousando sobre o chão
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    canDoubleJump = false;

                    CancelInvoke(nameof(EnableDoubleJump));
                    break;
                }
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                isGrounded = false;
            }
        }

        #endregion

        #region Flip

        private void Flip()
        {
            facingRight = !facingRight;

            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        #endregion
    }
