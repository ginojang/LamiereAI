using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPrefab : ControllerPrefab
{
    [SerializeField]
    public NPCManager.NPCData npcData;

    [SerializeField]
    GameObject headMarkQuestion, headMarkExclamation;

    // Start is called before the first frame update
    void Start()
    {
        InitOnStart();

        headMarkQuestion.SetActive(false);
        headMarkExclamation.SetActive(false);
    }


    public void SetHeadStatus(NPCManager.NPCData.HeadStatus status)
    {
        switch(status)
        {
            case NPCManager.NPCData.HeadStatus.NONE:
                headMarkQuestion.SetActive(false);
                headMarkExclamation.SetActive(false);
                break;

            case NPCManager.NPCData.HeadStatus.QUESTION:
                headMarkQuestion.SetActive(true);
                headMarkExclamation.SetActive(false);
                break;

            case NPCManager.NPCData.HeadStatus.EXCLAMATION:
                headMarkQuestion.SetActive(false);
                headMarkExclamation.SetActive(true);
                break;
        }
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
    public void SetRotation(Quaternion rot)
    {
        transform.rotation = rot;    
    }
    public void LookAt(Vector3 pos)
    {
        transform.LookAt(pos);
    }


}
