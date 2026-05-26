using System;
using System.Globalization;
using System.IO.Ports;
using UnityEngine;

public class ESP32Input : MonoBehaviour
{
    [Header("Serial Settings")]
    [SerializeField] private string portName = "COM5";
    [SerializeField] private int baudRate = 115200;

    [Header("Live Values")]
    public float horizontal;
    public float vertical;
    public bool isConnected = false;

    private SerialPort serialPort;

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = 50;
        serialPort.NewLine = "\n";

        try
        {
            serialPort.Open();
            isConnected = true;
            Debug.Log("[ESP32Input] Serial Opened on " + portName);
        }
        catch (Exception e)
        {
            isConnected = false;
            Debug.LogError("[ESP32Input] Serial Error: " + e.Message);
        }
    }

    void Update()
    {
        if (serialPort == null || !serialPort.IsOpen) return;

        try
        {
            string data = serialPort.ReadLine();
            data = data.Trim().Replace("\r", "").Replace("\n", "");

            if (string.IsNullOrEmpty(data)) return;

            string[] val = data.Split(',');

            // Terima format 2 nilai (x,y) ATAU 3 nilai (x,y,z) dari ESP32
            if (val.Length >= 2)
            {
                bool okX = float.TryParse(val[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
                bool okY = float.TryParse(val[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y);

                if (okX && okY)
                {
                    horizontal = Mathf.Clamp(x, -1f, 1f);
                    vertical   = Mathf.Clamp(y, -1f, 1f);
                    // val[2] (misal tombol/z) diabaikan, tambah sendiri kalau perlu
                }
                else
                {
                    Debug.LogWarning("[ESP32Input] Parse gagal: '" + val[0] + "', '" + val[1] + "'");
                }
            }
            else
            {
                Debug.LogWarning("[ESP32Input] Data kurang dari 2 nilai | data: " + data);
            }
        }
        catch (TimeoutException) { /* normal, abaikan */ }
        catch (Exception e)
        {
            Debug.LogWarning("[ESP32Input] Read error: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }

    [ContextMenu("Test Input (0.5, 0.5)")]
    void TestInput()
    {
        horizontal = 0.5f;
        vertical   = 0.5f;
    }
}