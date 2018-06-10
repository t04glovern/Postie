using UnityEngine;

public class ShootProjectile : MonoBehaviour {

    public GameObject[] projectilePrefabs;
    public Vector2 firePosition;

    public void Fire(bool facingRight, Vector3 position) 
    {
        firePosition = new Vector2(position.x, position.y);

        if(projectilePrefabs.Length >= 1)
        {
            CreateProjectile(firePosition, facingRight);
        }
    }

    public void CreateProjectile(Vector2 pos, bool facingRight)
    {
        var clone = Instantiate(GetRandomProjectile(), pos, Quaternion.identity) as GameObject;
        clone.transform.localScale = transform.localScale;

        if(facingRight)
            clone.GetComponent<Rigidbody>().AddForce(new Vector3(1000, -600, 0));
        else
            clone.GetComponent<Rigidbody>().AddForce(new Vector3(-1000, -600, 0));
    }

    GameObject GetRandomProjectile()
    {
        return projectilePrefabs[Random.Range(0, (projectilePrefabs.Length))];
    }
}
