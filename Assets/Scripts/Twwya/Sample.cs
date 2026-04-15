using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Twwya
{
    public sealed class Sample : MonoBehaviour
    {
        [SerializeField] private Sequence sequence = null!;
        [SerializeField] private bool playOnStart = true;

        private Cue? currentCue;

        private void Awake()
        {
            Assert.IsNotNull(sequence, "Sequence が未設定です。Sample コンポーネントに Sequence を割り当ててください。");
        }

        private void Start()
        {
            sequence.CueStarted.AddListener(HandleCueStarted);
            sequence.CueCompleted.AddListener(HandleCueCompleted);
            sequence.SequenceCompleted.AddListener(HandleSequenceCompleted);

            if (playOnStart)
            {
                sequence.Play().Forget();
            }
        }

        private void OnDestroy()
        {
            sequence.CueStarted.RemoveListener(HandleCueStarted);
            sequence.CueCompleted.RemoveListener(HandleCueCompleted);
            sequence.SequenceCompleted.RemoveListener(HandleSequenceCompleted);
        }

        private void HandleCueStarted(Cue cue, int _)
        {
            currentCue = cue;
            cue.ShowText();
        }

        private void HandleCueCompleted(Cue cue, int _)
        {
            if (ReferenceEquals(currentCue, cue))
            {
                currentCue = null;
                cue.ClearText();
            }
        }

        private void HandleSequenceCompleted()
        {
            if (currentCue != null)
            {
                currentCue.ClearText();
                currentCue = null;
            }
        }
    }
}
