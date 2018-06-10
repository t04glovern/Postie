using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int bounces = 2;

    private Rigidbody _body;
    private ScoreManager _scoreManager;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
    }

    void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.transform.position.y < transform.position.y
            && target.gameObject.tag != "Player")
        {
            bounces--;
        }

        if (bounces <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.tag == "Enemy")
        {
            _scoreManager.AddPoint();
            target.GetComponent<Explode>().OnExplode();
            Destroy(gameObject);
        }
    }
}
