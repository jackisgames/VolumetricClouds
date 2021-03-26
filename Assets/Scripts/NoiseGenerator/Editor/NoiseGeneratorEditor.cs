using UnityEditor;
using UnityEngine;

namespace NoiseGenerator
{
	[CustomEditor(typeof(NoiseGenerator))]
	public class NoiseGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Generate 3D texture"))
			{
				for ( int i = 0; i < targets.Length; i++ )
				{
					((NoiseGenerator) targets[i]).Generate3DTexture();
				}
			}
		}
    }
}
