using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using TMPro;

public class PlayerInteraction : NetworkBehaviour
{
    public string tooltip;
    public int priority;
    public KeyCode button;
}
