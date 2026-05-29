using System;
using System.Globalization;
using System.IO.Ports;
using UnityEngine;

public class ESP32Input : MonoBehaviour
{
    [Header("Serial Settings")]
    [SerializeField] private string portName = "COM5";
    [SerializeField] private int baudRate = 115200;

    [Header("Joystick Settings")]
    [Range(0.1f, 1f)]
    public float joystickSensitivity = 0.5f;

    [Range(0.1f, 5f)]
    public float joystickMoveMultiplier = 1f;

    [Header("Live Values")]
    public float horizontal;
    public float vertical;

    [Header("Button")]
    public bool selectPressed;      // one-shot: true hanya 1 frame
    public bool selectHeld;         // true selama tombol ditekan

    [Header("Connection")]
    public bool isConnected = false;

    private SerialPort serialPort;

    private bool lastButtonState = false; // state tombol frame sebelumnya

    // =========================================
    // START
    // =========================================

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = 50;
        serialPort.NewLine = "\n";

        try
        {
            serialPort.Open();
            isConnected = true;
            Debug.Log("[ESP32Input] Connected to " + portName);
        }
        catch (Exception e)
        {
            isConnected = false;
            Debug.LogError("[ESP32Input] Serial Error: " + e.Message);
        }
    }

    // =========================================
    // UPDATE
    // =========================================

    void Update()
    {
        // Reset one-shot tiap frame
        selectPressed = false;

        if (serialPort == null || !serialPort.IsOpen)
            return;

        try
        {
            string data = serialPort.ReadLine();

            data = data.Trim().Replace("\r", "").Replace("\n", "");

            if (string.IsNullOrEmpty(data))
                return;

            // FORMAT: x,y,button
            string[] val = data.Split(',');

            if (val.Length >= 3)
            {
                bool okX = float.TryParse(
                    val[0].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float x
                );

                bool okY = float.TryParse(
                    val[1].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float y
                );

                bool okButton = int.TryParse(
                    val[2].Trim(),
                    out int btn
                );

                // =====================================
                // JOYSTICK
                // =====================================

                if (okX && okY)
                {
                    if (Mathf.Abs(x) < joystickSensitivity)
                        x = 0;

                    if (Mathf.Abs(y) < joystickSensitivity)
                        y = 0;

                    horizontal = Mathf.Clamp(x * joystickMoveMultiplier, -1f, 1f);
                    vertical   = Mathf.Clamp(y * joystickMoveMultiplier, -1f, 1f);
                }

                // =====================================
                // BUTTON — one-shot
                // =====================================

                if (okButton)
                {
                    bool currentState = (btn == 1);

                    // selectPressed hanya true saat PERTAMA kali ditekan
                    selectPressed = currentState && !lastButtonState;

                    selectHeld = currentState;

                    lastButtonState = currentState;
                }
            }
        }
        catch (TimeoutException)
        {
            // normal
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ESP32Input] Read Error: " + e.Message);
        }
    }

    // =========================================
    // CLOSE SERIAL
    // =========================================

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}