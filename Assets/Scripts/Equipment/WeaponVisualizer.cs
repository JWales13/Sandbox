using UnityEngine;

// Shows the equipped weapon's model in the character's right hand, swapping it
// whenever equipment changes. Rig-agnostic: finds the hand via the Humanoid
// avatar (HumanBodyBones.RightHand), so it survives a character-model swap.
public class WeaponVisualizer : MonoBehaviour
{
    [Tooltip("The character model's Animator (Humanoid). Auto-found in children if empty.")]
    public Animator characterAnimator;

    Transform handBone;
    GameObject currentModel;

    void Start()
    {
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
        if (characterAnimator != null && characterAnimator.isHuman)
            handBone = characterAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        if (Equipment.Instance != null) Equipment.Instance.OnChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (Equipment.Instance != null) Equipment.Instance.OnChanged -= Refresh;
    }

    void Refresh()
    {
        if (currentModel != null) Destroy(currentModel);
        currentModel = null;

        if (handBone == null || Equipment.Instance == null) return;

        var weapon = Equipment.Instance.CurrentWeapon;
        if (weapon == null || weapon.worldModel == null) return;

        currentModel = Instantiate(weapon.worldModel, handBone);
        currentModel.transform.localPosition = weapon.gripPosition;
        currentModel.transform.localEulerAngles = weapon.gripEuler;
        currentModel.transform.localScale = weapon.gripScale;
    }
}