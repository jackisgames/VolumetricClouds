using UnityEngine;

class CameraController : MonoBehaviour
{
	[SerializeField]
	private Transform m_target;

	[SerializeField]
	private Vector3 m_positionOffset;

	[SerializeField]
	private Vector3 m_lookOffset;

	private Vector3 m_currentLookTarget;
	private Vector3 m_currentLookUp = Vector3.up;

    private void Update()
    {
	    float inputX = Input.GetAxis("Horizontal");
	    float inputY = Input.GetAxis("Vertical");

	    Vector3 targetOffset = m_positionOffset;
	    Vector3 lookOffset = m_lookOffset;
	    targetOffset.x -= inputX;
	    targetOffset.y += inputY * 2;

        Vector3 targetPosition = m_target.position + m_target.rotation * targetOffset;
	    Vector3 targetLook = m_target.position + m_target.rotation * lookOffset;

		transform.position = Vector3.Lerp( transform.position, targetPosition, Time.deltaTime * 3 );
	    m_currentLookTarget = Vector3.Lerp(m_currentLookTarget, targetLook, Time.deltaTime * 3 );
	    m_currentLookUp = Vector3.Slerp(m_currentLookUp, m_target.up, Time.deltaTime * 3 );

	    Quaternion targetRotation = Quaternion.LookRotation(m_currentLookTarget - transform.position, m_currentLookUp);
	    float cruising = 1.0f - Mathf.Abs( Vector3.Dot(transform.forward, Vector3.up) );
		Quaternion cruiseRoll = Quaternion.Euler( 0, 0, (Mathf.Sin( Time.time * .5f) * 10) * cruising );

	    targetRotation *= cruiseRoll;
	    transform.rotation = targetRotation;
    }
}
