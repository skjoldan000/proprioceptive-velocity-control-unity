using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;
using UXF;
using System.Globalization;

public class ArduinoDualReceiver : MonoBehaviour {
    // Serial Ports and Threads for MPU and Audio Arduinos
    private SerialPort mpuSerialPort;
    private Thread mpuSerialThread;
    private ConcurrentQueue<string> mpuSerialQueue = new ConcurrentQueue<string>(); // Thread-safe queue for MPU

    private SerialPort audioSerialPort;
    private Thread audioSerialThread;
    private ConcurrentQueue<string> audioSerialQueue = new ConcurrentQueue<string>(); // Thread-safe queue for Audio

    [SerializeField] private bool isRunning = false;
    [SerializeField] private bool useMPU = false;
    [SerializeField] private bool useAudio = true;
    [SerializeField] private string mpuCOMPort = "COM20";
    [SerializeField] private string audioCOMPort = "COM22";
    [SerializeField] private int Baudrate = 1000000;
    [SerializeField] private int MPU6050_ACCEL_FS = 8;
    private float accelerationTranslation;
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

        if (useMPU)
        {
            InitSerialPort(ref mpuSerialPort, mpuCOMPort, ref mpuSerialThread, ReadMPUSerialData);
        }

        if (useAudio)
        {
            InitSerialPort(ref audioSerialPort, audioCOMPort, ref audioSerialThread, ReadAudioSerialData);
        }
    }

    void InitSerialPort(ref SerialPort serialPort, string COMPort, ref Thread serialThread, ThreadStart threadStart)
    {
        try
        {
            serialPort = new SerialPort(COMPort, Baudrate);
            serialPort.ReadTimeout = 10;

            // Reset the Arduino
            serialPort.Open();
            Debug.Log($"Serial port {COMPort} opened for reset.");
            Thread.Sleep(100);
            serialPort.Close();
            Debug.Log($"Serial port {COMPort} closed to complete reset.");

            Thread.Sleep(1000); // Wait for Arduino to reset

            serialPort.Open();
            Debug.Log($"Serial port {COMPort} reopened for communication.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error initializing serial port {COMPort}: {ex.Message}");
            isRunning = false;
            return;
        }

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
        if (isRunning && saving)
        {
            if (useMPU)
            {
                ProcessQueueData(mpuSerialQueue, linesToProcess, ParseAndProcessMPUData);
            }

            if (useAudio)
            {
                ProcessQueueData(audioSerialQueue, linesToProcess, ParseAndProcessAudioData);
            }
        }
        //Debug.Log($"aud: {audioSignalAmplitude}");
        //Debug.Log($"saving: {saving}");
    }

    void ProcessQueueData(ConcurrentQueue<string> queue, int linesToProcess, System.Action<string> parseMethod)
    {
        for (int i = 0; i < linesToProcess; i++) {
            if (queue.TryDequeue(out string data)) {
                parseMethod(data);
            } else {
                break;
            }
        }
    }

    public void InitTrialDataFrame(Trial trial)
    {
        if (isRunning)
        {
            if (useMPU)
            {
                var mpuHeaders = new string[]{ "time", "frameOffset", "zeroedTime", "ax", "ay", "az", "vibrationOn", "arduinoRecievedTime", "micros" };
                currentMPUTrialDataTable = new UXFDataTable(mpuHeaders);
                Debug.Log($"MPU Initialized data table for trial {trial.number}");
            }

            if (useAudio)
            {
                var audioHeaders = new string[]{ "time", "frameOffset", "zeroedTime", "ax", "ay", "az", "signalAmplitude", "vibrationOn", "arduinoRecievedTime", "micros" };
                currentAudioTrialDataTable = new UXFDataTable(audioHeaders);
                Debug.Log($"Audio Initialized data table for trial {trial.number}");
            }
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
            if (serialPort.BytesToRead > 0) {
                string data = serialPort.ReadLine();
                double unityFrameTime = FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds;
                string unityFrameTimeString = unityFrameTime.ToString(CultureInfo.InvariantCulture);

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
                float frameOffset = float.Parse(parsedParts[0], CultureInfo.InvariantCulture);
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
            if (parsedParts.Length == 9)
            {
                float frameOffset = float.Parse(parsedParts[0], CultureInfo.InvariantCulture);
                float unitytime = Time.time;
                ax = float.Parse(parsedParts[1]) / 32768.0f * MPU6050_ACCEL_FS;
                ay = float.Parse(parsedParts[2]) / 32768.0f * MPU6050_ACCEL_FS;
                az = float.Parse(parsedParts[3]) / 32768.0f * MPU6050_ACCEL_FS;
                audioSignalAmplitude = int.Parse(parsedParts[4]);
                int vibrationOn = int.Parse(parsedParts[5]);
                int zeroedTime = int.Parse(parsedParts[6]);
                int arduinoRecievedTime = int.Parse(parsedParts[7]);
                int micros = int.Parse(parsedParts[8]);

                var dataRow = new UXF.UXFDataRow();
                dataRow.Add(("time", unitytime));
                dataRow.Add(("frameOffset", frameOffset));
                dataRow.Add(("zeroedTime", zeroedTime));
                dataRow.Add(("ax", ax));
                dataRow.Add(("ay", ay));
                dataRow.Add(("az", az));
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
                Debug.LogError($"Error parsing Audio data: {ex.Message}, data: {data}");
            }
        }
    }

    public void SaveDataFrame(Trial trial)
    {
        try
        {
            if (useMPU)
            {
                trial.SaveDataTable(currentMPUTrialDataTable, "mpu_data");
                Debug.Log($"MPU Data saved for trial {trial.number}");
            }

            if (useAudio)
            {
                trial.SaveDataTable(currentAudioTrialDataTable, "audio_data");
                Debug.Log($"Audio Data saved for trial {trial.number}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error saving trial data: " + ex.Message);
        }
    }

    void OnApplicationQuit() {
        isRunning = false;

        if (useMPU)
        {
            StopThread(ref mpuSerialThread);
            CloseSerialPort(ref mpuSerialPort);
        }

        if (useAudio)
        {
            StopThread(ref audioSerialThread);
            CloseSerialPort(ref audioSerialPort);
        }

        Debug.Log("Serial ports closed and threads stopped.");
    }

    void StopThread(ref Thread thread)
    {
        if (thread != null && thread.IsAlive) {
            thread.Join();
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
            if (useMPU)
            {
                mpuSerialQueue = new ConcurrentQueue<string>();
            }

            if (useAudio)
            {
                audioSerialQueue = new ConcurrentQueue<string>();
            }

            Debug.Log("Serial queues reinitialized.");
        }
    }

    public void SendTrigger(string value)
    {
        if (isRunning)
        {
            if (value == "true" || value == "false" || value == "zerotimer") {
                string message = $"{value}\n";

                if (useMPU && mpuSerialPort.IsOpen)
                {
                    mpuSerialPort.WriteLine(message);
                }

                if (useAudio && audioSerialPort.IsOpen)
                {
                    audioSerialPort.WriteLine(message);
                }
            }
            else {
                Debug.LogError("Serial port string send is invalid");
            }
        }
    }
}
