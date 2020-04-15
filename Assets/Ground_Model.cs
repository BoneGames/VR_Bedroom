using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Ground_Model : MonoBehaviour
{
    public void OnValidate()
    {
        Move();
    }

    [Button]
    public void Move()
    {
        //Debug.Log(this.name + " ground_Mod");
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localScale.y / 2, transform.localPosition.z);
    }
}
