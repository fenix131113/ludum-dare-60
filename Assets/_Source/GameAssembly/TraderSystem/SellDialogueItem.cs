using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameAssembly.TraderSystem
{
    public class SellDialogueItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text dialogueLabel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private float lifetime;

        private SellMinigame _minigame;
        private bool _isDestroyed;
        
        public int Influence { get; private set; }

        public void Initialize(string text, int influence, SellMinigame minigame)
        {
            dialogueLabel.text = text;
            Influence = influence;
            _minigame = minigame;
            
            dialogueLabel.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueLabel.rectTransform);
            backgroundImage.rectTransform.sizeDelta = dialogueLabel.rectTransform.sizeDelta;

            StartCoroutine(DestroyAfterDelay(lifetime));
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if(_isDestroyed)
                return;
            
            _minigame.RemoveDialogue(this);
            _minigame.ApplyInfluence(Influence);
            
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            _isDestroyed = true;
            _minigame.RemoveDialogue(this);
            Destroy(gameObject);
        }
    }
}