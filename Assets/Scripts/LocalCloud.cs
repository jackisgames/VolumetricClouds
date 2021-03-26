using UnityEngine;

struct LocalCloudData
{
	public Vector3 Position;
    public Vector3 Bounds;
	public float TextureScale;
	public float DensityThreshold;
	public float DensityMultiplier;
	public float EdgeFadeDistance;

    public static int GetSize()
	{
		return sizeof(float) * ( 3 + 3 + 4 );
	}
}

class LocalCloud : MonoBehaviour
{
	[SerializeField]
	private float m_textureScale = .1f;

	[SerializeField]
	private float m_densityThreshold = .6f;

	[SerializeField]
	private float m_densityMultiplier = 1.0f;

	[SerializeField]
	private float m_edgeFadeDistance = 9.0f;

    [SerializeField]
	private Transform m_tester;

    public LocalCloudData GetData()
	{
		var shapeData = new LocalCloudData();
		shapeData.Position = transform.position;
		shapeData.Bounds = transform.lossyScale * .5f;
		shapeData.TextureScale = m_textureScale;
		shapeData.DensityThreshold = m_densityThreshold;
		shapeData.DensityMultiplier = m_densityMultiplier;
		shapeData.EdgeFadeDistance = m_edgeFadeDistance;
		return shapeData;
	}

#if UNITY_EDITOR
    private void OnDrawGizmos()
	{
		Vector3 center = transform.position;
		Vector3 extents = transform.lossyScale * .5f;
		Color color = Color.white;
		Debug.DrawLine( center + new Vector3(extents.x, extents.y, extents.z ), center + new Vector3( -extents.x, extents.y, extents.z), color, 0 , true );
		Debug.DrawLine( center + new Vector3(extents.x, -extents.y, extents.z ), center + new Vector3( -extents.x, -extents.y, extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(extents.x, extents.y, -extents.z), center + new Vector3(-extents.x, extents.y, -extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(extents.x, -extents.y, -extents.z), center + new Vector3(-extents.x, -extents.y, -extents.z), color, 0, true);

		Debug.DrawLine( center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(extents.x, extents.y, -extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(-extents.x, extents.y, extents.z), center + new Vector3(-extents.x, extents.y, -extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(extents.x, -extents.y, extents.z), center + new Vector3(extents.x, -extents.y, -extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(-extents.x, -extents.y, extents.z), center + new Vector3(-extents.x, -extents.y, -extents.z), color, 0, true);

		Debug.DrawLine( center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(extents.x, -extents.y, extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(-extents.x, extents.y, extents.z), center + new Vector3(-extents.x, -extents.y, extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(extents.x, extents.y, -extents.z), center + new Vector3(extents.x, -extents.y, -extents.z), color, 0, true);
		Debug.DrawLine( center + new Vector3(-extents.x, extents.y, -extents.z), center + new Vector3(-extents.x, -extents.y, -extents.z), color, 0, true);

		if( m_tester == null )
			return;
		LocalCloudData data = GetData();
		Vector2 result = SlabTest(data.Position - data.Bounds, data.Position + data.Bounds, m_tester.position, m_tester.forward);

		Debug.DrawLine( m_tester.position, m_tester.position + m_tester.forward * result.y, Color.red );
		Debug.DrawLine( m_tester.position, m_tester.position + m_tester.forward * result.x, Color.green );

        //falloff test
		Vector3 boundsMin = data.Position - data.Bounds;
		Vector3 boundsMax = data.Position + data.Bounds;
		Vector3 testPos = m_tester.position + m_tester.forward * Mathf.Lerp(result.x, result.y, .05f);

		const float containerEdgeFadeDst = 1f;
		float dstFromEdgeX = Mathf.Min(containerEdgeFadeDst, Mathf.Min(testPos.x - boundsMin.x, boundsMax.x - testPos.x));
		float dstFromEdgeY = Mathf.Min(containerEdgeFadeDst, Mathf.Min(testPos.y - boundsMin.y, boundsMax.y - testPos.y));
		float dstFromEdgeZ = Mathf.Min(containerEdgeFadeDst, Mathf.Min(testPos.z - boundsMin.z, boundsMax.z - testPos.z));

		float falloff = Mathf.Min(dstFromEdgeY, Mathf.Min(dstFromEdgeZ, dstFromEdgeX)) / containerEdgeFadeDst;

		Debug.DrawLine(m_tester.position + m_tester.forward * result.x, testPos, Color.yellow);
		
    }

	private Vector2 SlabTest(Vector3 boundsMin, Vector3 boundsMax, Vector3 rayOrigin, Vector3 raydir)
	{
        // Efficient slab test for ray intersection adapted from: http://jcgt.org/published/0007/03/04/
		Vector3 rayDirInv = new Vector3( 1.0f/ raydir.x , 1.0f / raydir.y, 1.0f / raydir.z);//1.0f / raydir;
		Vector3 t0 = Vector3.Scale( (boundsMin - rayOrigin) , rayDirInv );
		Vector3 t1 = Vector3.Scale( (boundsMax - rayOrigin) , rayDirInv );
		Vector3 tmin = Vector3.Min(t0, t1);
		Vector3 tmax = Vector3.Max(t0, t1);

		float dstA = Mathf.Max(Mathf.Max(tmin.x, tmin.y), tmin.z);
		float dstB = Mathf.Min(tmax.x, Mathf.Min(tmax.y, tmax.z));

		// CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
		// dstA is dst to nearest intersection, dstB dst to far intersection

		// CASE 2: ray intersects box from inside (dstA < 0 < dstB)
		// dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

		// CASE 3: ray misses box (dstA > dstB)
		return new Vector2(Mathf.Max(0, dstA), Mathf.Max(0, dstB));
	}
#endif
}
