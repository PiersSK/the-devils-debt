using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WallRune : MonoBehaviour
{
    [SerializeField] private Image runeImg;
    [SerializeField] private Sprite[] runeImgSprites;

    public void SetRuneImage(int index)
    {
        runeImg.sprite = runeImgSprites[index];
    }
}
