using UnityEngine;
using TMPro;

public class LastKeyPressed : MonoBehaviour {
    private TMP_Text textComponent;

    void Start () {
        textComponent = GetComponent<TMP_Text>();
    }

    void Update () {
        if (Input.anyKeyDown) {
            string lastKeyPressed = Input.inputString;
            textComponent.text = "Last key pressed: " + lastKeyPressed;
        }
    }
}
