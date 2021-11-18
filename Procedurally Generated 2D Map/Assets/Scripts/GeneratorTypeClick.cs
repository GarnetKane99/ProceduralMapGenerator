using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeneratorTypeClick : MonoBehaviour
{
    public GameObject MenuItems;
    public Button SlowGen;
    public Button BackButton;
    public MapGeneration mapGen;

    // Start is called before the first frame update
    void Start()
    {
        SlowGen.onClick.AddListener(SlowGeneration);
        BackButton.onClick.AddListener(OpenMenu);
    }

    void SlowGeneration()
    {
        mapGen.cancelGen = false;
        mapGen.SetupRoom();
        MenuItems.SetActive(false);
        BackButton.gameObject.SetActive(true);
    }

    void OpenMenu()
    {
        mapGen.StopCoroutine(mapGen.CreateFloors());
        mapGen.StopCoroutine(mapGen.StartWalls());
        mapGen.cancelGen = true;
        mapGen.clearMap();
        mapGen.floorCounter = 0;
        MenuItems.SetActive(true);
        BackButton.gameObject.SetActive(false);
    }
}
