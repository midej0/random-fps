using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public string itemID { get; private set; }
    public string itemName { get; private set; }
    public string itemDescription { get; private set; }
    public ItemSlot slot { get; private set; }
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Vector3 posOffset;

    [HideInInspector] public GameObject itemModel;

    public void SpawnItem(Transform parent)
    {
        itemModel = Instantiate(itemPrefab, parent);
        itemModel.transform.localPosition = posOffset;
    }

    public void Holster()
    {
        itemModel.SetActive(false);
    }
    public void Deploy()
    {
        itemModel.SetActive(true);
    }
}
