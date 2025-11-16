using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> inventory;

    private GameObject camHolder;
    private PlayerInputHandler playerInputHandler;
    private int selectedItemIndex = 0;

    void Awake()
    {
        camHolder = GameObject.FindGameObjectWithTag("cameraHolder");
    }

    void Start()
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            playerInputHandler = PlayerInputHandler.instance;
            inventory[i].SpawnItem(camHolder.transform);
            if(i != selectedItemIndex)
            {
                inventory[i].Holster();
            }
        }
    }

    void Update()
    {
        if(playerInputHandler.slot1Triggered && selectedItemIndex != 0)
        {
            SwitchItem(0);
        }
        else if(playerInputHandler.slot2Triggered && selectedItemIndex != 1)
        {
            SwitchItem(1);
        }
        else if(playerInputHandler.slot3Triggered && selectedItemIndex != 2)
        {
            SwitchItem(2);
        }
    }

    private void SwitchItem(int newItemIndex)
    {
        inventory[selectedItemIndex].Holster();
        selectedItemIndex = newItemIndex;
        inventory[selectedItemIndex].Deploy();
    }
}