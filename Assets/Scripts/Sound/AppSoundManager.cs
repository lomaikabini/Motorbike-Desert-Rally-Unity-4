using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Controls every aspect on sound playing in the game
/// 
/// </summary>
public class AppSoundManager : MonoBehaviour {

    public static float SfxVolume = 1.0f;
    public static float _MusicVolume = 1.0f;
    public static bool MuteSfx = false;
    public static bool MuteMusic = false;


    public const int ChannelsCount = 8;
    public const float FadeoutMusicDelay = 1.0f;

    private List<AudioSource> sourceChannels;
    private AudioSource backgroudMusic;


    private static AppSoundManager _instance = null;
    private bool isPaused;

    private List<AudioSource> pausedChannels;

    private bool onFadeoutMusic;
    private float fadeoutMusicTimer;
    private float fadeoutMusicRealDelay;
    private bool _mute;


	public static AudioSource car;
	public static float startPitch = 0.3f;
	public static float endPitch = 1f;
	public static float startVolume = 0.3f;
	public static float endVolume = 1f;

	public void PlaySfxCar(Sfx.Type type)
	{
		if (MuteSfx) return;
		
		// name
		if (isPaused)
		{
			Debug.LogWarning("Its paused... get out");
			//    return;
		}
		
		string resName = Sfx.sfxFiles[(int)type];
		//Debug.Log("Load sfx:" + resName);
		AudioClip clip = (AudioClip)Resources.Load(resName,typeof(AudioClip));
		if (clip == null)
		{
			Debug.LogError("Unable to load clip: "+resName);
			return;
		}
		car = getFreeChannel();
		car.volume = _mute ? 0 : startVolume;
		car.pitch = startPitch;
		car.loop = true;
		car.clip = clip;
		car.Play();
		//XXXchan.PlayOneShot(clip);
	}
	


    public bool Mute
    {
        get
        {
            return _mute;
        }
        set
        {
            _mute = value;
            if (backgroudMusic.isPlaying)
            {
                backgroudMusic.volume = _mute ? 0 : MusicVolume;
            }
        }
    }

    public float MusicVolume
    {
        set { backgroudMusic.volume = value; }
        get { return _MusicVolume; }

    }

