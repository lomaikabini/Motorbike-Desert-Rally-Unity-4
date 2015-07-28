using UnityEngine;
using System.Collections;

public class Sfx 
{
    public enum Type
    {
		sfx_click_character,
		sfx_click,
		sfx_idle
    };


    public static string[] sfxFiles = 
    {
		"bonus",
		"sfx_click",
		"Idle"
    };

}

public class Music
{
    public enum Type
    {
        GameOverMusic
    }

    public static string[] musicFiles =
    {
		"Hustle"
    };
}

