using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ESP32Input : MonoBehaviour
{
    [Header("Serial Settings")]
    [SerializeField] private string portName = "COM5";
    [SerializeField] private int baudRate = 115200;

    [Header("Joystick Settings")]
    [Range(0.1f, 1f)] public float joystickSensitivity = 0.15f; // Rekomendasi: 0.15f agar tidak terlalu sensitif
    [Range(0.1f, 5f)] public float joystickMoveMultiplier = 1f;

    [Header("Live Values")]
    public float horizontal;
    public float vertical;

    [Header("Button")]
    public bool selectPressed;      // One-shot: true hanya 1 frame saat ditekan
    public bool selectHeld;         // True selama tombol ditekan

    [Header("Connection")]
    public bool isConnected = false;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;

    // Variabel thread-safe untuk menampung data dari background thread
    private string latestSerialData = "";
    private readonly object dataLock = new object();

    private bool lastButtonState = false;

    void Start()
    {
        // Pastikan port ditutup dulu jika sebelumnya masih menggantung
        CloseSerial();

        serialPort = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 50,
            WriteTimeout = 50,
            NewLine = "\n"
        };

        try
        {
            serialPort.Open();
            isConnected = true;
            Debug.Log($"[ESP32Input] Connected to {portName}");

            // Memulai Background Thread untuk membaca data serial
            isRunning = true;
            readThread = new Thread(ReadSerialLoop);
            readThread.Start();
        }
        catch (Exception e)
        {
            isConnected = false;
            Debug.LogError("[ESP32Input] Serial Error: " + e.Message);
        }
    }

    void Update()
    {
        // 1. Reset one-shot button tiap frame awal
        selectPressed = false;

        if (!isConnected) return;

        string dataToParse = "";

        // 2. Ambil data terbaru dari Thread secara aman (Thread-safe)
        lock (dataLock)
        {
            dataToParse = latestSerialData;
        }

        if (string.IsNullOrEmpty(dataToParse)) return;

        // 3. Proses Parsing Data di Main Thread Unity
        try
        {
            string[] val = dataToParse.Split(',');

            if (val.Length >= 3)
            {
                bool okX = float.TryParse(val[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
                bool okY = float.TryParse(val[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
                bool okButton = int.TryParse(val[2].Trim(), out int btn);

                // --- JOYSTICK HANDLING ---
                if (okX && okY)
                {
                    // Deadzone / Sensitivity check
                    if (Mathf.Abs(x) < joystickSensitivity) x = 0;
                    if (Mathf.Abs(y) < joystickSensitivity) y = 0;

                    horizontal = Mathf.Clamp(x * joystickMoveMultiplier, -1f, 1f);
                    vertical = Mathf.Clamp(y * joystickMoveMultiplier, -1f, 1f);
                }

                // --- BUTTON HANDLING ---
                if (okButton)
                {
                    bool currentState = (btn == 1);

                    // Deteksi One-Shot (Hanya true di frame pertama ditekan)
                    if (currentState && !lastButtonState)
                    {
                        selectPressed = true;
                    }

                    selectHeld = currentState;
                    lastButtonState = currentState;
                }
            }
        }
        catch (Exception e)
        {
            // Menyembunyikan eror parsing ringan saat awal boot up ESP32
            // Debug.LogWarning("[ESP32Input] Parse Error: " + e.Message);
        }
    }

    // Loop ini berjalan di background, terpisah dari frame rate Unity
    private void ReadSerialLoop()
    {
        while (isRunning && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine();
                if (!string.IsNullOrEmpty(data))
                {
                    // Kirim data ke variabel utama menggunakan lock
                    lock (dataLock)
                    {
                        latestSerialData = data.Trim().Replace("\r", "").Replace("\n", "");
                    }
                }
            }
            catch (TimeoutException)
            {
                // Timeout normal terjadi jika ESP32 belum mengirim data baru
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ESP32Input] Thread Read Error: " + e.Message);
                Thread.Sleep(10); // Istirahat sejenak jika ada error keras
            }
        }
    }

    private void CloseSerial()
    {
        isRunning = false;

        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(100); // Tunggu thread selesai maksimal 100ms
        }

        if (serialPort != null)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.Dispose();
            serialPort = null;
        }
        isConnected = false;
    }

    void OnApplicationQuit()
    {
        CloseSerial();
    }

    void OnDisable()
    {
        CloseSerial();
    }
}