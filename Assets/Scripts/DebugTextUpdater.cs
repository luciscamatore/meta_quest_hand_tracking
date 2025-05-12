using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Numerics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.Networking.PlayerConnection;
using Oculus.Interaction.Input;

[Serializable]
public class FingerCurl
{
    public float indexCurl;
    public float middleCurl;
    public float ringCurl;
    public float pinkyCurl;
}

[Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;
}
[Serializable]
public class HandData
{
    public Vector3Data leftHandPos;
    public Vector3Data leftHandRot;
    public FingerCurl leftFingerCurl;

    public Vector3Data rightHandPos;
    public Vector3Data rightHandRot;
    public FingerCurl rightFingerCurl;
}

public class DebugTextUpdater : MonoBehaviour
{
    //HAND DATA
    public TextMeshProUGUI handPositionText; // Reference to the TextMeshProUGUI component
    public TextMeshProUGUI fingerJointsText;
    public Transform leftHandTransform; // Reference to the left hand transform
    public Transform rightHandTransform; // Reference to the right hand transform
    public OVRHand leftHand;
    public OVRHand rightHand;
    public OVRSkeleton leftHandSkeleton;
    public OVRSkeleton rightHandSkeleton;
    UnityEngine.Vector3 leftHandPos;
    UnityEngine.Vector3 rightHandPos;
    UnityEngine.Vector3 leftHandRot;
    UnityEngine.Vector3 rightHandRot;

    //SERVER DATA
    private TcpClient client;
    private NetworkStream stream;
    private Thread connectThread;
    private bool isConnected = false;

    //FINGER CURL
    private const float maxFingerCount = 270f;
    private float left_indexCurl;
    private float left_middleCurl;
    private float left_ringCurl;
    private float left_pinkyCurl;
    private float right_indexCurlRight;
    private float right_middleCurlRight;
    private float right_ringCurlRight;
    private float right_pinkyCurlRight;

