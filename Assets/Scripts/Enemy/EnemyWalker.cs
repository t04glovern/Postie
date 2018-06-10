using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyWalker : MonoBehaviour {

    Transform target;
    public float walkingSpeed = 2f;
    public string loadLevel = "Menu";

    void Update() 
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        transform.LookAt(target);
        transform.Translate(Vector3.forward * walkingSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider _target)
    {
        if(_target.gameObject.tag == "Player")
        {
            Destroy(_target.gameObject);
            SceneManager.LoadScene(loadLevel);
        }
    }
}
