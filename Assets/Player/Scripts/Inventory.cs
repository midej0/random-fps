using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items;
    private GameObject camHolder;
    private PlayerInputHandler playerInputHandler;

    void Awake()
    {
        camHolder = GameObject.FindGameObjectWithTag("cameraHolder");
    }

    void Start()
    {
        playerInputHandler = PlayerInputHandler.instance;
        items[0].SpawnItem(camHolder.transform);
    }
}