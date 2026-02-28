using UnityEngine;

public class MinhoBattleBone : BaseTBone
{
    public GameObject swordSplash;

    public void OnSwordSlashStart(int animIndex)
    {
        if (animIndex == 2)
        {
            swordSplash.transform.localRotation = Quaternion.identity;
            swordSplash.transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
            swordSplash.SetActive(true);
        }
        else if(animIndex == 3)
        {
            swordSplash.transform.localRotation = Quaternion.identity;
            swordSplash.transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
            swordSplash.SetActive(true);
        }
        else if (animIndex == 1)
        {
            swordSplash.transform.localRotation = Quaternion.Euler(new Vector3(-180.0f, 0.0f, -60.0f));
            swordSplash.transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
            swordSplash.transform.localRotation *= Quaternion.Euler(0, 180f, 0);

            //swordSplash.transform.localScale = new Vector3(-0.7f, 0.7f, -0.7f);
            swordSplash.SetActive(true);
        }
    }

    public void OnNormalAttackExit(int animIndex)
    {
        swordSplash.SetActive(false);
    }
}
