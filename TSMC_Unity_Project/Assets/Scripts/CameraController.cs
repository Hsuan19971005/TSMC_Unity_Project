using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public float keycode_translation_sensitivity = 9f;
    public float mouse_translation_sensitivity = 0.9f;
    public float mouse_rotation_sensitivity = 120f;
    public float scroll_speed = 200f;
    float x_rotation = 0f;
    float y_rotation = 0f;
    Button reposition_button;
    private Vector3 initial_position;
    private Quaternion initial_rotation;
    // Start is called before the first frame update
    void Start()
    {
        reposition_button = GameObject.Find("Reposition_Button").GetComponent<Button>();
        initial_position = transform.position;
        initial_rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //按住滑鼠左鍵 水平、垂直旋轉
        if (Input.GetMouseButton(0))
        {
            float mouse_x = Input.GetAxis("Mouse X") * mouse_rotation_sensitivity * Time.deltaTime;
            float mouse_y = Input.GetAxis("Mouse Y") * mouse_rotation_sensitivity * Time.deltaTime;
            x_rotation -= mouse_y;
            y_rotation += mouse_x;
            x_rotation = Mathf.Clamp(x_rotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(x_rotation, y_rotation, 0f);//垂直旋轉
            //transform.Rotate(transform.up * (mouse_x * -1f));//水平旋轉
        }
        //按住滑鼠右鍵 左右、上下平移
        if (Input.GetMouseButton(1))
        {
            float mouse_x = Input.GetAxis("Mouse X") * mouse_rotation_sensitivity * Time.deltaTime;
            float mouse_y = Input.GetAxis("Mouse Y") * mouse_rotation_sensitivity * Time.deltaTime;
            transform.Translate(mouse_x * mouse_translation_sensitivity*-1f, 0, 0);//相機Local左右平移
            transform.Translate(0,mouse_y * mouse_translation_sensitivity*-1f, 0);//相機Local上下平移
            //transform.position += new Vector3(0, mouse_y * mouse_translation_sensitivity, 0);//相機global上下平移
        }
        //滾輪往前滾 前進
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            transform.Translate(0, 0, scroll_speed * Time.deltaTime);
        }
        //滾輪往後滾 後退
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            transform.Translate(0, 0, scroll_speed * -1f * Time.deltaTime);
        }
        //按A向左移動，按D向右移動
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.right * -1f * keycode_translation_sensitivity * Time.deltaTime);
        else if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * keycode_translation_sensitivity * Time.deltaTime);
        //按W向前移動，按S向後移動
        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.forward * keycode_translation_sensitivity * Time.deltaTime);
        else if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.forward * -1f * keycode_translation_sensitivity * Time.deltaTime);
        //按E向上移動，按Q向下移動
        if (Input.GetKey(KeyCode.E)) transform.position += new Vector3(0, keycode_translation_sensitivity * Time.deltaTime, 0);
        else if (Input.GetKey(KeyCode.Q)) transform.position += new Vector3(0, keycode_translation_sensitivity * -1f * Time.deltaTime, 0);
    }
    public void ReturnInitialLocation()
    {
        transform.position = this.initial_position;
        transform.rotation = this.initial_rotation;
        this.x_rotation = 0;
        this.y_rotation = 0;
    }
}
