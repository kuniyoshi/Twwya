using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Twwya
{
    public sealed class Sample : MonoBehaviour
    {
        [SerializeField] private Sequence? sequence;
        [SerializeField] private bool playOnStart = true;

        private Cue? currentCue;
        private GUIStyle? labelStyle;

        private void Start()
        {
            if (sequence == null)
            {
                Debug.LogWarning("Sequence が未設定です。Sample コンポーネントに Sequence を割り当ててください。", this);
                return;
            }

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
            if (sequence == null)
            {
                return;
            }

            sequence.CueStarted.RemoveListener(HandleCueStarted);
            sequence.CueCompleted.RemoveListener(HandleCueCompleted);
            sequence.SequenceCompleted.RemoveListener(HandleSequenceCompleted);
        }

        private void OnGUI()
        {
            if (currentCue == null || string.IsNullOrWhiteSpace(currentCue.Text))
            {
                return;
            }

            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(24, Screen.height / 18),
                wordWrap = true,
                richText = true
            };

            labelStyle.normal.textColor = Color.white;

            var width = Mathf.Min(Screen.width - 64f, 960f);
            var rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.4f, width, Screen.height * 0.2f);

            var shadowRect = rect;
            shadowRect.position += new Vector2(2f, 2f);
            var originalColor = labelStyle.normal.textColor;
            labelStyle.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(shadowRect, currentCue.Text, labelStyle);

            labelStyle.normal.textColor = originalColor;
            GUI.Label(rect, currentCue.Text, labelStyle);
        }

        private void HandleCueStarted(Cue cue, int _)
        {
            currentCue = cue;
        }

        private void HandleCueCompleted(Cue cue, int _)
        {
            if (ReferenceEquals(currentCue, cue))
            {
                currentCue = null;
            }
        }

        private void HandleSequenceCompleted()
        {
            currentCue = null;
        }
    }
}
