using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IDragHandler
{
	RectTransform m_transform = null;

	// Use this for initialization
	void Start()
	{
		m_transform = transform.parent.gameObject.GetComponent<RectTransform>();
	}

	public void OnDrag(PointerEventData eventData)
	{
		m_transform.position += new Vector3(eventData.delta.x, eventData.delta.y);
		//Debug.Log("draaaaag");
		// magic : add zone clamping if's here.
	}

}
