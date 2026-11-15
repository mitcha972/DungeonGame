using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>Owns equipped items, their visuals and inventory exchanges for any character.</summary>
public class CharacterEquipment : MonoBehaviour
{
    public enum BodyRegion { Torso = 0, Arms = 1, Legs = 2, Feet = 3 }
    [FormerlySerializedAs("targetMesh"), SerializeField] private SkinnedMeshRenderer bodyRenderer;
    [FormerlySerializedAs("itemDrop"), SerializeField] private GameObject itemDropPrefab;
    [FormerlySerializedAs("attackHitBox"), SerializeField] private GameObject weaponHitboxObject;
    [SerializeField] private WeaponAttackSet innateAttacks;
    [SerializeField] private Inventory inventory;
    private readonly Dictionary<EquipmentSlot, Equipment> items = new Dictionary<EquipmentSlot, Equipment>();
    private readonly Dictionary<EquipmentSlot, SkinnedMeshRenderer> visuals = new Dictionary<EquipmentSlot, SkinnedMeshRenderer>();
    private CharacterAnimation animationOwner;
    private AttackHitBox weaponHitbox;
    private bool initialized, changing;
    public IReadOnlyDictionary<EquipmentSlot, Equipment> Items => items;
    public IReadOnlyDictionary<EquipmentSlot, SkinnedMeshRenderer> Visuals => visuals;
    public Inventory Inventory => inventory;
    public bool IsChanging => changing;
    public WeaponAttackSet InnateAttacks => innateAttacks;
    public WeaponAttackSet ActiveAttackSet => items.TryGetValue(EquipmentSlot.MainHand, out var weapon) ? weapon.weaponAttacks : innateAttacks;
    public event Action<Equipment, Equipment> EquipmentChanged;
    protected CharacterAnimation Animation { get { Initialize(); return animationOwner; } }
    protected virtual void Awake() => Initialize();
    private void Initialize()
    {
        if (initialized) return;
        initialized = true;
        if (inventory == null) inventory = GetComponent<Inventory>();
        animationOwner = GetComponentInParent<CharacterAnimation>();
        if (weaponHitboxObject != null) weaponHitbox = weaponHitboxObject.GetComponent<AttackHitBox>();
        if (weaponHitbox == null) weaponHitbox = GetComponentInParent<CharacterCombat>()?.attackHitBox;
    }
    protected virtual void Start() { Initialize(); Setup(); }
    public virtual void Setup() { }
    public virtual void Equip(Equipment item, int inventorySlot)
    {
        Initialize();
        if (changing || item == null || inventory == null || inventorySlot < 0 || inventorySlot >= inventory.items.Count || inventory.items[inventorySlot] != item) return;
        if (Animation != null && Animation.IsBusy) return;
        items.TryGetValue(item.equipSlot, out var previous);
        changing = true;
        try
        {
            inventory.items[inventorySlot] = previous;
            Replace(item, previous);
            inventory.NotifyExchange();
            Animation?.PlayAction("Equip", true);
        }
        finally { changing = false; }
    }
    // Startup/class/AI equipment does not consume a bag slot; changes still cancel an active attack.
    public virtual void Equip(Equipment item)
    {
        Initialize();
        if (changing || item == null) return;
        items.TryGetValue(item.equipSlot, out var previous);
        if (item == previous) return;
        changing = true;
        try { Replace(item, previous); }
        finally { changing = false; }
    }
    private void Replace(Equipment item, Equipment previous)
    {
        RemoveVisual(item.equipSlot);
        items[item.equipSlot] = item;
        if (item.mesh != null && bodyRenderer != null)
        {
            var mesh = Instantiate(item.mesh, bodyRenderer.transform.parent, false);
            mesh.rootBone = bodyRenderer.rootBone;
            mesh.bones = bodyRenderer.bones;
            visuals[item.equipSlot] = mesh;
            if (item.equipSlot == EquipmentSlot.MainHand) weaponHitbox?.ConfigureWeapon(mesh);
        }
        RefreshBodyRegions();
        EquipmentChanged?.Invoke(item, previous);
    }
    public virtual Equipment Unequip(EquipmentSlot slot)
    {
        Initialize();
        if (changing || !items.TryGetValue(slot, out var previous)) return null;
        changing = true;
        try { Remove(slot, previous); return previous; }
        finally { changing = false; }
    }
    private void Remove(EquipmentSlot slot, Equipment previous)
    {
        items.Remove(slot);
        RemoveVisual(slot);
        RefreshBodyRegions();
        EquipmentChanged?.Invoke(null, previous);
    }
    public bool TryUnequipToInventory(EquipmentSlot slot)
    {
        Initialize();
        if (changing || inventory == null || !items.TryGetValue(slot, out var previous)) return false;
        int destination = inventory.items.FindIndex(item => item == null);
        if (destination < 0) return false;
        changing = true;
        try
        {
            inventory.items[destination] = previous;
            Remove(slot, previous);
            inventory.NotifyExchange();
            return true;
        }
        finally { changing = false; }
    }
    public void UnequipAll() { foreach (var slot in items.Keys.ToArray()) Unequip(slot); }
    public void UnequipAllToInventory() { foreach (var slot in items.Keys.ToArray()) TryUnequipToInventory(slot); }
    private void RemoveVisual(EquipmentSlot slot)
    {
        if (slot == EquipmentSlot.MainHand) weaponHitbox?.ConfigureWeapon(null);
        if (visuals.TryGetValue(slot, out var mesh) && mesh != null) Destroy(mesh.gameObject);
        visuals.Remove(slot);
    }
    private void RefreshBodyRegions()
    {
        if (bodyRenderer == null || bodyRenderer.sharedMesh == null) return;
        for (int i = 0; i < 4 && i < bodyRenderer.sharedMesh.blendShapeCount; i++)
        {
            bool covered = items.Values.Any(item => item.coveredMeshRegions != null && Array.IndexOf(item.coveredMeshRegions, (BodyRegion)i) >= 0);
            bodyRenderer.SetBlendShapeWeight(i, covered ? 100f : 0f);
        }
    }
    public void DropItem(Item item) => TryDropItem(item);
    public bool TryDropItem(Item item)
    {
        if (item == null) return false;
        if (item.worldPrefab != null)
        {
            if (item.worldPrefab.GetComponent<ItemInteract>() == null) return false;
            var pickup = Instantiate(item.worldPrefab, transform.position + transform.up + transform.forward, Quaternion.identity);
            pickup.name = item.name;
            pickup.GetComponent<ItemInteract>().item = item;
            return true;
        }
        if (!(item is Equipment equipment) || equipment.mesh == null || itemDropPrefab == null) return false;
        var dropped = Instantiate(itemDropPrefab, transform.position + transform.up + transform.forward, Quaternion.identity);
        var interact = dropped.GetComponent<ItemInteract>();
        var preview = dropped.transform.Find("ItemPreview");
        var filter = preview != null ? preview.GetComponent<MeshFilter>() : null;
        var renderer = preview != null ? preview.GetComponent<MeshRenderer>() : null;
        if (interact == null || filter == null || renderer == null)
        {
            dropped.SetActive(false); Destroy(dropped);
            Debug.LogWarning("Item drop requires ItemInteract and an ItemPreview mesh/renderer.", this);
            return false;
        }
        dropped.name = equipment.name; interact.item = item;
        filter.sharedMesh = equipment.mesh.sharedMesh; renderer.sharedMaterials = equipment.mesh.sharedMaterials;
        preview.localScale = equipment.mesh.transform.localScale;
        return true;
    }
    protected virtual void OnDestroy()
    {
        foreach (var mesh in visuals.Values) if (mesh != null) Destroy(mesh.gameObject);
        visuals.Clear();
    }
}
