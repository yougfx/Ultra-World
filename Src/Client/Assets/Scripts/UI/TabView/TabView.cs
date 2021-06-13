﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabView : MonoBehaviour
{

    public TabButton[] tabButtons;
    public GameObject[] tabPages;

    public int index = -1;
    // Use this for initialization
    IEnumerator Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].tabView = this;
            tabButtons[i].tabIndex = i;
        }
        yield return new WaitForEndOfFrame();
        SelectTab(0);

    }
    public void SelectTab(int index)
    {
        //先判断当前是第几页
        if (this.index != index)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].Select(i == index);
                tabPages[i].SetActive(i == index);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
