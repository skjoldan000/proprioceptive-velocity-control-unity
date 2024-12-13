using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UXF;
using System.Globalization;


public class ArduinoDualReciever : MonoBehaviour {
    // Serial Port and Thread for MPU Arduino
    private SerialPort mpuSerialPort;
    private Thread mpuSerialThread;
    private ConcurrentQueue<string> mpuSerialQueue = new ConcurrentQueue<string>(); // Thread-safe queue for MPU
    
    // Serial Port and Thread for Audio Arduino
    private SerialPort audioSerialPort;
    private Thread audioSerialThread;
    private ConcurrentQueue<string> audioSerialQueue = new ConcurrentQueue<string>(); // Thread-safe queue for Audio

    [SerializeField] private bool isRunning = false;
    [SerializeField] private string mpuCOMPort = "COM20";
    [SerializeField] private string audioCOMPort = "COM21";
    [SerializeField] private int Baudrate = 500000;
    [SerializeField] private int MPU6050_ACCEL_FS = 8;
    private float accelerationTranslation = 1;
    private UXFDataTable currentMPUTrialDataTable;
    private UXFDataTable currentAudioTrialDataTable;
    public bool saving = false;
    
    // MPU Variables
    private float ax;
    private float ay;
    private float az;
    private int mpuMicros;
    private int mpuSignalAmplitude;
    private int mpuFrequency;

    // Audio Variables
    private int audioSignalAmplitude;
    private int audioMicros;
    private float audioFrequency;

    // Timing variables
    private int linesToProcess = 100;
    public double pcSendTime = 0;

    void Start() {
        accelerationTranslation = 32767 / (MPU6050_ACCEL_FS / 2);

        
        // Initialize Serial Ports
        // InitSerialPort(ref mpuSerialPort, mpuCOMPort, ref mpuSerialThread, ReadMPUSerialData);
        InitSerialPort(ref audioSerialPort, audioCOMPort, ref audioSerialThread, ReadAudioSerialData);
    }
    
    void InitSerialPort(ref SerialPort serialPort, string COMPort, ref Thread serialThread, ThreadStart threadStart)
    {
        try
        {
            // Initialize the SerialPort object
            serialPort = new SerialPort(COMPort, Baudrate);
            serialPort.ReadTimeout = 10; // Prevent blocking

            // Attempt to open the port to trigger a reset
            serialPort.Open();
            Debug.Log($"Serial port {COMPort} opened for reset.");
            System.Threading.Thread.Sleep(100); 
            serialPort.Close();
            Debug.Log($"Serial port {COMPort} closed to complete reset.");

            // Wait to allow the Arduino to reinitialize
            System.Threading.Thread.Sleep(1000); 

            // Reopen the port for normal operations
            serialPort.Open();
            Debug.Log($"Serial port {COMPort} reopened for communication.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error initializing serial port {COMPort}: {ex.Message}");
            isRunning = false;
            return; // Exit if there was an error
        }

        // Start the serial reading thread if the port is successfully opened
        if (serialPort.IsOpen)
        {
            serialThread = new Thread(threadStart);
            serialThread.Start();
            isRunning = true;
            Debug.Log($"Serial port {COMPort} opened and thread started successfully.");
        }
        else
        {
            isRunning = false;
            Debug.LogWarning($"Port not opened {COMPort}. Setting isRunning to {isRunning}.");
        }
    }


    void Update() {
        if (isRunning)
        {
            if (saving)
            {
                // ProcessQueueData(mpuSerialQueue, linesToProcess, ParseAndProcessMPUData);
                ProcessQueueData(audioSerialQueue, linesToProcess, ParseAndProcessAudioData);
            }
        }
    }

    void ProcessQueueData(ConcurrentQueue<string> queue, int linesToProcess, System.Action<string> parseMethod)
    {
        for (int i = 0; i < linesToProcess; i++) {
            if (queue.TryDequeue(out string data)) {
                parseMethod(data);
            } else {
                break; // No more data to process
            }
        }
    }

    public void InitTrialDataFrame(Trial trial)
    {
        if (isRunning)
        {
            // var mpuHeaders = new string[]{ "time", "frameOffset", "zeroedTime", "ax", "ay", "az", "vibrationOn", "arduinoRecievedTime", "micros" };
            // currentMPUTrialDataTable = new UXFDataTable(mpuHeaders);
            // Debug.Log($"MPU Initialized data table for trial {trial.number}");

            var audioHeaders = new string[]{ "time", "frameOffset", "zeroedTime", "signalAmplitude", "vibrationOn", "arduinoRecievedTime", "micros" };
            currentAudioTrialDataTable = new UXFDataTable(audioHeaders);
            Debug.Log($"Audio Initialized data table for trial {trial.number}");
        }
    }

    void ReadMPUSerialData() {
        while (isRunning) {
            ReadSerialData(mpuSerialPort, mpuSerialQueue);
        }
    }

    void ReadAudioSerialData() {
        while (isRunning) {
            ReadSerialData(audioSerialPort, audioSerialQueue);
        }
    }

    void ReadSerialData(SerialPort serialPort, ConcurrentQueue<string> queue) {
        try {
            if (serialPort.BytesToRead > 0) { // Ensure there is data to read
                string data = serialPort.ReadLine();
                double unityFrameTime = FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds;
                string unityFrameTimeString = unityFrameTime.ToString(System.Globalization.CultureInfo.InvariantCulture);

                string timedData = $"{unityFrameTimeString},{data}";
                queue.Enqueue(timedData);
            }
        } catch (System.Exception ex) {
            Debug.LogWarning("Serial read error: " + ex.Message);
        }
    }

