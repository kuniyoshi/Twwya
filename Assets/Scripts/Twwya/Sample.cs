using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Twwya
{
    public sealed class Sample : MonoBehaviour
    {
        [SerializeField] private Sequence sequence = null!;
        [SerializeField] private TMP_Text lyricText = null!;
        [SerializeField] private bool playOnStart = true;

        private Cue? currentCue;

        private void Awake()
        {
            Assert.IsNotNull(sequence, "Sequence が未設定です。Sample コンポーネントに Sequence を割り当ててください。");
            Assert.IsNotNull(lyricText, "Lyric Text が未設定です。Sample コンポーネントに TextMeshProUGUI を割り当ててください。");
        }

        private void Start()
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

            lyricText.text = string.Empty;
            lyricText.enableWordWrapping = true;
            lyricText.alignment = TextAlignmentOptions.Center;
            lyricText.color = Color.white;
            lyricText.fontSizeMin = 24f;
            lyricText.fontSizeMax = 72f;
            lyricText.enableAutoSizing = true;

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
            lyricText.text = cue.Text;
        }

        private void HandleCueCompleted(Cue cue, int _)
        {
            if (ReferenceEquals(currentCue, cue))
            {
                currentCue = null;
                lyricText.text = string.Empty;
            }
        }

        private void HandleSequenceCompleted()
        {
            currentCue = null;
            lyricText.text = string.Empty;
        }
    }
}
