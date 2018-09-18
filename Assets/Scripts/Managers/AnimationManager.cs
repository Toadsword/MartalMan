using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour {

    [System.Serializable]
    public struct AnimStruct
    {
        public AnimStruct(Transform obj, Vector2 deltaPos, float time)
        {
            this.obj = obj;
            this.time = time;
            this.timer = Utility.StartTimer(time);
            this.startPos = obj.localPosition;
            this.endPos = (Vector2)obj.localPosition + deltaPos;
        }

        public Transform obj;
        public float time;
        public float timer;
        public Vector2 startPos;
        public Vector2 endPos;
    }

    List<AnimStruct> listAnimations;   

    public static AnimationManager _instance;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        listAnimations = new List<AnimStruct>();
    }

    private void FixedUpdate()
    {
        if (GameManager._instance.isInMenu || GameManager._instance.gamePaused)
            return;

        List<AnimStruct> doneAnim = new List<AnimStruct>();
        foreach (AnimStruct anim in listAnimations)
        {
            Vector2 newPos = Vector2.Lerp(anim.endPos, anim.startPos, Utility.GetTimerRemainingTime(anim.timer) / anim.time);
            anim.obj.localPosition = newPos;
            if (Utility.IsOver(anim.timer))
                doneAnim.Add(anim);
        }

        foreach (AnimStruct anim in doneAnim)
        {
            listAnimations.Remove(anim);
        }
    }

    public AnimStruct AddLerpAnimation(Transform obj, Vector2 deltaPos, float time)
    {
        AnimStruct anim = new AnimStruct(obj, deltaPos, time);
        listAnimations.Add(anim);

        return anim;
    }

    public void RemoveLerpAnimation(AnimStruct anim)
    {
        if (listAnimations.Contains(anim))
            listAnimations.Remove(anim);
    }
}