    void Start()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 5005);
            stream = client.GetStream();
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }


    void Update()
    {
        if (leftHandTransform != null || rightHandTransform != null)
        {
            UnityEngine.Vector3 leftHandPosition = leftHandTransform.position;
            UnityEngine.Vector3 rightHandPosition = rightHandTransform.position;

            leftHandPos = leftHandTransform.transform.position;
            rightHandPos = rightHandTransform.transform.position;

            leftHandRot = leftHandTransform.transform.rotation.eulerAngles;
            rightHandRot = rightHandTransform.transform.rotation.eulerAngles;

            handPositionText.text = "Left Hand: \npX: " + leftHandPos.x.ToString("F2") + "\npY: " + leftHandPos.y.ToString("F2") + "\npZ:" + leftHandPos.z.ToString("F2") + "\n" + "rX: " + leftHandRot.x.ToString("F2") + "\nrY: " + leftHandRot.y.ToString("F2") + "\nrZ:" + leftHandRot.z.ToString("F2") + "\n" +
                                "Right Hand: \npX:" + rightHandPos.x.ToString("F2") + "\npY: " + rightHandPos.y.ToString("F2") + "\npZ:" + rightHandPos.z.ToString("F2") + "\n" + "\nrX: " + rightHandRot.x.ToString("F2") + "\nrY: " + rightHandRot.y.ToString("F2") + "\nrZ:" + rightHandRot.z.ToString("F2") + "\n";

            fingerJointsText.text = "Finger Joints: \n";

            Dictionary<OVRSkeleton.BoneId, Transform> leftHandMap = new();
            Dictionary<OVRSkeleton.BoneId, Transform> rightHandMap = new();

            foreach (var bone in leftHandSkeleton.Bones)
            {
                leftHandMap[bone.Id] = bone.Transform;
            }

            foreach (var bone in rightHandSkeleton.Bones)
            {
                rightHandMap[bone.Id] = bone.Transform;
            }

            left_indexCurl = GetFingerCurl(leftHandMap, OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.Hand_Index3, OVRSkeleton.BoneId.Hand_IndexTip);
            left_middleCurl = GetFingerCurl(leftHandMap, OVRSkeleton.BoneId.Hand_Middle1, OVRSkeleton.BoneId.Hand_Middle2, OVRSkeleton.BoneId.Hand_Middle3, OVRSkeleton.BoneId.Hand_MiddleTip);
            left_ringCurl = GetFingerCurl(leftHandMap, OVRSkeleton.BoneId.Hand_Ring1, OVRSkeleton.BoneId.Hand_Ring2, OVRSkeleton.BoneId.Hand_Ring3, OVRSkeleton.BoneId.Hand_RingTip);
            left_pinkyCurl = GetFingerCurl(leftHandMap, OVRSkeleton.BoneId.Hand_Pinky1, OVRSkeleton.BoneId.Hand_Pinky2, OVRSkeleton.BoneId.Hand_Pinky3, OVRSkeleton.BoneId.Hand_PinkyTip);

            right_indexCurlRight = GetFingerCurl(rightHandMap, OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.Hand_Index3, OVRSkeleton.BoneId.Hand_IndexTip);
            right_middleCurlRight = GetFingerCurl(rightHandMap, OVRSkeleton.BoneId.Hand_Middle1, OVRSkeleton.BoneId.Hand_Middle2, OVRSkeleton.BoneId.Hand_Middle3, OVRSkeleton.BoneId.Hand_MiddleTip);
            right_ringCurlRight = GetFingerCurl(rightHandMap, OVRSkeleton.BoneId.Hand_Ring1, OVRSkeleton.BoneId.Hand_Ring2, OVRSkeleton.BoneId.Hand_Ring3, OVRSkeleton.BoneId.Hand_RingTip);
            right_pinkyCurlRight = GetFingerCurl(rightHandMap, OVRSkeleton.BoneId.Hand_Pinky1, OVRSkeleton.BoneId.Hand_Pinky2, OVRSkeleton.BoneId.Hand_Pinky3, OVRSkeleton.BoneId.Hand_PinkyTip);

            fingerJointsText.text = " Left Finger Joints: \n" +
                              "Index Finger Curl: " + left_indexCurl.ToString("F2") + "\n" +
                              "Middle Finger Curl: " + left_middleCurl.ToString("F2") + "\n" +
                              "Ring Finger Curl: " + left_ringCurl.ToString("F2") + "\n" +
                              "Pinky Finger Curl: " + left_pinkyCurl.ToString("F2") + "\n";

            fingerJointsText.text += "Right Finger Joints: \n" +
                              "Index Finger Curl: " + right_indexCurlRight.ToString("F2") + "\n" +
                              "Middle Finger Curl: " + right_middleCurlRight.ToString("F2") + "\n" +
                              "Ring Finger Curl: " + right_ringCurlRight.ToString("F2") + "\n" +
                              "Pinky Finger Curl: " + right_pinkyCurlRight.ToString("F2") + "\n";

            HandData handData = new HandData
            {
                leftHandPos = new Vector3Data { x = leftHandPos.x, y = leftHandPos.y, z = leftHandPos.z },
                leftHandRot = new Vector3Data { x = leftHandRot.x, y = leftHandRot.y, z = leftHandRot.z },
                leftFingerCurl = new FingerCurl
                {
                    indexCurl = left_indexCurl,
                    middleCurl = left_middleCurl,
                    ringCurl = left_ringCurl,
                    pinkyCurl = left_pinkyCurl
                },
                rightHandPos = new Vector3Data { x = rightHandPos.x, y = rightHandPos.y, z = rightHandPos.z },
                rightHandRot = new Vector3Data { x = rightHandRot.x, y = rightHandRot.y, z = rightHandRot.z },
                rightFingerCurl = new FingerCurl
                {
                    indexCurl = right_indexCurlRight,
                    middleCurl = right_middleCurlRight,
                    ringCurl = right_ringCurlRight,
                    pinkyCurl = right_pinkyCurlRight
                }

            };

            if(stream != null && stream.CanWrite)
            {
                SendData(handData);
            }
        }
        else
        {
            handPositionText.text = "Left Hand Position: Not Tracked\n" +
                              "Right Hand Position: Not Tracked";
        }

    }

    void SendData(HandData handData)
    {
        string json = JsonUtility.ToJson(handData) + "\n";
        Debug.Log("Sending data: " + json);

        if (stream != null && stream.CanWrite)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            try
            {
                stream.Write(jsonBytes, 0, jsonBytes.Length);
                stream.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError("Error sending data: " + e.Message);
            }
        }
    }

    float GetFingerCurl(Dictionary<OVRSkeleton.BoneId, Transform> map, OVRSkeleton.BoneId id1, OVRSkeleton.BoneId id2, OVRSkeleton.BoneId id3, OVRSkeleton.BoneId idTip)
    {
        UnityEngine.Vector3 dir1 = (map[id2].position - map[id1].position).normalized;
        UnityEngine.Vector3 dir2 = (map[id3].position - map[id2].position).normalized;
        UnityEngine.Vector3 dir3 = (map[idTip].position - map[id3].position).normalized;

        float angle1 = UnityEngine.Vector3.Angle(dir1, dir2);
        float angle2 = UnityEngine.Vector3.Angle(dir2, dir3);

        float totalCurl = angle1 + angle2;
        float minAngle = 0f;
        float maxAngle = 120f;
        float normalizedCurl = Mathf.InverseLerp(minAngle, maxAngle, totalCurl);

        return Mathf.Clamp(normalizedCurl * 100, 0f, 100f);
    }

    void OnApplicationQuit()
    {
        try
        {
            stream?.Close();
            client?.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error closing connection: " + e.Message);
        }
    }
}

