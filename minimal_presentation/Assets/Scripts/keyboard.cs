using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class keyboard : MonoBehaviour
{

    public Text spellerText;
    public string DEFAULTTEXT = "Your text here";
    // Start is called before the first frame update
    void Start()
    {
        spellerText.text = DEFAULTTEXT;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addCharacter(string newtext)
    {
        spellerText.text += newtext;
    }
    public void addCharacter(GameObject go)
    {
        if ( go != null )
            spellerText.text += go.name;
    }
    public void removeCharacter()
    {
        spellerText.text = spellerText.text.Remove(spellerText.text.Length-1);
    }
}
