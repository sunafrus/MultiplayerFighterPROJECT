using UnityEngine;
using System.Collections;

public class PlayerMobility : MonoBehaviour
{
	public float speed;

	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private Quaternion syncStartRotation;
	private Quaternion syncEndRotation;


	void Start ()
	{

	}

	void Update ()
	{
		if (networkView.isMine)
		{
			InputMovement ();
		}
		else
		{
			SyncedMovement ();
		}
	}

	private void InputMovement ()
	{
		var mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		
		Quaternion rot = Quaternion.LookRotation (transform.position - mousePosition, Vector3.forward);
		
		transform.rotation = rot;
		transform.eulerAngles = new Vector3 (0, 0, transform.eulerAngles.z);
		rigidbody2D.angularVelocity = 0;
		
		//Movement toward/away from the mouse
		float verticalInput = Input.GetAxis ("Vertical");
		rigidbody2D.AddForce (gameObject.transform.up * speed * verticalInput);
		
		//Strafing around the mouse
		float horizontalInput = Input.GetAxis ("Horizontal");
		rigidbody2D.AddForce (gameObject.transform.right * speed/2 *  horizontalInput);
	}

	private void SyncedMovement ()
	{
		syncTime += Time.deltaTime;

		rigidbody2D.position = Vector3.Lerp (syncStartPosition, syncEndPosition, syncTime / syncDelay);

		transform.rotation = Quaternion.Lerp (syncStartRotation, syncEndRotation, syncTime / syncDelay);
	}

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		Quaternion syncRotation = Quaternion.identity;

		if (stream.isWriting)
		{
			syncPosition = rigidbody2D.position;
			stream.Serialize (ref syncPosition);

			syncRotation = transform.rotation;
			stream.Serialize (ref syncRotation);
		}
		else
		{
			stream.Serialize (ref syncPosition);

			stream.Serialize (ref syncRotation);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			syncStartPosition = rigidbody2D.position;
			syncEndPosition = syncPosition;

			syncStartRotation = transform.rotation;
			syncEndRotation = syncRotation;
		}
	}

}
