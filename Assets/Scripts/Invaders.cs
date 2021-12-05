using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Invaders : MonoBehaviour
{

    [Header("Invaders")]
    public Invader[] prefabs = new Invader[5];
    public AnimationCurve speed = new AnimationCurve();
    public Vector3 direction { get; private set; } = Vector3.right;
    public Vector3 initialPosition { get; private set; }
    public System.Action<Invader> killed;

    public int amountKilled { get; private set; }
    public int amountAlive => this.TotalAmount - this.amountKilled;
    public int TotalAmount => this.rows * this.columns;
    public float pocentKilled => (float)this.amountKilled / (float)this.TotalAmount;

    [Header("Grid")]
    public int rows = 5;
    public int columns = 11;

    [Header("Missiles")]
    public Projectile missilePrefab;
    public float missileSpawnRate = 1.0f;

    private void Awake()
    {
        this.initialPosition = this.transform.position;

        for (int row = 0; row < this.rows; row++)
        {
            float width = 2.0f * (this.columns - 1);
            float height = 2.0f * (this.rows - 1);
            Vector2 centering = new Vector2(-width / 2, -height / 2);
            Vector3 rowPosition = new Vector3(centering.x, centering.y + (row * 2.0f), 0.0f);
            for(int col = 0; col < this.columns; col++)
            {
                Invader invader = Instantiate(this.prefabs[row], this.transform);
                invader.killed += OnInvaderKilled;
                Vector3 position = rowPosition;
                position.x += col * 2.0f;
                invader.transform.localPosition = position;
            }
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(MissileAttack), this.missileSpawnRate, this.missileSpawnRate);
    }

    private void Update()
    {
        this.transform.position += direction * this.speed.Evaluate(this.pocentKilled) * Time.deltaTime;

        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        foreach (Transform invader in this.transform)
        {
            if(!invader.gameObject.activeInHierarchy)
            {
                continue;
            }

            if(direction == Vector3.right && invader.position.x >= (rightEdge.x - 1.0f))
            {
                AdvanceRow();
            }
            else if(direction == Vector3.left && invader.position.x <= (leftEdge.x + 1.0))
            {
                AdvanceRow();
            }
        }
    }

    private void MissileAttack()
    {
        foreach (Transform invader in this.transform)
        {
            //dead invader
            if (!invader.gameObject.activeInHierarchy)
            {
                continue;
            }
            //only one laser come from invaders at the time
            if(Random.value < (1.0f/(float)this.amountAlive))
            {
                Instantiate(this.missilePrefab, invader.position, Quaternion.identity);
                break;
            }
        }
    }

    private void AdvanceRow()
    {
        this.direction = new Vector3(-this.direction.x, 0.0f, 0.0f); //flipuje se smìrem jakým se ubírají invaders
        Vector3 position = this.transform.position;
        position.y -= 1.0f;
        this.transform.position = position;
    }

    private void OnInvaderKilled(Invader invader)
    {
        invader.gameObject.SetActive(false);

        this.amountKilled++;
        this.killed(invader);
    }

    public void ResetInvaders()
    {
        this.amountKilled = 0;
        this.direction = Vector3.right;
        this.transform.position = this.initialPosition;
        foreach(Transform invader in this.transform)
        {
            invader.gameObject.SetActive(true);
        }
    }
}
