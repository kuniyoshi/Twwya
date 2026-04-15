using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Twwya
{
    [DisallowMultipleComponent]
    public sealed class Cue : MonoBehaviour
    {
        [SerializeField] private string cueName = "Cue";
        [SerializeField, TextArea] private string text = string.Empty;
        [SerializeField] private TMP_Text lyricText = null!;
        [SerializeField, Min(0f)] private float duration = 2f;
        [SerializeField, Min(0f)] private float overlapBeforeNext = 0.25f;
        [SerializeField] private bool playOnEnable;

        public UnityEvent<Cue> Started = new();
        public UnityEvent<Cue> Completed = new();

        private CancellationTokenSource? playCancellationTokenSource;

        public string Name => string.IsNullOrWhiteSpace(cueName) ? "Cue" : cueName;

        public string Text => text;

        public float Duration => duration;

        public float OverlapBeforeNext => Mathf.Min(overlapBeforeNext, duration);

        public float DelayUntilNextCue => Mathf.Max(0f, duration - OverlapBeforeNext);

        public bool IsPlaying => playCancellationTokenSource != null;

        private void Awake()
        {
            Assert.IsNotNull(lyricText, "Lyric Text が未設定です。Cue コンポーネントに TextMeshProUGUI を割り当ててください。");
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play().Forget();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public UniTask Play(CancellationToken cancellationToken = default)
        {
            Stop();
            return PlayAsync(cancellationToken);
        }

        public void Stop()
        {
            if (playCancellationTokenSource == null)
            {
                return;
            }

            playCancellationTokenSource.Cancel();
            playCancellationTokenSource.Dispose();
            playCancellationTokenSource = null;
        }

        public async UniTask PlayAsync(CancellationToken cancellationToken = default)
        {
            if (lyricText.font == null)
            {
                lyricText.font = TMP_Settings.defaultFontAsset;
            }

            if (lyricText.font == null)
            {
                Debug.LogWarning("TMP Default Font Asset が見つかりません。TMP Essential Resources をインポートしてください。", this);
                return;
            }

            lyricText.enableWordWrapping = true;
            lyricText.alignment = TextAlignmentOptions.Center;
            lyricText.color = Color.white;
            lyricText.fontSizeMin = 24f;
            lyricText.fontSizeMax = 72f;
            lyricText.enableAutoSizing = true;

            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this.GetCancellationTokenOnDestroy());

            playCancellationTokenSource = linkedTokenSource;
            Started.Invoke(this);

            try
            {
                if (duration > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: linkedTokenSource.Token);
                }

                Completed.Invoke(this);
            }
            catch (OperationCanceledException) when (linkedTokenSource.IsCancellationRequested)
            {
            }
            finally
            {
                if (ReferenceEquals(playCancellationTokenSource, linkedTokenSource))
                {
                    playCancellationTokenSource = null;
                }

                linkedTokenSource.Dispose();
            }
        }

        public void ShowText()
        {
            lyricText.text = text;
        }

        public void ClearText()
        {
            lyricText.text = string.Empty;
        }
    }
}
