using UnityEngine;
using System.Collections;

public class MusicSfx : MonoBehaviour {

	public Type type;
	private bool state = true;

	public enum Type
	{
		MUSIC,
		SFX
	}

	void Start()
	{
		if(type!=null)
		{
			switch(type)
			{
				case Type.MUSIC: state = GameData.Get().music; 
										if(state) setOnImg();
										else setOffImg();break;
				case Type.SFX: state = GameData.Get().sfx;
										if(state) setOnImg();
										else setOffImg();break;
				default:break;
			}
		}
	}

	public void OnBtnPress()
	{
		state = !state;
		if(type == Type.MUSIC)
		{
			if(state)
			{
				Debug.Log("Music On");
				setOnImg();
				AppSoundManager.MuteMusic = false;
				GameData.Get().music = true;
				GameData.Get().save();
			}
			else
			{
				setOffImg();
				Debug.Log("Music OFF");
				AppSoundManager.MuteMusic = true;
				GameData.Get().music = false;
				GameData.Get().save();
			}
		}
		else
		{
			if(state)
			{
				setOnImg();
				Debug.Log("SFX On");
				AppSoundManager.MuteSfx = false;
				GameData.Get().sfx = true;
				GameData.Get().save();
			}
			else
			{
				setOffImg();
				Debug.Log("SFX OFF");
				AppSoundManager.MuteSfx = true;
				GameData.Get().sfx = false;
				GameData.Get().save();
			}
		}
		AppSoundManager.Get ().PlaySfx (Sfx.Type.sfx_click);
	}

	void setOnImg()
	{
		gameObject.GetComponent<UISprite> ().spriteName = "music_on";
		gameObject.GetComponent<UIButton> ().normalSprite = "music_on";
		gameObject.GetComponent<UIButton> ().hoverSprite = "music_on";
		gameObject.GetComponent<UIButton> ().pressedSprite = "music_on";
	}

	void setOffImg()
	{
		gameObject.GetComponent<UISprite> ().spriteName = "music_off";
		gameObject.GetComponent<UIButton> ().normalSprite = "music_off";
		gameObject.GetComponent<UIButton> ().hoverSprite = "music_off";
		gameObject.GetComponent<UIButton> ().pressedSprite = "music_off";
	}
	bool preval;
	public void muteTMP()
	{
		preval = AppSoundManager.MuteSfx;
		AppSoundManager.MuteSfx = true;
	}
	public void releaseTMP()
	{
		AppSoundManager.MuteSfx = preval;
	}
}
