using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayersManager : MonoBehaviour
{
    public Text text;
    public int players = 0;

	void Awake () 
    {
        text = GetComponent<Text>();
        //if (MenuManager.Instance.Room != null)
        //    players = MenuManager.Instance.Room.playerCount;
	}
	
	void Update () 
    {
        if (text != null)
            text.text = players.ToString();
	}
}
