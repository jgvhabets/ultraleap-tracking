using Leap;
using Leap.Unity;
using UnityEngine;
using LSL;

public class log_hand_position : MonoBehaviour
{

    /* class to log the position of hands that are detected in the scene
    OnUpdateFrame() is called as soon as hand is detected in the scene and will then (as specified in the options)
    write hand position to the log file and also stream information to the LSL stream
    (c) Johannes Achtzehn, 07|2022, Charite - Universitaetsmedizin Berlin, johannes.achtzehn@charite.de
    */

    // Input options
    [Header("Logfile options")]
    [SerializeField] public bool write_logs = true;                     // write nan if hand is not found?
    [SerializeField] public bool write_nan = true;                      // write nan if hand is not found?

    [Header("LSL configuration")]
    [SerializeField] public bool enable_lsl_streaming = true;           // push data to LSL?
    [SerializeField] public string StreamName = "Unity.ExampleStream";  // name of the stream
    [SerializeField] public string StreamType = "Unity.StreamType";     // type (currently matlab uses this string to find stream)
    [SerializeField] public string StreamId = "MyStreamID-Unity1234";   // stream ID
    [SerializeField] public int lsl_channels = 0;                       // how many channels are to be streamed

    [Header("Inputs")]
    public LeapServiceProvider LeapServiceProvider;                     // this provides us with the data from Ultraleap tracking software

    // Global var definition
    private StreamOutlet outlet;        // outlet for LSL
    private float[] currentSample;      // outlet variable for LSL (what will be pushed)

    // this function is called before the first frame, use this to initialze stuff
    void Start()
    {
        StreamInfo streamInfo = new StreamInfo(StreamName, StreamType, lsl_channels, Time.fixedDeltaTime * 1000, LSL.channel_format_t.cf_float32);
        XMLElement chans = streamInfo.desc().append_child("channels");
        chans.append_child("channel").append_child_value("label", "x_lh");
        chans.append_child("channel").append_child_value("label", "y_lh");
        chans.append_child("channel").append_child_value("label", "z_lh");
        chans.append_child("channel").append_child_value("label", "x_rh");
        chans.append_child("channel").append_child_value("label", "y_rh");
        chans.append_child("channel").append_child_value("label", "z_rh");
        chans.append_child("channel").append_child_value("label", "dist_lh");
        chans.append_child("channel").append_child_value("label", "dist_rh");
        outlet = new StreamOutlet(streamInfo);
        currentSample = new float[lsl_channels];
    }

    // this function calles the function OnUpdateFrame as soon as a hand is detected
    private void OnEnable()
    {
        LeapServiceProvider.OnUpdateFrame += OnUpdateFrame;
    }

    // this function removes the function OnUpdateFrame as soon as a hand is detected
    private void OnDisable()
    {
        LeapServiceProvider.OnUpdateFrame -= OnUpdateFrame;
    }

    // this function writes information about hand position to the log file
    private void write_handposition_to_logfile(Frame frame)
    {

        foreach (var hand in frame.Hands)   // Get a list of all the hands in the frame and loop through
        {

            string position_string = Time.deltaTime.ToString("0.0000") + ",";  // get the time it took for the current frame

            // iterate over each finger
            foreach (var finger in hand.Fingers)
            {
                // iterate over each segment of each finger and add position to the string
                for (int j = 0; j < 4; j++)
                {
                    Vector3 position = finger.Bone((Bone.BoneType)j).NextJoint.ToVector3();
                    position_string += position.x.ToString("0.0000") + ",";
                    position_string += position.y.ToString("0.0000") + ",";
                    position_string += position.z.ToString("0.0000") + ",";
                }

            }

            // palm
            Vector3 palmPosition = hand.PalmPosition.ToVector3();
            position_string += palmPosition.x.ToString("0.0000") + ",";
            position_string += palmPosition.y.ToString("0.0000") + ",";
            position_string += palmPosition.z.ToString("0.0000") + ",";

            // get some additional information
            // pinching
            string pinchStrength = hand.PinchStrength.ToString("0.0000");
            string pinchDistance = hand.PinchDistance.ToString("0.0000");
            Vector3 pinchPosition = hand.GetPinchPosition();
            Vector3 predictedPinchPosition = hand.GetPredictedPinchPosition();

            string isPinching = "";
            if (hand.IsPinching())
            {
                isPinching = "1";
            }
            else
            {
                isPinching = "0";
            }

            // grabbing
            string grabStrength = hand.GrabStrength.ToString("0.00");

            // misc
            string confidence = hand.Confidence.ToString("0.00");

            // write additional info to string
            position_string += pinchPosition.x.ToString("0.0000") + "," + pinchPosition.y.ToString("0.0000") + "," + pinchPosition.z.ToString("0.0000") +
                "," + predictedPinchPosition.x.ToString("0.0000") + "," + predictedPinchPosition.y.ToString("0.0000") + "," + predictedPinchPosition.z.ToString("0.0000") +
                "," + isPinching + "," + pinchStrength + "," + pinchDistance + ",";
            position_string += grabStrength + "," + confidence;

            // write to log file
            if (hand.IsLeft)
            {
                logfile_handler.pos_file_lh.write(position_string);

                // if nan are to be written out if a hand is not visible, write nan for the other hand here
                if (write_nan && frame.Hands.Count == 1)
                {
                    logfile_handler.pos_file_rh.write(Time.deltaTime.ToString("0.0000") + "," + main.nan_string);
                }

            }
            else if (hand.IsRight)
            {
                logfile_handler.pos_file_rh.write(position_string);

                // if nan are to be written out if a hand is not visible, write nan for the other hand here
                if (write_nan && frame.Hands.Count == 1)
                {
                    logfile_handler.pos_file_lh.write(Time.deltaTime.ToString("0.0000") + "," + main.nan_string);
                }
            }

        }
    }

    // this function pushes information about palm position and pinch distance to the LSL stream
    private void stream_to_lsl(Frame frame)
    {

        //Get a list of all the hands in the frame and loop through
        foreach (var hand in frame.Hands)
        {
            // palm position
            Vector3 palmPosition = hand.PalmPosition.ToVector3();

            // pinch distance
            float pinchDistance = hand.PinchDistance;

            if (hand.IsLeft)
            {
                currentSample[0] = palmPosition.x;
                currentSample[1] = palmPosition.y;
                currentSample[2] = palmPosition.z;
                currentSample[6] = pinchDistance;
            }
            else if (hand.IsRight)
            {
                currentSample[3] = palmPosition.x;
                currentSample[4] = palmPosition.y;
                currentSample[5] = palmPosition.z;
                currentSample[7] = pinchDistance;
            }
        }

        outlet.push_sample(currentSample);  // finally push the vector to the LSL stream

    }
        // this function is called every frame that is created by UltraLeap
    void OnUpdateFrame(Frame frame)
    {

        // write hand positions to log file
        if (write_logs) {    
            if (frame.Hands.Count > 0)
            {
                write_handposition_to_logfile(frame);
            }
            else if (write_nan)
            {
                logfile_handler.pos_file_lh.write(Time.deltaTime.ToString("0.0000") + "," + main.nan_string);
                logfile_handler.pos_file_rh.write(Time.deltaTime.ToString("0.0000") + "," + main.nan_string);
            }
        }

        // stream to LSL ui
        if (enable_lsl_streaming) 
        {
            currentSample = new float[lsl_channels];
            if (frame.Hands.Count > 0)
            {
                stream_to_lsl(frame);
            }
            else
            {
                outlet.push_sample(currentSample);
            }

        }
    }
}