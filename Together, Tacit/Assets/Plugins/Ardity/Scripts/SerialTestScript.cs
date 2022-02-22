using UnityEngine;
using System.Collections;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class SerialTestScript : MonoBehaviour
{
    public SerialController serialController;
    // TEMP: Currently set to a single strong pulse.
    private long hapticString = 0b10100001000101101000;

    // Initialization
    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();

        Debug.Log("Beginning connection, sending haptic codes.");
    }

    // Executed each frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------

        // Check if the 'A' key is pressed. If so, send a haptic code.
        if (Input.GetKeyDown(KeyCode.A)) {
            SendHapticCode();
        }

        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------

        // string message = serialController.ReadSerialMessage();

        // // Check if the message is plain data or a connect/disconnect event.
        // if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
        //     Debug.Log("Connection established");
        // else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
        //     Debug.Log("Connection attempt failed or disconnection detected");
        // else
        //     Debug.Log("Message arrived: " + message);
    }

    void SendHapticCode()
    {
        // When called, sends a haptic string to the COM port
        Debug.Log("Sent haptic code: " + hapticString);
        serialController.SendSerialMessage(hapticString);
    }
}