    public void ParseAndProcessMPUData(string data)
    {
        try
        {
            string[] parsedParts = data.Split(',');
            if (parsedParts.Length == 8)
            {
                float frameOffset = float.Parse(parsedParts[0], System.Globalization.CultureInfo.InvariantCulture);
                float unitytime = Time.time;
                ax = float.Parse(parsedParts[1]) / 32768.0f * MPU6050_ACCEL_FS;
                ay = float.Parse(parsedParts[2]) / 32768.0f * MPU6050_ACCEL_FS;
                az = float.Parse(parsedParts[3]) / 32768.0f * MPU6050_ACCEL_FS;
                int vibrationOn = int.Parse(parsedParts[4]);
                int zeroedTime = int.Parse(parsedParts[5]);
                int arduinoRecievedTime = int.Parse(parsedParts[6]);
                int micros = int.Parse(parsedParts[7]);

                var dataRow = new UXF.UXFDataRow();
                dataRow.Add(("time", unitytime));
                dataRow.Add(("frameOffset", frameOffset));
                dataRow.Add(("zeroedTime", zeroedTime));
                dataRow.Add(("ax", ax));
                dataRow.Add(("ay", ay));
                dataRow.Add(("az", az));
                dataRow.Add(("vibrationOn", vibrationOn));
                dataRow.Add(("arduinoRecievedTime", arduinoRecievedTime));
                dataRow.Add(("micros", micros));

                currentMPUTrialDataTable.AddCompleteRow(dataRow);
            }
            else
            {
                Debug.LogWarning("Invalid MPU data format: " + data);
            }
        }
        catch (System.Exception ex)
        {
            if (saving)
            {
                Debug.LogError($"Error parsing MPU data: {ex.Message}, data: {data}");
            }
        }
    }

    public void ParseAndProcessAudioData(string data)
    {
        try
        {
            string[] parsedParts = data.Split(',');
            if (parsedParts.Length == 6)
            {
                float frameOffset = float.Parse(parsedParts[0], System.Globalization.CultureInfo.InvariantCulture);
                float unitytime = Time.time;
                audioSignalAmplitude = int.Parse(parsedParts[1]);
                int vibrationOn = int.Parse(parsedParts[2]);
                int zeroedTime = int.Parse(parsedParts[3]);
                int arduinoRecievedTime = int.Parse(parsedParts[4]);
                int micros = int.Parse(parsedParts[5]);

                var dataRow = new UXF.UXFDataRow();
                dataRow.Add(("time", unitytime));
                dataRow.Add(("frameOffset", frameOffset));
                dataRow.Add(("zeroedTime", zeroedTime));
                dataRow.Add(("signalAmplitude", audioSignalAmplitude));
                dataRow.Add(("vibrationOn", vibrationOn));
                dataRow.Add(("arduinoRecievedTime", arduinoRecievedTime));
                dataRow.Add(("micros", micros));

                currentAudioTrialDataTable.AddCompleteRow(dataRow);
            }
            else
            {
                Debug.LogWarning("Invalid Audio data format: " + data);
            }
        }
        catch (System.Exception ex)
        {
            if (saving)
            {
                Debug.LogError($"Error parsing MPU data: {ex.Message}, data: {data}");
            }
        }
    }

    public void SaveDataFrame(Trial trial)
    {
        try
        {
            // trial.SaveDataTable(currentMPUTrialDataTable, "mpu_data");
            // Debug.Log($"MPU Data saved for trial {trial.number}");

            trial.SaveDataTable(currentAudioTrialDataTable, "audio_data");
            Debug.Log($"Audio Data saved for trial {trial.number}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error saving trial data: " + ex.Message);
        }
    }

    void OnApplicationQuit() {
        // Stop threads and close serial ports
        isRunning = false;
        // StopThread(ref mpuSerialThread);
        StopThread(ref audioSerialThread);
        // CloseSerialPort(ref mpuSerialPort);
        CloseSerialPort(ref audioSerialPort);
        Debug.Log("Serial ports closed and threads stopped.");
    }

    void StopThread(ref Thread thread)
    {
        if (thread != null && thread.IsAlive) {
            thread.Join(); // Wait for thread to finish
        }
    }

    void CloseSerialPort(ref SerialPort serialPort)
    {
        if (serialPort != null && serialPort.IsOpen) {
            serialPort.Close();
        }
    }

    public void ResetSerialQueue()
    {
        if (isRunning)
        {
            // mpuSerialQueue = new ConcurrentQueue<string>();
            audioSerialQueue = new ConcurrentQueue<string>();
            Debug.Log("Serial queues reinitialized.");
        }
    }

    public void SendTrigger(string value)
    {
        if (isRunning)
        {
            if (mpuSerialPort.IsOpen && audioSerialPort.IsOpen) 
            {
                if (value == "true" | value == "false" | value == "zerotimer") {
                    string message = $"{value}\n"; // Append newline character for Arduino's readStringUntil
                    // mpuSerialPort.WriteLine(message);
                    audioSerialPort.WriteLine(message);
                }
                else {
                    Debug.LogError("serial port string send is invalid");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Serial port is not open!");
            }
        }
    }
}
