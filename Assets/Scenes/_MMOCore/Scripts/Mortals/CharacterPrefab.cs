using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPrefab : MonoBehaviour
{
    public GameObject[] SkinParts;
    public int[] SkinPartMaterialIndex;

    public DataManager.eCHARACTER_SKINCOLLOR skinColor;

    List<Color32> skinIndex;



    private void Awake()
    {
        skinIndex = new List<Color32>();

        skinIndex.Add(new Color32(0, 0, 0, 0));

        skinIndex.Add(new Color32(255, 255, 255, 255));     // 1
        skinIndex.Add(new Color32(220, 220, 220, 255));     // 2
        skinIndex.Add(new Color32(185, 185, 185, 255));     // 3
        skinIndex.Add(new Color32(175, 175, 175, 255));     // 4
        skinIndex.Add(new Color32(165, 165, 165, 255));     // 5
        skinIndex.Add(new Color32(150, 150, 150, 255));     // 6
        skinIndex.Add(new Color32(126, 126, 126, 255));     // 7
        skinIndex.Add(new Color32(100, 100, 100, 255));     // 8

    }

    void Start()
    {
        SetSkinColor(skinColor);
    }


    public void SetSkinColor(DataManager.eCHARACTER_SKINCOLLOR skin)
    {
        if (SkinParts.Length != SkinPartMaterialIndex.Length || skinColor == DataManager.eCHARACTER_SKINCOLLOR.NONE)
            return;

        skinColor = skin;

        Color32 color;
        switch (skinColor)
        {
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin1:
                color = skinIndex[1];
                break;
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin2:
                color = skinIndex[2];
                break;
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin3:
                color = skinIndex[3];
                break;
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin4:
                color = skinIndex[4];
                break;
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin5:
                color = skinIndex[5];
                break;
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin6:
                color = skinIndex[6];
                break;
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin7:
                color = skinIndex[7];
                break;
            case DataManager.eCHARACTER_SKINCOLLOR._color_skin8:
                color = skinIndex[8];
                break;
            default:
                return;
        }

        for (int i = 0; i < SkinParts.Length; i++)
        {
            SkinParts[i].GetComponent<SkinnedMeshRenderer>().materials[SkinPartMaterialIndex[i]].SetColor("_BaseColor", color);
        }
    }



}
