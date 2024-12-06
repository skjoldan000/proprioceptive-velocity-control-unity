using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UXF;
using System.Globalization;

public class ArduinoReciever : MonoBehaviour {
    private SerialPort serialPortMPU;
    private Thread serialThreadMPU;
    private ConcurrentQueue<string> serialQueueMPU = new ConcurrentQueue<string>();
    private SerialPort serialPortMPUAudio;
    private Thread serialThreadAudio;
    private ConcurrentQueue<string> serialQueueAudio = new ConcurrentQueue<string>(); 
    [SerializeField] private bool isRunning = false;
    [SerializeField] private string COMportMPU = "COM20";
    [SerializeField] private string COMportAudio = "COM21";
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
    private string[] parsedPartsMPU = new string[6]; // Fixed size for 6 fields

    // List to store trial data
    private List<string> trialData = new List<string>();

    void Start() {
        accelerationTranslation = 32767 / (MPU6050_ACCEL_FS / 2);

        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatchOffset = (Time.time - (stopwatch.ElapsedMilliseconds/1000f)); 
        // Configure and open the serial port
        try
        {
            serialPortMPU = new serialPortMPU(COMportMPU, Baudrate);
            serialPortMPU.ReadTimeout = 10; // Prevent blocking
            serialPortMPU.Open();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error opening serial port {COMportMPU}: {ex.Message}");
        }

        try
        {
            serialPortAudio = new serialPortMPU(COMportMPU, Baudrate);
            serialPortAudio.ReadTimeout = 10; // Prevent blocking
            serialPortAudio.Open();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error opening serial port {COMportAudio}: {ex.Message}");
        
        // Start the serial reading thread

        if (serialPortMPU.IsOpen)
        {
            serialThreadMPU = new Thread(ReadSerialData);
            serialThreadMPU.Start();
            isRunning = true;
            Debug.Log($"Serial port {serialPortMPU} opened and thread started.");
        }
        else{
            isRunning = false;
            Debug.LogWarning($"Port not openned {COMportMPU}. Setting isRunning to {isRunning}");
        }
        if (serialPortAudio.IsOpen)
        {
            serialThreadAudio = new Thread(ReadSerialData);
            serialThreadAudio.Start();
            isRunning = true;
            Debug.Log($"Serial port {serialPortAudio} opened and thread started.");
        }
        else{
            isRunning = false;
            Debug.LogWarning($"Port not openned {COMportAudio}. Setting isRunning to {isRunning}");
        }
        if (!isRunning)
        {
            Debug.LogError("At least 1 COM port not properly opened.");
        }
        

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
                if (serialQueueMPU.Count > 15)
                {
                    Debug.LogWarning($"len of queue: {serialQueueMPU.Count}");
                }
                //Debug.Log("parsing and saving data...");
                for (int i = 0; i < linesToProcess; i++) {
                    if (serialQueueMPU.TryDequeue(out string data)) {
                        ParseAndProcessData(data);
                    } else {
                        break; // No more data to process
                    }
                }
                for (int i = 0; i < linesToProcess; i++) {
                    if (serialQueueMPU.TryDequeue(out string data)) {
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
                ResetserialQueueMPU();
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
                if (serialPortMPU.BytesToRead > 0) { // Ensure there is data to read
                    string data = serialPortMPU.ReadLine(); // Read incoming serial data
                    double unityFrameTime = FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds;
                    string unityFrameTimeString = unityFrameTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    // Append timestamp to the data
                    string timedData = $"{unityFrameTimeString},{data}";
                    serialQueueMPU.Enqueue(timedData);
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
            string[] parsedPartsMPU = data.Split(',');

            if (parsedPartsMPU.Length == 7)
            {
                // Parse the data
                float unityFrameTime = float.Parse(parsedPartsMPU[0], System.Globalization.CultureInfo.InvariantCulture);
                float unitytime = Time.time;
                ax = float.Parse(parsedPartsMPU[1]) / 32768.0f * MPU6050_ACCEL_FS; // Accelerometer X
                ay = float.Parse(parsedPartsMPU[2]) / 32768.0f * MPU6050_ACCEL_FS; // Y-axis
                az = float.Parse(parsedPartsMPU[3]) / 32768.0f * MPU6050_ACCEL_FS; // Z-axis
                //ax = float.Parse(parsedPartsMPU[1]); // Accelerometer X
                //ay = float.Parse(parsedPartsMPU[2]); // Y-axis
                //az = float.Parse(parsedPartsMPU[3]); // Z-axis
                signalAmplitude = int.Parse(parsedPartsMPU[4]); // Signal amplitude
                micros = int.Parse(parsedPartsMPU[5]); // Current time in µs
                int vibrationOn = int.Parse(parsedPartsMPU[6]); // vibrationTriggered
                frequency = int.Parse(parsedPartsMPU[7]); // Frequency in Hz
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
        if (serialThreadMPU != null && serialThreadMPU.IsAlive) {
            serialThreadMPU.Join(); // Wait for thread to finish
        }
        if (serialPortMPU != null && serialPortMPU.IsOpen) {
            serialPortMPU.Close();
        }
        Debug.Log("Serial port closed and thread stopped.");
    }
    public void ResetserialQueueMPU()
    {
        if (isRunning)
        {
            serialQueueMPU = new ConcurrentQueue<string>();
            Debug.Log("Serial queue reinitialized.");
        }
    }
    public void SendTrigger(string value)
    {
        if (isRunning)
        {
            if (serialPortMPU.IsOpen)
            {
                string message = $"{value}\n"; // Append newline character for Arduino's readStringUntil
                serialPortMPU.WriteLine(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Serial port is not open!");
            }
            if (serialPortAudio.IsOpen)
            {
                string message = $"{value}\n"; // Append newline character for Arduino's readStringUntil
                serialPortAudio.WriteLine(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Serial port is not open!");
            }
            
        }
    }
}

