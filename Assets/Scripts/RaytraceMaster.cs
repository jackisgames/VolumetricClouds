using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
class RaytraceMaster : MonoBehaviour
{
	[SerializeField]
	private ComputeShader m_computeShader;

	[SerializeField]
	private Texture3D m_cloudTexture;

	[SerializeField]
	private Texture3D m_cloudDetailTexture;

	[SerializeField]
	private Texture2D m_randomNoiseTexture;

	[SerializeField]
	private Texture2D m_cloudFormationTexture;

	[SerializeField]
	private Texture2D m_blueNoiseTexture;

    [SerializeField]
	private LocalCloud[] m_localClouds;

	[SerializeField]
	private float m_densityThreshold = .3f;

	[SerializeField]
	private float m_densityMultiplier = 2.3f;

	[SerializeField]
	private Vector3 m_cloudScale = Vector3.one;

    [SerializeField]
	private int m_maxRaySteps = 10;

	[SerializeField]
	private float m_globalCloudHeight;

	[SerializeField]
	private float m_globalCloudSize;

    private LocalCloudData[] m_localCloudsdata;

    private Camera m_camera;
	private RenderTexture m_target;

	private void Start()
	{
		m_camera = GetComponent<Camera>();
		
	}

	private void OnDestroy()
	{
		if (m_target != null)
		{
			m_target.Release();
		}
    }

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if ( m_target == null || m_target.width != Screen.width || m_target.height != Screen.height)
		{
			if (m_target != null)
			{
				m_target.Release();
			}
			m_target = new RenderTexture(Screen.width, Screen.height, 1);
			m_target.enableRandomWrite = true;
			m_target.Create();
        }

		//apply settings
		m_computeShader.SetInt("MaxRaySteps", m_maxRaySteps);
		m_computeShader.SetFloat("DensityThreshold", m_densityThreshold);
		m_computeShader.SetFloat("DensityMultiplier", m_densityMultiplier);
		m_computeShader.SetFloat("GlobalCloudSize", m_globalCloudSize);
		m_computeShader.SetFloat("GlobalCloudHeight", m_globalCloudHeight);
		m_computeShader.SetVector("CloudScale", m_cloudScale);

        //Generate scene
        if ( m_localCloudsdata == null || m_localClouds.Length > m_localCloudsdata.Length )
		{
			m_localCloudsdata = new LocalCloudData[m_localClouds.Length];
		}

		int numLocalClouds = 0;
		for (int i = 0; i < m_localClouds.Length; ++i)
		{
			if (m_localClouds[i].isActiveAndEnabled)
			{
				m_localCloudsdata[numLocalClouds] = m_localClouds[i].GetData();
				numLocalClouds++;
            }
		}

		ComputeBuffer localCloudsBuffer = new ComputeBuffer(m_localCloudsdata.Length, LocalCloudData.GetSize());
		localCloudsBuffer.SetData(m_localCloudsdata, 0, 0, Mathf.Max( numLocalClouds, 1) );
		m_computeShader.SetBuffer(0, "LocalClouds", localCloudsBuffer);
        m_computeShader.SetInt("NumLocalClouds", numLocalClouds);
		

		uint threadGroupX, threadGroupY, threadGroupZ;
		m_computeShader.GetKernelThreadGroupSizes(0, out threadGroupX, out threadGroupY, out threadGroupZ);
		m_computeShader.SetMatrix( "CameraToWorldMatrix", m_camera.cameraToWorldMatrix );
		m_computeShader.SetMatrix( "CameraInvProjectionMatrix", m_camera.projectionMatrix.inverse );
		m_computeShader.SetTexture(0, "CloudTexture", m_cloudTexture );
		m_computeShader.SetTexture(0, "CloudDetailTexture", m_cloudDetailTexture );
		m_computeShader.SetTexture(0, "RandomNoiseTexture", m_randomNoiseTexture );
		m_computeShader.SetTexture(0, "BlueNoiseTexture", m_blueNoiseTexture );
		m_computeShader.SetTexture(0, "CloudFormationMapTexture", m_cloudFormationTexture );
		m_computeShader.SetTextureFromGlobal(0, "_CameraDepthTexture", "_CameraDepthTexture");

        m_computeShader.SetTexture(0, "Source", source);
        
        m_computeShader.SetTexture(0, "Target", m_target);
		m_computeShader.Dispatch(0, Mathf.CeilToInt(m_target.width / threadGroupX), Mathf.CeilToInt(m_target.width / threadGroupY), (int)threadGroupZ);
		
		localCloudsBuffer.Dispose();
        Graphics.Blit(m_target, destination);
    }
}
