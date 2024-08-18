using Mono.Cecil.Cil;
using UnityEngine;

public class RotateIconsUI : MonoBehaviour
{
    // Update is called once per frame
    public Transform RotateIcons;  

    void Update()
    {
        var block = BuildingController.Instance.currentBlock;
        if (block == null) return;
    
        RotateIcons.position = block.transform.position;
    }
}
