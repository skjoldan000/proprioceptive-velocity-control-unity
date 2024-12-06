using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UXF;
using System.Globalization;

public class ArduinoReciever : MonoBehaviour {
    private SerialPort serialPort;
    private Thread serialThread;
    private ConcurrentQueue<string> serialQueue = new ConcurrentQueue<string>(); // Thread-safe queue
    [SerializeField] private bool isRunning = false;
    [SerializeField] private string COMPort = "COM20";
    [SerializeField] private int Baudrate = 500000;
    [SerializeField] private int MPU6050_ACCEL_FS = 8;
    private float accelerationTranslation = 1;
    private UXFDataTable currentTrialDataTable;
    public bool saving = false;
    private float ax;
    private float ay;
    private float az;
    private int signalAmplitude;
    private int micros;
    private int frequency;
    public float alignedTime;
    private float unityTime;
    private System.Diagnostics.Stopwatch stopwatch; 
    public float alignedStopwatch;
    private float stopwatchOffset;
    private string alignedTimeString;
    public bool offsetApplied = false;
    private int linesToProcess = 100;


    // Preallocate parsing memory
    private string[] parsedParts = new string[6]; // Fixed size for 6 fields

    // List to store trial data
    private List<string> trialData = new List<string>();

    void Start() {
        accelerationTranslation = 32767 / (MPU6050_ACCEL_FS / 2);

        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatchOffset = (Time.time - (stopwatch.ElapsedMilliseconds/1000f)); 
        // Configure and open the serial port
        try
        {
            serialPort = new SerialPort(COMPort, Baudrate);
            serialPort.ReadTimeout = 10; // Prevent blocking
            serialPort.Open();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error opening serial port {COMPort}: {ex.Message}");
        }

        // Start the serial reading thread

        if (serialPort.IsOpen)
        {
            serialThread = new Thread(ReadSerialData);
            serialThread.Start();
            isRunning = true;
            Debug.Log("Serial port opened and thread started.");
        }
        else{
            isRunning = false;
            Debug.LogWarning($"Port not openned {COMPort}. Setting isRunning to {isRunning}");
        }


        //Debug.Log($"timing: stopwatch: {stopwatch.ElapsedMilliseconds}, offset: {stopwatchOffset}, offsetted: {(stopwatch.ElapsedMilliseconds/1000f) + stopwatchOffset}");
    }
    
    public void AlignStopwatch()
    {
        stopwatchOffset = (Time.time - (stopwatch.ElapsedMilliseconds/1000f));
    }

    void Update() {
        if (isRunning)
        {
            alignedStopwatch = (stopwatch.ElapsedMilliseconds/1000f) + stopwatchOffset;
            if (saving)
            {
                if (serialQueue.Count > 15)
                {
                    Debug.LogWarning($"len of queue: {serialQueue.Count}");
                }
                //Debug.Log("parsing and saving data...");
                for (int i = 0; i < linesToProcess; i++) {
                    if (serialQueue.TryDequeue(out string data)) {
                        ParseAndProcessData(data);
                    } else {
                        break; // No more data to process
                    }
                }
                //Debug.Log($"Data added to table: {alignedTime},{micros},{ax},{ay},{az},{signalAmplitude},{frequency}");
                //Debug.Log($"Audio detected: {signalAmplitude}");
            }
            //Debug.Log($"timing: unitytime{Time.time}, stopwatch: {(stopwatch.ElapsedMilliseconds/1000f)} stopwatch aligned: {(stopwatch.ElapsedMilliseconds/1000f) + stopwatchOffset}, diff: {Time.time - ((stopwatch.ElapsedMilliseconds/1000f) + stopwatchOffset)}");
            if (!offsetApplied)
            {
                stopwatchOffset = (Time.time - (stopwatch.ElapsedMilliseconds/1000f));
                ResetSerialQueue();
                offsetApplied = true;
            }
        }
    }
    public void InitTrialDataFrame(Trial trial)
    {
        if (isRunning)
        {
            // Initialize a new data table for the current trial
            var headers = new string[]{ "time", "stopwatch", "micros", "ax", "ay", "az", "signalAmplitude", "vibrationOn", "frequency" };
            currentTrialDataTable = new UXFDataTable(headers);
            Debug.Log($"Arduino Initialized data table for trial {trial.number}");
        }
    }

