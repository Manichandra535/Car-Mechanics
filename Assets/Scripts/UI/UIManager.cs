using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Toggle steerContolsToggle;
    [SerializeField] GameObject steerControlInpuObject;
    [SerializeField] GameObject buttonControlInputObject;
    [SerializeField] TMP_Text rPMText;
    [SerializeField] TMP_Text speedText;

    [SerializeField] CarControl carControl;


    void Start()
    {
        steerContolsToggle.onValueChanged.AddListener(OnChangeInControls);
        OnChangeInControls(steerContolsToggle.isOn);
    }

    void OnChangeInControls(bool isOn)
    {
        steerControlInpuObject.SetActive(isOn);
        buttonControlInputObject.SetActive(!isOn);
    }

    void Update()
    {
        rPMText.text = "RPM: " + Mathf.Abs(carControl.GetRPM()).ToString("0.00");
        speedText.text = "Speed: " + Mathf.Abs(carControl.GetSpeed()).ToString("0.00") + " KPH";
    }
}
