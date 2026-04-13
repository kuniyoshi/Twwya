using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Twwya
{
    [DisallowMultipleComponent]
    public sealed class Sequence : MonoBehaviour
    {
        [Serializable]
        public sealed class CueEvent : UnityEvent<Cue, int>
        {
        }

        [SerializeField] private bool playOnEnable;
        [SerializeField] private List<Cue> cues = new();

        public CueEvent CueStarted = new();
        public CueEvent CueCompleted = new();
        public UnityEvent SequenceCompleted = new();

        private CancellationTokenSource? playbackCancellationTokenSource;

        public IReadOnlyList<Cue> Cues => cues;

        public bool IsPlaying => playbackCancellationTokenSource != null;

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
            if (playbackCancellationTokenSource == null)
            {
                return;
            }

            playbackCancellationTokenSource.Cancel();
            playbackCancellationTokenSource.Dispose();
            playbackCancellationTokenSource = null;
        }

        public async UniTask PlayAsync(CancellationToken cancellationToken = default)
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this.GetCancellationTokenOnDestroy());

            playbackCancellationTokenSource = linkedTokenSource;
            var playingCueTasks = new List<UniTask>(cues.Count);

            try
            {
                for (var i = 0; i < cues.Count; i++)
                {
                    var cue = cues[i];
                    if (cue == null)
                    {
                        continue;
                    }

                    playingCueTasks.Add(PlayCueAsync(cue, i, linkedTokenSource.Token));

                    var delayUntilNextCue = cue.DelayUntilNextCue;
                    if (delayUntilNextCue > 0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(delayUntilNextCue), cancellationToken: linkedTokenSource.Token);
                    }
                }

                await UniTask.WhenAll(playingCueTasks);
                SequenceCompleted.Invoke();
            }
            catch (OperationCanceledException) when (linkedTokenSource.IsCancellationRequested)
            {
            }
            finally
            {
                if (ReferenceEquals(playbackCancellationTokenSource, linkedTokenSource))
                {
                    playbackCancellationTokenSource = null;
                }

                linkedTokenSource.Dispose();
            }
        }

        private async UniTask PlayCueAsync(Cue cue, int cueIndex, CancellationToken cancellationToken)
        {
            CueStarted.Invoke(cue, cueIndex);
            await cue.PlayAsync(cancellationToken);
            CueCompleted.Invoke(cue, cueIndex);
        }
    }
}
