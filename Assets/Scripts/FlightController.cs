using UnityEngine;

public class FlightController : MonoBehaviour
{
	[SerializeField]
	private float m_maxAltitude = 120;

	[SerializeField]
	private float m_turnSpeed = 40;

	[SerializeField]
	private float m_forwardSpeed = 10;

	private float m_engineStallTimer = 0;
	private float m_stallTimer = 0;
	private float m_dropSpeed = 0;
    private void Update()
    {
	    if (m_stallTimer > 0)
	    {
			Vector3 cross = Vector3.Cross( transform.forward, Vector3.down );
		    float dot = Vector3.Dot(transform.forward, Vector3.up);

            //transform.Rotate(new Vector3(cross.x, 0, Mathf.Sign(cross.x) ), 180 * Time.deltaTime, Space.Self );
			transform.rotation = Quaternion.Lerp( transform.rotation, Quaternion.LookRotation( Vector3.down, transform.right), Time.deltaTime );
            transform.Translate( Vector3.down * ( m_dropSpeed * Time.deltaTime ) , Space.World );
		    m_stallTimer -= Time.deltaTime;
		    m_dropSpeed += Time.deltaTime;
			return;
	    }

        float inputX = Input.GetAxis("Horizontal");
	    float inputY = Input.GetAxis("Vertical");

	    float altitudeResistance = Mathf.Clamp01( (transform.position.y - m_maxAltitude) / 30 );
	    float upDirection = Vector3.Dot(transform.forward, Vector3.up);

	    float extra = upDirection > 0 ? altitudeResistance * upDirection : upDirection;
        float thrust = m_forwardSpeed * (1 - altitudeResistance) - ( upDirection * m_forwardSpeed * .9f );

	    if (thrust <= 5)
	    {
            //stall
		    m_engineStallTimer += Time.deltaTime;
		    if (m_engineStallTimer > 3)
		    {
			    m_dropSpeed = 0;
			    m_stallTimer = 3 + altitudeResistance * 4;
            }
        }
	    else
	    {
		    m_engineStallTimer = Mathf.Max(0, m_engineStallTimer - Time.deltaTime);
	    }

	    

	    Vector3 moveVector = transform.forward * thrust;
	    moveVector.y -= altitudeResistance * 10;

		transform.Rotate( new Vector3( inputY, 0, inputX), m_turnSpeed * Time.deltaTime, Space.Self );
		transform.Translate( moveVector * Time.deltaTime, Space.World );
    }
}
