using UnityEngine;

public class Instantiator : MonoBehaviour {
	
    public Transform prefab;

    bool directionChanged;
    int direction = 1;

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.X) && prefab != null)
        {
            Instantiate(prefab, transform.position + (Vector3.down * 0.4f), Quaternion.identity);
        }

        transform.position += Vector3.left * Time.deltaTime * direction;

        if (!directionChanged && Mathf.Abs(transform.position.x) >= 3)
        {
            direction *= -1;
            directionChanged = true;
        }

        if (Mathf.Abs(transform.position.x) < 3)
        {
            directionChanged = false;
        }
	}
}
