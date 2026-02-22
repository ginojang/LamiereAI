using UnityEngine;

public class BaseSceneEnvrionment : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MyCharacterCore.Instance.CreateMyCharacter();
        //InitPlayer(Vector3.zero, 0.0f);
    }


    // Update is called once per frame
    void Update()
    {

    }


}