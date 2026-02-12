using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance = null;


    //
    [Serializable]
    public class NPCData
    {
        //
        public int index;
        //public GameManager.SceneIndex sceneIndex;
        public string npcName;
        public Vector3 position, euler;
        public DataManager.eCHARACTER_TYPE characterType;
        
        public int curDialogCondition;
        public Dictionary<int, List<string>> dialogs;

        public enum HeadStatus { NONE, QUESTION, EXCLAMATION };
        public HeadStatus headStatus;

    }


    [SerializeField]
    List<NPCData> npc_list;


    //
    private void Awake()
    {
        Instance = this;
        InitWithHardCoding();
    }

    public void InitWithHardCoding()
    {
        npc_list = new List<NPCData>();

       
    }
    /*
    public List<NPCData> GetNPCList(GameManager.SceneIndex sceneIndex)
    {
        List<NPCData> getNpcs = new List<NPCData>();

        foreach(var npc in npc_list)
        {
            if (npc.sceneIndex == sceneIndex)
                getNpcs.Add(npc);
        }

        return getNpcs;
    }

    public void SetNPCHeadStatusData(int npcID, NPCData.HeadStatus status)
    {
        foreach(var npc in npc_list)
        {
            if(npc.index == npcID)
            {
                npc.headStatus = status;
                break;
            }
        }

        GameManager.Instance.SetNPCHeadStatusRealMark();
    }*/

    public NPCData GetNPCData(int npcID)
    {
        foreach (var npc in npc_list)
            if (npc.index == npcID)
                return npc;


        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        npc_list = new List<NPCData>();

        var dataPath = System.IO.Path.Combine(Application.persistentDataPath, $"NPC_Data.json");
        File.Delete(dataPath);
        File.WriteAllText(dataPath, JsonUtility.ToJson(npc_list));
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
