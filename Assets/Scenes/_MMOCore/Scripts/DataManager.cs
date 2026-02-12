using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;



public class DataManager : MonoBehaviour
{
    [SerializeField]
    public int damdamInfoVersion, AppBundleVersion;

    [SerializeField]
    string InfoServerIP;

    [SerializeField]
    int current_quest_id;


    public static DataManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    #region 시작 담담 인포.

    [Serializable]
    public class DamDamInfo
    {
        public int version;
        public int appBundleVersion;
        
        public bool isStartEvent;
        public string eventStartText;

        public bool isEventPage;
        public string eventPageUrl;


    };

    DamDamInfo damdamInfo;
    public bool is_init_damdam_info, damdam_done, damdam_ok;

    void InitDamDamInfo()
    {
        damdamInfo = new DamDamInfo();

        damdamInfo.version = damdamInfoVersion;
        damdamInfo.appBundleVersion = AppBundleVersion;

        damdamInfo.isStartEvent = false;
        damdamInfo.eventStartText = "";

        damdamInfo.isEventPage = false;
        damdamInfo.eventPageUrl = "";


        //var dataPath = System.IO.Path.Combine(Application.persistentDataPath, $"damdaminfo.json");
        //File.Delete(dataPath);
        //File.WriteAllText(dataPath, JsonUtility.ToJson(damdamInfo));
    }

    
    public DamDamInfo GetDamDamInfo()
    {
        return damdamInfo;
    }

    public IEnumerator LoadDamDamInfo()
    {
        damdam_done = false;
        damdam_ok = false;

        try
        {
            InitDamDamInfo();
            // yield return Fetch...
            damdam_ok = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            damdam_ok = false;
        }
        finally
        {
            damdam_done = true;
        }

        yield return null;
    }



    // 실제로는 쓰일일이 없음.
    //
    void SaveDamDamInfo()
    {
        //var dataPath = System.IO.Path.Combine(Application.persistentDataPath, $"damdaminfo.json");
        //File.Delete(dataPath);
        //File.WriteAllText(dataPath, JsonUtility.ToJson(damdamInfo));
    }


#endregion

#region 사용자 데이터
    //private const string SAVE_USER_DATA_PATH = "Settings/user_data.json";

    public enum eUSER_TYPE
    {
        NORMAL,
        ADMIN,
    }


    [Serializable]
    public class UserData
    {
        public bool isInit, isTutorial;
        
        public UInt64 userKey;
        public string key_value;
        public string user_name;

        public int current_quest_id;
        
        //GINO
        //public QuestManager.QuestData.Status current_quest_status;
        
        
        // 게임정보
        public float heart_cnt;
        public float money_cnt;


        UInt64 MakeUserKey()
        {
            DateTime time = DateTime.Now;
            long ticks = time.Ticks;
            byte[] bytes = new byte[8];

            int i;
            for (i = 0; i < 8; i++, ticks >>= 8)
                bytes[i] = (byte)(ticks & 0xFF);

            // 4번과 5번만 랜덤으로 치환.
            for (i = 4; i < 6; i++)
                bytes[i] = (byte)(UnityEngine.Random.Range(0, 0xFF));

            // 6번과 7번은 그대로 둔다  0x08dd 이어야 한다.  바이트 오더링 확인하기 위해.  (아이폰)
            UInt64 key = BitConverter.ToUInt64(bytes);

            return key;
        }


        public UserData()
        {
            isInit = false;
            isTutorial = true;

            userKey = MakeUserKey();

            key_value = "328923";
            user_name = "일반사용자";


            // 재화 데이터
            heart_cnt = 0;
            money_cnt = 0;
        }
    }

    UserData userData;

    public void UserDataLoading()
    {
        // isInit가 false가 세팅 되어 있다.
        userData = new UserData();
        userData.current_quest_id = 0;

        Debug.Log($"Created New User : Key  [{userData.userKey}]");
    }

    public UserData GetMyUserData()
    {
        return userData;
    }

    public bool SaveUserData()
    {
        //var dataPath = System.IO.Path.Combine(Application.persistentDataPath, $"user_data_{UserInfoJsonVersion}.json");
        //File.Delete(dataPath);
        //File.WriteAllText(dataPath, JsonUtility.ToJson(userData));

        return true;
    }

#endregion

#region 캐릭터 데이터