    void ReadSerialData() {
        // Thread for reading serial data
        while (isRunning) {
            try {
                if (serialPort.BytesToRead > 0) { // Ensure there is data to read
                    string data = serialPort.ReadLine(); // Read incoming serial data
                    //alignedTime = (stopwatch.ElapsedMilliseconds / 1000.0f) + stopwatchOffset;
                    //alignedTimeString = alignedTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    double unityFrameTime = FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds;
                    string unityFrameTimeString = unityFrameTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    // Append timestamp to the data
                    string timedData = $"{unityFrameTimeString},{data}";
                    serialQueue.Enqueue(timedData);
                    
                    //Debug.Log($"timing2: Raw received data: {data}");
                    //Debug.Log($"timing2: Formatted aligned time: {alignedTimeString}");
                    //Debug.Log($"timing2: full timed data string: {timedData}");

                }
            } catch (System.Exception ex) {
                Debug.LogWarning("Serial read error: " + ex.Message);
            }
        }
    }

    public void ParseAndProcessData(string data)
    {
        try
        {
            string[] parsedParts = data.Split(',');

            if (parsedParts.Length == 8)
            {
                // Parse the data
                float unityFrameTime = float.Parse(parsedParts[0], System.Globalization.CultureInfo.InvariantCulture);
                float unitytime = Time.time;
                ax = float.Parse(parsedParts[1]) / 32768.0f * MPU6050_ACCEL_FS; // Accelerometer X
                ay = float.Parse(parsedParts[2]) / 32768.0f * MPU6050_ACCEL_FS; // Y-axis
                az = float.Parse(parsedParts[3]) / 32768.0f * MPU6050_ACCEL_FS; // Z-axis
                //ax = float.Parse(parsedParts[1]); // Accelerometer X
                //ay = float.Parse(parsedParts[2]); // Y-axis
                //az = float.Parse(parsedParts[3]); // Z-axis
                signalAmplitude = int.Parse(parsedParts[4]); // Signal amplitude
                micros = int.Parse(parsedParts[5]); // Current time in µs
                int vibrationOn = int.Parse(parsedParts[6]); // vibrationTriggered
                frequency = int.Parse(parsedParts[7]); // Frequency in Hz
                //time = Time.time;

                // Add data to the trial's UXFDataTable
                var dataRow = new UXF.UXFDataRow();
                dataRow.Add(("time", unitytime));
                dataRow.Add(("stopwatch", unityFrameTime));
                dataRow.Add(("micros", micros));
                dataRow.Add(("ax", ax));
                dataRow.Add(("ay", ay));
                dataRow.Add(("az", az));
                dataRow.Add(("signalAmplitude", signalAmplitude));
                dataRow.Add(("vibrationOn", vibrationOn));
                dataRow.Add(("frequency", frequency));

                currentTrialDataTable.AddCompleteRow(dataRow);

                //Debug.Log($"Data added to table: {time},{micros},{ax},{ay},{az},{signalAmplitude},{frequency}");
            }
            else
            {
                Debug.LogWarning("Invalid data format: " + data);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error parsing data: " + ex.Message);
        }
    }

    public void SaveDataFrame(Trial trial)
    {
        try
        {
            // Save the data table for the current trial
            trial.SaveDataTable(currentTrialDataTable, "arduino_data");

            Debug.Log($"Arduino Data saved for trial {trial.number}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error saving trial data: " + ex.Message);
        }
    }

    void OnApplicationQuit() {
        // Stop the thread and close the serial port
        isRunning = false;
        if (serialThread != null && serialThread.IsAlive) {
            serialThread.Join(); // Wait for thread to finish
        }
        if (serialPort != null && serialPort.IsOpen) {
            serialPort.Close();
        }
        Debug.Log("Serial port closed and thread stopped.");
    }
    public void ResetSerialQueue()
    {
        if (isRunning)
        {
            serialQueue = new ConcurrentQueue<string>();
            Debug.Log("Serial queue reinitialized.");
        }
    }
    public void SendTrigger(bool value)
    {
        if (isRunning)
        {
            if (serialPort.IsOpen)
            {
                string message = value ? "TRUE\n" : "FALSE\n"; // Append newline character for Arduino's readStringUntil
                serialPort.WriteLine(message);
                //UnityEngine.Debug.Log($"Sent: {message}");
            }
            else
            {
                UnityEngine.Debug.LogWarning("Serial port is not open!");
            }
        }
    }
}

