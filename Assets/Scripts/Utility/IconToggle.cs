using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconToggle : MonoBehaviour {

    public Sprite m_iconTrue;
    public Sprite m_iconFalse;

    public bool m_defaultIconState = true;

    Image m_image;

	// Use this for initialization
	void Start () {
        m_image = GetComponent<Image>();
        m_image.sprite = (m_defaultIconState) ? m_iconTrue : m_iconFalse;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ToggleIcon (bool state)
    {
        if (!m_image || !m_iconTrue || !m_iconFalse)
        {
            Debug.LogWarning("ATENÇÃO icone toggle não encontrado");
            return;
        }
        m_image.sprite = (state) ? m_iconTrue : m_iconFalse;
    }
}
