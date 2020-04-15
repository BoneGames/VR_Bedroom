using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBoardCamController : MonoBehaviour
{
    public float moveSpeed;
    public float turnSpeed;
    CharacterController cc;
    public Text text;
    void Start()
    {
        cc = GetComponent<CharacterController>();
        text = GameObject.FindGameObjectWithTag("debugText").GetComponent<Text>();
        text.text = "start";
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR

        if (Input.GetKey(KeyCode.UpArrow))
        {
            cc.SimpleMove(transform.forward * moveSpeed * Time.deltaTime);
            text.text = "touch editor";
        }
        else
        {
            text.text = "no touch editor";
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0, -turnSpeed, 0));
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(new Vector3(0, turnSpeed, 0));
        }
#endif

#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            text.text = "touch android";
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;
            cc.SimpleMove(forward * 50 * Time.deltaTime);

            //transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        }
        else
        {
            text.text = "no touch android";
        }
#endif
    }
}
