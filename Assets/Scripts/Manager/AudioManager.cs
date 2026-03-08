using UnityEngine;
using NekoCollect.Util;

namespace NekoCollect.Manager
{
    /// <summary>
    /// BGM・SE再生を管理するオーディオマネージャー
    /// </summary>
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        [Header("BGM")]
        [SerializeField] private AudioClip bgmMain;
        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 0.4f;

        [Header("SE")]
        [SerializeField] private AudioClip seClick;
        [SerializeField] private AudioClip seCoin;
        [SerializeField] private AudioClip seTap;
        [SerializeField] private AudioClip seGachaRoll;
        [SerializeField] private AudioClip seGachaResult;
        [SerializeField] private AudioClip seGachaRare;
        [SerializeField] private AudioClip seFeed;
        [SerializeField] private AudioClip seLevelUp;
        [SerializeField] private AudioClip seEvolve;
        [SerializeField] private AudioClip seBonus;
        [Range(0f, 1f)]
        [SerializeField] private float seVolume = 0.7f;

        private AudioSource bgmSource;
        private AudioSource seSource;

        protected override void Awake()
        {
            base.Awake();

            // BGM用AudioSource
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;

            // SE用AudioSource
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.playOnAwake = false;
            seSource.volume = seVolume;
        }

        private void Start()
        {
            PlayBGM();
        }

        public void PlayBGM()
        {
            if (bgmMain == null) return;
            bgmSource.clip = bgmMain;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        // SE再生メソッド
        public void PlayClick() => PlaySE(seClick);
        public void PlayCoin() => PlaySE(seCoin);
        public void PlayTap() => PlaySE(seTap);
        public void PlayGachaRoll() => PlaySE(seGachaRoll);
        public void PlayGachaResult() => PlaySE(seGachaResult);
        public void PlayGachaRare() => PlaySE(seGachaRare);
        public void PlayFeed() => PlaySE(seFeed);
        public void PlayLevelUp() => PlaySE(seLevelUp);
        public void PlayEvolve() => PlaySE(seEvolve);
        public void PlayBonus() => PlaySE(seBonus);

        private void PlaySE(AudioClip clip)
        {
            if (clip == null) return;
            seSource.PlayOneShot(clip, seVolume);
        }
    }
}