    public enum eCHARACTER_TYPE
    {
        NONE = 0,
        /*
        GIRL_00,
        GIRL_01,
        GIRL_02,
        GIRL_03,*/
        BOY_00 = 1,
        BOY_01 = 2,
        BOY_02 = 3,
        BOY_04 = 4,

        MOMO = 100,
    }

    public enum eCHARACTER_FATNESS
    {
        NONE,
        SLIM,
        MIDDLE_SLIM,
        MIDDLE_FAT,
        FAT
    }

    public enum eCHARACTER_SKINCOLLOR
    {
        NONE,
        _color_skin1,
        _color_skin2,
        _color_skin3,
        _color_skin4,
        _color_skin5,
        _color_skin6,
        _color_skin7,
        _color_skin8
    }

    [Serializable]
    public class CharacterData
    {
        // 가장 최근에 있던 씬.
        //GINO
        //public eSCENE_TYPE current_scene;

        // 가장 최근에 선택한 캐릭터
        public eCHARACTER_TYPE char_type;
        public eCHARACTER_FATNESS char_fatness;
        public eCHARACTER_SKINCOLLOR char_skincolor;

        public Vector3 position;
        public Quaternion cameraAngle;
        public float rotate;

        // 캐릭터 선택 창에서의 회전 값
        public Vector3 select_rotate;

        public float cameraYaw, cameraPitch;


        public CharacterData()
        {
            // 처음 사작은 캐릭터 선택 창만 뜬 상태.
            //GINO
            //current_scene = eSCENE_TYPE.NONE;

            // 
            char_type = eCHARACTER_TYPE.BOY_04;
            char_fatness = eCHARACTER_FATNESS.NONE;         // 캐릭터 프리팹 디폴트
            char_skincolor = eCHARACTER_SKINCOLLOR.NONE;    // 캐릭터 프리팹 디폴트
            select_rotate = new Vector3(0.0f, 30.0f, 0.0f);

            //
            position = Vector3.zero;
            cameraAngle = Quaternion.identity;
            rotate = 0.0f;

            cameraYaw = 0.0f;
            cameraPitch = 0.0f;
        }
    }

    CharacterData my_characterData;


    // 제일 처음 시작 할떄 튜토리얼에서만 호출한다.
    public void InitMyCharacterData()
    {
        my_characterData = new CharacterData();
    }

    public bool CharacterDataLoading()
    {
        var dataPath = System.IO.Path.Combine(Application.persistentDataPath, $"character_data.json");

        if (File.Exists(dataPath))
        {
            // JSON 로드..
            my_characterData = JsonUtility.FromJson<CharacterData>(File.ReadAllText(dataPath));
        }

        else
        {
            // 그렇지 않으면 새로 생성
            my_characterData = new CharacterData();
        }

        return true;
    }
    public CharacterData GetMyCharacterData()
    {
        return my_characterData;
    }

    public bool SaveMyCharaterData()
    {
        var dataPath = System.IO.Path.Combine(Application.persistentDataPath, $"character_data.json");

        File.Delete(dataPath);
        File.WriteAllText(dataPath, JsonUtility.ToJson(my_characterData));
        
        return true;
    }
    #endregion


    #region 네트워크 서버 통신


    public string MakeFirstPacketToServer()
    {
        Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>   MakeFirstPacketToServer");

        // 제일 처음 보낼 패킷 -  제일 처음은 씬위치 NONE
        // GINO
        //CharacterInfoPacket charInfo = new CharacterInfoPacket(my_characterData);
        //return $"{(int)PacketIndex.FirstCharacterInfo}|{0}|{JsonUtility.ToJson(charInfo)}";

        return "";
    }

    public string MakeMoveToScene()
    {
        //Debug.Log($">>>>>>>>>>>>>>>>>>>>>>>>>>>>  MakeMoveToScene   {(int)my_characterData.current_scene}");

        //CharacterInfoPacket charInfo = new CharacterInfoPacket(my_characterData);
        //return $"{(int)PacketIndex.MoveToScene}|{(int)my_characterData.current_scene}|{JsonUtility.ToJson(charInfo)}";

        return "";
    }

    public string MakeLeaveFromScene()
    {
        //return $"{(int)PacketIndex.LeaveFromScene}|{0}|";

        return "";
    }


    #endregion


}
