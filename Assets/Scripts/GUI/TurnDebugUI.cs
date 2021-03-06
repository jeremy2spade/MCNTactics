﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TurnDebugUI : MonoBehaviour
{
    private Text _turnTeam;
    private Text _actPoint;

    void Start()
    {
        _turnTeam = transform.GetChild(0).GetComponent<Text>();
        _actPoint = transform.GetChild(1).GetComponent<Text>();
    }

    void Update()
    {
        DisplayTurnInfo();
    }

    private void DisplayTurnInfo()
    {
        _turnTeam.text = string.Format("TurnTeam : {0}", GameManager.Get<GameManager.Turn>().Team);
        _actPoint.text = string.Format("TurnActPoint : {0}", GameManager.Get<GameManager.Turn>().ActPoint);
    }
}
