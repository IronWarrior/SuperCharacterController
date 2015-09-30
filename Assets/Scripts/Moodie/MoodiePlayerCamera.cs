using UnityEngine;
using System.Collections;

public class MoodiePlayerCamera : MonoBehaviour {

    public float Distance = 5.0f;
    public float Height = 2.0f;

    public GameObject PlayerTarget;

    private PlayerInputController input;
    private Transform target;
    private MoodiePlayerMachine machine;
    private float yRotation;

    private CustomController controller;

    // Use this for initialization
    void Start()
    {
        input = PlayerTarget.GetComponent<PlayerInputController>();
        machine = PlayerTarget.GetComponent<MoodiePlayerMachine>();
        controller = PlayerTarget.GetComponent<CustomController>();
        target = PlayerTarget.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position;

        yRotation += input.Current.MouseInput.y;
 
        Vector3 left = Vector3.Cross(machine.lookDirection, controller.transform.up);

        transform.rotation = Quaternion.LookRotation(machine.lookDirection, controller.transform.up);
        transform.rotation = Quaternion.AngleAxis(yRotation, left) * transform.rotation;

        transform.position -= transform.forward * Distance;
        transform.position += controller.transform.up * Height;
    }
}
