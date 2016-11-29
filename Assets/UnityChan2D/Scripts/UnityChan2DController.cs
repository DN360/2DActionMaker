using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class UnityChan2DController : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float jumpPower = 1000f;
    public Vector2 backwardForce = new Vector2(-4.5f, 5.4f);

    public LayerMask whatIsGround;

    private Animator m_animator;
    private BoxCollider2D m_boxcollier2D;
    public Rigidbody2D m_rigidbody2D;
	private bool m_isGround;
	public bool m_isBoost;
    private const float m_centerY = 1.5f;

	private float ang = 0.05f; 

    public State m_state = State.Normal;

    void Reset()
    {
        Awake();

        // UnityChan2DController
        maxSpeed = 10f;
        jumpPower = 1000;
        backwardForce = new Vector2(-4.5f, 5.4f);
        whatIsGround = 1 << LayerMask.NameToLayer("Ground");

        // Transform
        transform.localScale = new Vector3(1, 1, 1);

        // Rigidbody2D
        m_rigidbody2D.gravityScale = 3.5f;
		m_rigidbody2D.fixedAngle = true;
		m_rigidbody2D.velocity = Vector2.zero;
		ang = m_rigidbody2D.angularDrag;

        // BoxCollider2D
        m_boxcollier2D.size = new Vector2(1, 2.5f);
        m_boxcollier2D.offset = new Vector2(0, -0.25f);

        // Animator
        m_animator.applyRootMotion = false;
    }

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_boxcollier2D = GetComponent<BoxCollider2D>();
        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
		if (m_state != State.Damaged && !AutoGroundGenerator.ActionEnd) {
			float x = Input.GetAxis ("Horizontal");
			bool jump = Input.GetButtonDown ("Jump");
			Move (x, jump);
		} else if (AutoGroundGenerator.ActionEnd) {
			Move(0, false);
		}
    }

    void Move(float move, bool jump)
    {
        if (Mathf.Abs(move) > 0)
        {
            Quaternion rot = transform.rotation;
            transform.rotation = Quaternion.Euler(rot.x, Mathf.Sign(move) == 1 ? 0 : 180, rot.z);
        }

		if (Mathf.Abs(m_rigidbody2D.velocity.x) < maxSpeed && Mathf.Abs(m_rigidbody2D.velocity.y) <= 0f) {
			m_isBoost = false;
		}

		if (!m_isBoost) {
			if (Mathf.Abs(m_rigidbody2D.velocity.x) > maxSpeed) {
				if (Mathf.Abs (move) == 0) {
					m_rigidbody2D.velocity = new Vector2 (m_rigidbody2D.velocity.x, m_rigidbody2D.velocity.y);
				} else if (move * m_rigidbody2D.velocity.x < 0) {
					m_rigidbody2D.velocity = new Vector2 (m_rigidbody2D.velocity.x * 0.99f, m_rigidbody2D.velocity.y);
				} else {
					m_rigidbody2D.velocity = new Vector2 (m_rigidbody2D.velocity.x, m_rigidbody2D.velocity.y);
				}
			} else {
				m_rigidbody2D.velocity = new Vector2 (move * maxSpeed, m_rigidbody2D.velocity.y);
			}
			m_animator.SetFloat ("Horizontal", m_rigidbody2D.velocity.x);
			m_animator.SetFloat ("Vertical", m_rigidbody2D.velocity.y);
		} else {
			m_animator.SetFloat ("Horizontal", m_rigidbody2D.velocity.x);
			m_animator.SetFloat ("Vertical", m_rigidbody2D.velocity.y);
		}
		m_animator.SetBool ("isGround", m_isGround);
        if (jump && m_isGround)
        {
            m_animator.SetTrigger("Jump");
            SendMessage("Jump", SendMessageOptions.DontRequireReceiver);
            m_rigidbody2D.AddForce(Vector2.up * jumpPower);
        }
    }

	public void Boost(float x, float y) {
		m_rigidbody2D.velocity = Vector2.zero;
		m_rigidbody2D.AddForce(new Vector2(x, y) * jumpPower);
		m_isBoost = true;
	}

    void FixedUpdate()
    {
        Vector2 pos = transform.position;
        Vector2 groundCheck = new Vector2(pos.x, pos.y - (m_centerY * transform.localScale.y));
        Vector2 groundArea = new Vector2(m_boxcollier2D.size.x * 0.49f, 0.05f);

        m_isGround = Physics2D.OverlapArea(groundCheck + groundArea, groundCheck - groundArea, whatIsGround);
		if (m_isGround) {
			if (m_rigidbody2D.velocity.x > maxSpeed) {
				m_rigidbody2D.velocity = new Vector2 (m_rigidbody2D.velocity.x, m_rigidbody2D.velocity.y);
			}
			m_isBoost = false;
		}
        m_animator.SetBool("isGround", m_isGround);

	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "DamageObject" && m_state == State.Normal)
		{
			m_state = State.Damaged;
			StartCoroutine(INTERNAL_OnDamage());
		}
	}

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "DamageObject" && m_state == State.Normal)
        {
            m_state = State.Damaged;
            StartCoroutine(INTERNAL_OnDamage());
        }
    }

    IEnumerator INTERNAL_OnDamage()
    {
        m_animator.Play(m_isGround ? "Damage" : "AirDamage");
        m_animator.Play("Idle");

        SendMessage("OnDamage", SendMessageOptions.DontRequireReceiver);

        m_rigidbody2D.velocity = new Vector2(transform.right.x * backwardForce.x, transform.up.y * backwardForce.y);

        yield return new WaitForSeconds(.2f);

        while (m_isGround == false)
        {
            yield return new WaitForFixedUpdate();
        }
        m_animator.SetTrigger("Invincible Mode");
        m_state = State.Invincible;
    }

    void OnFinishedInvincibleMode()
    {
        m_state = State.Normal;
    }

    public enum State
    {
        Normal,
        Damaged,
        Invincible,
    }
}
