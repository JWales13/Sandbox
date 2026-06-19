using UnityEngine;

// Saves/restores this object's position + facing. Put it on the Player.
// (A CharacterController is briefly disabled so the teleport isn't fought.)
public class TransformSaver : MonoBehaviour, ISaveable
{
    public string saveId = "playerTransform";

    public string SaveId => saveId;

    public string WriteState() =>
        JsonUtility.ToJson(new TransformState { pos = transform.position, euler = transform.eulerAngles });

    public void ReadState(string data)
    {
        var s = JsonUtility.FromJson<TransformState>(data);
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = s.pos;
        transform.eulerAngles = new Vector3(0f, s.euler.y, 0f);
        if (cc != null) cc.enabled = true;
    }
}