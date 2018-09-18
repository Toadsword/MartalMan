using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        SceneManagement smInstance = FindObjectOfType<SceneManagement>();
        if (smInstance)
            smInstance.ChangeScene(SceneManagement.Scenes.MENU);
	}
}
