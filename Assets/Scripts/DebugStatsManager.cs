using UnityEngine;
using TMPro;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public enum DebugStatsType
{
	PrimEvent,
	PrimUpdate,
	NewPrim,
	TextureDownloadRequest,
	MeshDownloadRequest,
	SculptDownloadRequest,
	DecodedMeshProcess,
	DecodedSkinnedMeshProcess,
	DecodedTextureProcess,
}

public class DebugStatsManager : MonoBehaviour
{

	private static DebugStatsManager s_Instance;
	private string _reportText;

	private void Awake()
	{
		if (s_Instance != null && s_Instance != this)
		{
			Destroy(this);
		}
		else if (s_Instance == null)
		{
			s_Instance = this;
			_ = AsyncTimer();
		}
	}

	private async Task AsyncTimer()
	{
		while (true)
		{
			await Task.Delay(250);
			_reportText = BuildReport();
		}
	}

	public static void AddStateUpdate(DebugStatsType stateName, string stateValue)
	{
		if (s_Instance == null)
		{
			// 	Debug.LogError("MeshStatsManager is not initialized");
			return;
		}

		s_Instance.AddStateInternal(stateName, stateValue);
	}

	private ConcurrentDictionary<string, int> m_PrimEventCount = new();
	private ConcurrentDictionary<string, int> m_PrimUpdateCount = new();
	private ConcurrentDictionary<string, int> m_NewPrimCount = new();
	private int m_MeshDownloadRequestCount = 0;
	private int m_SculptDownloadRequestCount = 0;
	private int m_MeshDecodedCount = 0;
	private int m_SkinnedMeshDecodedCount = 0;
	private int m_TextureDownloadRequestCount = 0;
	private int m_TextureDecodedCount = 0;

	// private List<Vector3> tmplst = new();

	public TMP_Text debugUIDisplay = null;

	// fixed main thread updates, doesn't have remove state or a toggle. 
	// fixing bottle necks first, but add toggle, more efficient tracking, and better threaded collections
	private void AddStateInternal(DebugStatsType stateName, string stateValue)
	{
		// PrimEvent, PrimUpdate, NewPrim, MeshDownloadRequest, SculptDownloadRequest, DecodedMeshProcess
		if (stateName == DebugStatsType.PrimEvent)
		{
			m_PrimEventCount.TryAdd(stateValue, 0);
			m_PrimEventCount[stateValue]++;
		}
		else if (stateName == DebugStatsType.PrimUpdate)
		{
			m_PrimUpdateCount.TryAdd(stateValue, 0);
			m_PrimUpdateCount[stateValue]++;
		}
		else if (stateName == DebugStatsType.NewPrim)
		{
			m_NewPrimCount.TryAdd(stateValue, 0);
			m_NewPrimCount[stateValue]++;
		}
		else if (stateName == DebugStatsType.MeshDownloadRequest)
		{
			m_MeshDownloadRequestCount++;
		}
		else if (stateName == DebugStatsType.SculptDownloadRequest)
		{
			m_SculptDownloadRequestCount++;
		}
		else if (stateName == DebugStatsType.DecodedMeshProcess)
		{
			m_MeshDecodedCount++;
		}
		else if (stateName == DebugStatsType.DecodedSkinnedMeshProcess)
		{
			m_SkinnedMeshDecodedCount++;
		}
		else if (stateName == DebugStatsType.TextureDownloadRequest)
		{
			m_TextureDownloadRequestCount++;
		}
		else if (stateName == DebugStatsType.DecodedTextureProcess)
		{
			m_TextureDecodedCount++;
		}
		//else if(stateName == "NewPrimTemp")
		//{
		//	var tmp = stateValue.Split(' ');
		//	tmplst.Add(new Vector3(float.Parse(tmp[0]), float.Parse(tmp[2]), float.Parse(tmp[1])));
		// }

	}

	public string BuildReport()
	{
		string report = "", tmp = "";
		report += "PrimEventCount: ";
		tmp = "";
		foreach (var item in m_PrimEventCount)
		{
			if (tmp.Length > 100) tmp += "\n";
			tmp += item.Key + ": " + item.Value + ", ";
		}
		report += tmp + "\n";
		report += "PrimUpdateCount: ";
		tmp = "";
		foreach (var item in m_PrimUpdateCount)
		{
			if (tmp.Length > 100) tmp += "\n";
			tmp += item.Key + ": " + item.Value + ", ";
		}
		report += tmp + "\n";
		report += "NewPrimCount: ";
		tmp = "";
		foreach (var item in m_NewPrimCount)
		{
			if (tmp.Length > 100) tmp += "\n";
			tmp += item.Key + ": " + item.Value + ", ";
		}
		report += tmp + "\n";
		report += "MeshDownloadRequestCount: " + m_MeshDownloadRequestCount + "\n";
		report += "SculptDownloadRequestCount: " + m_SculptDownloadRequestCount + "\n";
		report += "MeshDecodedCount: " + m_MeshDecodedCount + "\n";
		report += "SkinnedMeshDecodedCount: " + m_SkinnedMeshDecodedCount + "\n";
		report += "TextureDownloadRequestCount: " + m_TextureDownloadRequestCount + "\n";
		report += "TextureDecodedCount: " + m_TextureDecodedCount + "\n";
		report += "MeshDownloadRequestCount to MeshDecodedCount: " + ((float)m_MeshDecodedCount / (float)m_MeshDownloadRequestCount) * 100 + "\n";
		report += "TextureDownloadRequestCount to TextureDecodedCount: " + ((float)m_TextureDecodedCount / (float)m_TextureDownloadRequestCount) * 100 + "\n";
		return report;
	}

	private void LateUpdate()
	{

		if (debugUIDisplay != null)
		{
			debugUIDisplay.text = _reportText;
		}

	}
}