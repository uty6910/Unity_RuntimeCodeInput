using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class menuActivate : MonoBehaviour {

    public Image ActivateImage;

    private bool isActivate;

    public bool Activate { get { return isActivate; } set { isActivate = value; ArrowButton(); } }


	// Use this for initialization
	void Start () {
        ActivateImage = this.GetComponentInChildren<Image>();
        ActivateImage.gameObject.SetActive(false);
	}


    void ArrowButton()
    {
        ActivateImage.gameObject.SetActive(isActivate);
    }
}
