using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PopulateFields : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //putText(text1, fields);
    }

    public void putText(string text1,string field)
    {
        GameObject IPFieldgameObject = GameObject.Find(field);
        if (IPFieldgameObject == null)
        {
            Debug.Log("Game Object is null");
        }
        InputField inputField = IPFieldgameObject.GetComponent<InputField>();
        if (inputField == null)
        {
            Debug.Log("inputfield is null");
        }
        inputField.text = text1;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
