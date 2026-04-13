using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Twwya
{
    [DisallowMultipleComponent]
    public sealed class Cue : MonoBehaviour
    {
        [SerializeField] private string cueName = "Cue";
        [SerializeField, TextArea] private string text = string.Empty;
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
    }
}
