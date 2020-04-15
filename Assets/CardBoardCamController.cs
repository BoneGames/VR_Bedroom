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
    public float clickTimer;
    bool doubleClick => clickTimer < doubleClickWindow;
    public float doubleClickWindow;
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
        // increment clickTimer
        clickTimer += Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            // ui feedback
            text.text = "touch android";

            // set move direction
            Vector3 dir = Camera.main.transform.forward;
            if (doubleClick)
            {
                // move backwards if double click
                dir *= -1;
                clickTimer = 0;
            }
            dir.y = 0;

            cc.SimpleMove(dir * 50 * Time.deltaTime);
            // reset click timer
        }
        else
        {
            text.text = "no touch android";
        }
        if (Input.GetMouseButtonUp(0))
        {
            clickTimer = 0;
        }
#endif
    }
}
