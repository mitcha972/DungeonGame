using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HotKeysUI : MonoBehaviour
{
    public Inventory inventory;
    public GameObject[] hotKeys;
    public GameObject[] imageSlots;

    private void Start()
    {
        if (inventory != null) inventory.onItemChangedCallback += UpUI;
        RefreshIcons();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.onItemChangedCallback -= UpUI;
    }

    private void UpUI() => RefreshIcons();

    private void RefreshIcons()
    {
        if (hotKeys == null || imageSlots == null) return;
        int count = Mathf.Min(hotKeys.Length, imageSlots.Length);
        for (int i = 0; i < count; i++)
        {
            if (hotKeys[i] == null || imageSlots[i] == null) continue;
            var source = imageSlots[i].GetComponent<Image>();
            var destination = hotKeys[i].GetComponent<Image>();
            if (source == null || destination == null) continue;
            destination.sprite = source.sprite;
            hotKeys[i].SetActive(source.sprite != null);
        }
    }

    public IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(0.08f);
        RefreshIcons();
    }
}
