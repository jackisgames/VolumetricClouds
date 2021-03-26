using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
namespace NoiseGenerator
{
    class NoiseGenerator : MonoBehaviour
    {
	    [SerializeField]
	    private int m_textureResolution = 256;

	    [SerializeField]
	    private int m_layer1NumPoints = 30;

	    [SerializeField]
	    private int m_layer2NumPoints = 50;

	    [SerializeField]
	    private int m_layer3NumPoints = 100;

	    [SerializeField]
	    private int m_layer4NumPoints = 300;

        [SerializeField]
	    private int m_randomSeeds;

	    [SerializeField]
	    private string m_assetName = "Noise";

	    [SerializeField]
	    private ComputeShader m_computeShader;

	    private void Start()
	    {
			Generate3DTexture();
	    }

	    public void Generate3DTexture()
	    {

		    // Shift the given address upwards if/as necessary to
		    // ensure it is aligned to the given number of bytes.
		    /*inline uintptr_t AlignAddress(uintptr_t addr, size_t align)
		    {
			    const size_t mask = align - 1;
			    assert((align & mask) == 0); // pwr of 2
			    return (addr + mask) & ~mask;
		    }*/

            System.Random random = new System.Random( m_randomSeeds );

			RenderTexture[] noiseTextures = new RenderTexture[ m_textureResolution ];

		    for (int i = 0; i < m_textureResolution; i++)
		    {
			    RenderTexture noiseTexture = new RenderTexture(m_textureResolution, m_textureResolution, 0);
			    noiseTexture.enableRandomWrite = true;
			    noiseTexture.Create();

			    noiseTextures[i] = noiseTexture;
		    }

		    

            Generate3DTexture( m_layer1NumPoints, new Vector4( 1, 0, 0, 0 ), random, noiseTextures);
            Generate3DTexture( m_layer2NumPoints, new Vector4( 0, 1, 0, 0 ), random, noiseTextures);
            Generate3DTexture( m_layer3NumPoints, new Vector4( 0, 0, 1, 0 ), random, noiseTextures);
            Generate3DTexture( m_layer4NumPoints, new Vector4( 0, 0, 0, 1 ), random, noiseTextures);


            //preview
		    Texture3D texture3D = new Texture3D(m_textureResolution, m_textureResolution, m_textureResolution, TextureFormat.ARGB32, false);
            Texture2D result = new Texture2D(m_textureResolution, m_textureResolution, TextureFormat.ARGB32, false);
		    Color32[] colors = texture3D.GetPixels32(0);

            for (int i = 0; i < m_textureResolution; i++)
		    {
			    RenderTexture.active = noiseTextures[i];
			    result.ReadPixels(new Rect(0, 0, m_textureResolution, m_textureResolution), 0, 0);
			    result.Apply();

				System.Array.ConstrainedCopy(result.GetPixels32(0), 0, colors, m_textureResolution * m_textureResolution * i, m_textureResolution * m_textureResolution );
			    RenderTexture.active = null;
            }
            File.WriteAllBytes("Assets/noise.png", result.EncodeToPNG());

		    texture3D.SetPixels32(colors);
            texture3D.Apply();

		    UnityEditor.AssetDatabase.CreateAsset( texture3D, "Assets/" + m_assetName + ".asset");
            //
        }