    public static AppSoundManager Get()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("SoundManager");
            go.transform.position = new Vector3(100,100,100);
            _instance = go.AddComponent<AppSoundManager>();
        }
        return _instance;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Destroy previous create sound manager");
            GameObject.Destroy(_instance.gameObject);
        }
        _instance = this;
        createChannels();
        pausedChannels = new List<AudioSource>();
        isPaused = false;
        onFadeoutMusic = false;
    }


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (onFadeoutMusic)
        {
            fadeoutMusicTimer -= Time.deltaTime;
            if (fadeoutMusicTimer > 0)
            {
                float fol = fadeoutMusicTimer / fadeoutMusicRealDelay * MusicVolume;
                backgroudMusic.volume = fol;
            }
            else
            {
                StopMusic();
                onFadeoutMusic = false;
            }
        }
	}

    /// <summary>
    /// Pause all effects and sound
    /// </summary>
    public void PauseAll()
    {
        if (isPaused)
        {
           // Debug.LogError("Probably bad logic or error. But sound manager is already in paused state");
            //return;
        }
		pausedChannels.Clear();
        if (backgroudMusic.isPlaying)
        {
            backgroudMusic.Pause();
        }
		pausedChannels.Add(backgroudMusic);
      	 StopSfx();
        isPaused = true;
    }
	
	public IEnumerator PlaySfxAndPauseMusic(Sfx.Type t)
	{
		PauseAll();
		yield return StartCoroutine(PlaySfxCoroutine(t));
	}

    /// <summary>
    /// Resume all
    /// </summary>
    public void ResumeAll()
    {
        isPaused = false;
        foreach (AudioSource audioChan in pausedChannels)
        {
            if (audioChan == backgroudMusic) 
                audioChan.volume = MusicVolume;
            else
                audioChan.volume = SfxVolume;

            audioChan.Play();
        }
        pausedChannels.Clear();
    }

    /// <summary>
    /// Plays seasoned click
    /// </summary>
    public void PlaySfxMenuClick()
    {
        
    }

    public void StopSfx()
	{
		foreach (AudioSource src in sourceChannels)
        {
            if (src.isPlaying) src.Stop();
        }
	}

    public void PlaySfx(Sfx.Type type)
    {
		if (MuteSfx) return;
		
        // name
        if (isPaused)
        {
            Debug.LogWarning("Its paused... get out");
        //    return;
        }

        string resName = Sfx.sfxFiles[(int)type];
        //Debug.Log("Load sfx:" + resName);
        AudioClip clip = (AudioClip)Resources.Load(resName,typeof(AudioClip));
        if (clip == null)
        {
            Debug.LogError("Unable to load clip: "+resName);
            return;
        }
        AudioSource chan = getFreeChannel();
        chan.volume = _mute ? 0 : SfxVolume;
        chan.loop = false;
        chan.clip = clip;
        chan.Play();
        //XXXchan.PlayOneShot(clip);
    }
	
	public IEnumerator PlaySfxCoroutine(Sfx.Type type)
    {
		if (MuteSfx) yield break;

        string resName = Sfx.sfxFiles[(int)type];
        //Debug.Log("Load sfx:" + resName);
        AudioClip clip = (AudioClip)Resources.Load(resName,typeof(AudioClip));
        if (clip == null)
        {
            Debug.LogError("Unable to load clip: "+resName);
            yield break;
        }

        AudioSource chan = getFreeChannel();
        chan.volume = _mute ? 0 : SfxVolume;
        chan.loop = false;
        chan.clip = clip;
        chan.Play();
		while(chan.isPlaying) yield return null;
    }
	
	AudioSource dynChannel;
	
	public void PlaySfxForDynamic(Sfx.Type type)
	{
		if (MuteSfx || (dynChannel != null && dynChannel.isPlaying)) return;
		
        // name
        if (isPaused)
        {
            Debug.LogWarning("Its paused... get out");
        //    return;
        }

        string resName = Sfx.sfxFiles[(int)type];
        //Debug.Log("Load sfx:" + resName);
        AudioClip clip = (AudioClip)Resources.Load(resName,typeof(AudioClip));
        if (clip == null)
        {
            Debug.LogError("Unable to load clip: "+resName);
            return;
        }

        dynChannel = getFreeChannel();
        dynChannel.volume = _mute ? 0 : SfxVolume;
        dynChannel.loop = false;
        dynChannel.clip = clip;
        dynChannel.Play();
	}
	
	public void StopSfxForDynamic()
	{
		if (dynChannel != null)
			dynChannel.Stop();
	}
	
    public AudioSource PlaySfxClip(AudioClip clip, bool isLoop = false)
    {
		if (MuteSfx) return null;
        AudioSource chan = getFreeChannel();
        chan.volume = _mute ? 0 : SfxVolume;
        chan.loop = isLoop;
        if (!isLoop)
        {
            chan.PlayOneShot(clip);
        }
        else
        {
            chan.clip = clip;
            chan.Play();
        }
        return chan;
    }


    public AudioSource PlaySfxUinque(AudioClip clip)
    {
        if (isClipIsPlaying(clip)) return null;
        AudioSource chan = getFreeChannel();
        chan.volume = _mute ? 0 : SfxVolume;
        chan.clip = clip;
        chan.Play();
        return chan;
    }

    public bool isMusicPlaying()
    {
        return backgroudMusic.isPlaying;
    }

    /// <summary>
    /// Create source source channel
    /// </summary>
    private void createChannels()
    {
        sourceChannels = new List<AudioSource>();
        for (int idx = 0; idx < ChannelsCount ; idx++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            sourceChannels.Add(src);
        }
        backgroudMusic = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Get free (non-playing channel) or the last one
    /// </summary>
    /// <returns></returns>
    private AudioSource getFreeChannel()
    {
        foreach (AudioSource src in sourceChannels)
        {
            if (!src.isPlaying && !pausedChannels.Contains(src)) 
				return src;
        }
		sourceChannels.Add(gameObject.AddComponent<AudioSource>());
        return sourceChannels[sourceChannels.Count - 1];
    }

    private bool isClipIsPlaying(AudioClip cl)
    {
        foreach (AudioSource src in sourceChannels)
        {
            if (src.isPlaying && src.clip == cl) return true;
        }
        return false;
    }


    #region "Music"
	
    public void PlayMusic(Music.Type t)
    {
		if (MuteMusic) return;
		if (backgroudMusic != null)
			StopMusic();
        string f = Music.musicFiles[(int)t];
        AudioClip ac =  Resources.Load(f, typeof(AudioClip)) as AudioClip;
        if (ac == null)
        {
            Debug.LogError("Unable to load: " + f + " clip as bg music");
            return;
        }
        backgroudMusic.volume = _mute ? 0 : MusicVolume;
        backgroudMusic.loop = true;
        backgroudMusic.clip = ac;
        backgroudMusic.Play();
    }
	public void PlayMusicNotLoop(Music.Type t)
	{
		if (MuteMusic) return;
		if (backgroudMusic != null)
			StopMusic();
		string f = Music.musicFiles[(int)t];
		AudioClip ac =  Resources.Load(f, typeof(AudioClip)) as AudioClip;
		if (ac == null)
		{
			Debug.LogError("Unable to load: " + f + " clip as bg music");
			return;
		}
		backgroudMusic.volume = _mute ? 0 : MusicVolume;
		backgroudMusic.loop = false;
		backgroudMusic.clip = ac;
		backgroudMusic.Play();
	}
    public void StopMusic()
    {
        backgroudMusic.Stop();
    }
	
	public void StartMusic()
    {
        backgroudMusic.Play();
    }

    /// <summary>
    /// Gradually fadeout music then stop it
    /// </summary>
    public void StopMusicWithFade(float delayTime = FadeoutMusicDelay)
    {
        onFadeoutMusic = true;
        if (delayTime <= 0) delayTime = 0.2f;
        fadeoutMusicTimer = delayTime;
        fadeoutMusicRealDelay = delayTime;

    }
    #endregion

}
