using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkLobby;

public class SkinManager : MonoBehaviour {
    
    public enum SkinType
    {
        NORMAL,
        ANGRY,
        HAPPY,
        CAT,
        ANGRY_CAT,
        HAPPY_CAT,
        CHINESE_CAT,
        ANGRY_CHINESE_CAT,
        CHINESE,
        ANGRY_CHINESE,
        ANGRY_ROOK,
        END_LIST
    }

    [Header("BlueTeamSkins")]
    [SerializeField] Sprite blue_normalSkin;
    [SerializeField] Sprite blue_angrySkin;
    [SerializeField] Sprite blue_happySkin;
    [SerializeField] Sprite blue_catSkin;
    [SerializeField] Sprite blue_angryCatSkin;
    [SerializeField] Sprite blue_happyCatSkin;
    [SerializeField] Sprite blue_chineseCatSkin;
    [SerializeField] Sprite blue_angryChineseCatSkin;
    [SerializeField] Sprite blue_chineseSkin;
    [SerializeField] Sprite blue_angryChineseSkin;
    [SerializeField] Sprite blue_angryRookSkin;

    [Header("RedTeamSkins")]
    [SerializeField] Sprite red_normalSkin;
    [SerializeField] Sprite red_angrySkin;
    [SerializeField] Sprite red_happySkin;
    [SerializeField] Sprite red_catSkin;
    [SerializeField] Sprite red_angryCatSkin;
    [SerializeField] Sprite red_happyCatSkin;
    [SerializeField] Sprite red_chineseCatSkin;
    [SerializeField] Sprite red_angryChineseCatSkin;
    [SerializeField] Sprite red_chineseSkin;
    [SerializeField] Sprite red_angryChineseSkin;
    [SerializeField] Sprite red_angryRookSkin;
    [Header("DeadSkins")]
    [SerializeField] Sprite red_deadNormalSkin;
    [SerializeField] Sprite red_deadCatSkin;
    [SerializeField] Sprite red_deadChineseSkin;
    [SerializeField] Sprite blue_deadNormalSkin;
    [SerializeField] Sprite blue_deadCatSkin;
    [SerializeField] Sprite blue_deadChineseSkin;


    public static SkinManager _instance;

    private void Awake()
    {
        if (_instance)
            Destroy(gameObject);
        else
            _instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public Sprite GetSprite(SkinType type, LobbyPlayer.PlayerTeam team, bool isDead = false)
    {
        if(team == LobbyPlayer.PlayerTeam.RED)
        {
            if (!isDead)
            {
                switch(type)
                {
                    case SkinType.ANGRY: return red_angrySkin;
                    case SkinType.ANGRY_CAT: return red_angryCatSkin;
                    case SkinType.ANGRY_CHINESE: return red_angryChineseSkin;
                    case SkinType.ANGRY_CHINESE_CAT: return red_angryChineseCatSkin;
                    case SkinType.ANGRY_ROOK: return red_angryRookSkin;
                    case SkinType.CAT: return red_catSkin;
                    case SkinType.CHINESE: return red_chineseSkin;
                    case SkinType.CHINESE_CAT: return red_chineseCatSkin;
                    case SkinType.HAPPY: return red_happySkin;
                    case SkinType.HAPPY_CAT: return red_happyCatSkin;
                    case SkinType.NORMAL: return red_normalSkin;
                }
            }
            else
            {
                switch (type)
                {
                    case SkinType.ANGRY:
                    case SkinType.ANGRY_ROOK:
                    case SkinType.HAPPY:
                    case SkinType.NORMAL:
                        return red_deadNormalSkin;
                    case SkinType.ANGRY_CAT:
                    case SkinType.CAT:
                    case SkinType.ANGRY_CHINESE_CAT:
                    case SkinType.CHINESE_CAT:
                    case SkinType.HAPPY_CAT:
                        return red_deadCatSkin;
                    case SkinType.CHINESE:
                    case SkinType.ANGRY_CHINESE:
                        return red_deadChineseSkin;
                }
            }
        }
        else
        {
            if (!isDead)
            {
                switch (type)
                {
                    case SkinType.ANGRY: return blue_angrySkin;
                    case SkinType.ANGRY_CAT: return blue_angryCatSkin;
                    case SkinType.ANGRY_CHINESE: return blue_angryChineseSkin;
                    case SkinType.ANGRY_CHINESE_CAT: return blue_angryChineseCatSkin;
                    case SkinType.ANGRY_ROOK: return blue_angryRookSkin;
                    case SkinType.CAT: return blue_catSkin;
                    case SkinType.CHINESE: return blue_chineseSkin;
                    case SkinType.CHINESE_CAT: return blue_chineseCatSkin;
                    case SkinType.HAPPY: return blue_happySkin;
                    case SkinType.HAPPY_CAT: return blue_happyCatSkin;
                    case SkinType.NORMAL: return blue_normalSkin;
                }
            }
            else
            {
                switch (type)
                {
                    case SkinType.ANGRY:
                    case SkinType.ANGRY_ROOK:
                    case SkinType.HAPPY:
                    case SkinType.NORMAL:
                        return blue_deadNormalSkin;
                    case SkinType.ANGRY_CAT:
                    case SkinType.CAT:
                    case SkinType.ANGRY_CHINESE_CAT:
                    case SkinType.CHINESE_CAT:
                    case SkinType.HAPPY_CAT:
                        return blue_deadCatSkin;
                    case SkinType.CHINESE:
                    case SkinType.ANGRY_CHINESE:
                        return blue_deadChineseSkin;
                }
            }
        }
        return null;
    }
}