	    private void Generate3DTexture( int numPoints, Vector4 colorMask, System.Random random, RenderTexture[] noiseTextures )
	    {
			List<Vector3Int> points= new List<Vector3Int>( numPoints * 9 );

		    for (int i = 0; i < numPoints; i++)
		    {
			    var pt = new Vector3Int(random.Next(0, m_textureResolution), random.Next(0, m_textureResolution), random.Next(0, m_textureResolution));

			    points.Add( pt );//center

			    points.Add( pt + Vector3Int.left * m_textureResolution);
			    points.Add( pt + Vector3Int.right * m_textureResolution);
                points.Add( pt + Vector3Int.up * m_textureResolution);
			    points.Add( pt + Vector3Int.down * m_textureResolution);

			    points.Add(pt + new Vector3Int(0, 0, m_textureResolution));
			    points.Add(pt + new Vector3Int(0, 0, -m_textureResolution));

			    points.Add(pt + new Vector3Int(m_textureResolution, m_textureResolution, 0));
			    points.Add(pt + new Vector3Int(-m_textureResolution, -m_textureResolution, 0));
			    points.Add(pt + new Vector3Int(-m_textureResolution, m_textureResolution, 0));
			    points.Add(pt + new Vector3Int(m_textureResolution, -m_textureResolution, 0));

			    points.Add(pt + new Vector3Int(0, m_textureResolution, m_textureResolution));
			    points.Add(pt + new Vector3Int(0, -m_textureResolution, m_textureResolution));
			    points.Add(pt + new Vector3Int(0, m_textureResolution, -m_textureResolution));
			    points.Add(pt + new Vector3Int(0, -m_textureResolution, -m_textureResolution));

			    points.Add(pt + new Vector3Int(m_textureResolution, 0, m_textureResolution)); //1,1,1
			    points.Add(pt + new Vector3Int(-m_textureResolution, 0, m_textureResolution));
			    points.Add(pt + new Vector3Int(-m_textureResolution, 0, -m_textureResolution));
			    points.Add(pt + new Vector3Int(m_textureResolution, 0, -m_textureResolution));

                points.Add( pt + new Vector3Int(m_textureResolution, m_textureResolution, m_textureResolution)); //1,1,1
			    points.Add( pt + new Vector3Int(-m_textureResolution, -m_textureResolution, m_textureResolution));
			    points.Add( pt + new Vector3Int(-m_textureResolution, m_textureResolution, m_textureResolution));
			    points.Add( pt + new Vector3Int(m_textureResolution, -m_textureResolution, m_textureResolution));

			    points.Add( pt + new Vector3Int(m_textureResolution, m_textureResolution, -m_textureResolution));//1, 1, -1
                points.Add( pt + new Vector3Int(-m_textureResolution, m_textureResolution, -m_textureResolution));//-1, 1, -1
                points.Add( pt + new Vector3Int(m_textureResolution, -m_textureResolution, -m_textureResolution));//1, -1, -1
			    points.Add( pt + new Vector3Int(-m_textureResolution, -m_textureResolution, -m_textureResolution));//-1,-1,-1
            }

            ComputeBuffer pointsBuffer = new ComputeBuffer(points.Count, sizeof(float) * 3);
		    pointsBuffer.SetData(points);

		    m_computeShader.SetInt("Dimension", m_textureResolution);
		    m_computeShader.SetVector("ColorChannelMask", colorMask);
		    m_computeShader.SetInt("NumPoints", points.Count);

            for ( int i = 0; i < m_textureResolution; i++ )
		    {
			    uint[] minMax = new[] { uint.MaxValue, 0u };
			    ComputeBuffer minMaxBuffer = new ComputeBuffer(2, sizeof(uint));

			    minMaxBuffer.SetData(minMax);
			    m_computeShader.SetBuffer(0, "MinMax", minMaxBuffer);

			    m_computeShader.SetTexture(0, "NoiseTexture", noiseTextures[i]);
			    
			    m_computeShader.SetBuffer(0, "Points", pointsBuffer);
			    
			    m_computeShader.SetInt("Slice", i);
			    

			    uint threadGroupX, threadGroupY, threadGroupZ;
			    m_computeShader.GetKernelThreadGroupSizes(0, out threadGroupX, out threadGroupY, out threadGroupZ);

			    //first pass generate noises
			    m_computeShader.Dispatch(0, Mathf.CeilToInt(m_textureResolution / threadGroupX), Mathf.CeilToInt(m_textureResolution / threadGroupY), 1 );

                //second pass, normalize values and copy
                m_computeShader.SetTexture(1, "NoiseTexture", noiseTextures[i]);
                m_computeShader.SetBuffer(1, "MinMax", minMaxBuffer);
			    m_computeShader.Dispatch(1, Mathf.CeilToInt(m_textureResolution / threadGroupX), Mathf.CeilToInt(m_textureResolution / threadGroupY), 1 );

			    minMaxBuffer.GetData(minMax);
			    minMaxBuffer.Dispose();
            }
		   
		    pointsBuffer.Dispose();
		    
        }
    }
}
#endif