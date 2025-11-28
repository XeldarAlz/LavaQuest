using UnityEngine;
using DG.Tweening;

namespace LavaQuest.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class GameStatePanel : MonoBehaviour, IGameState
    {
        [Header("Base Panel Settings")]
        [SerializeField] protected RectTransform panelRoot;
        [SerializeField] protected float showDuration = 0.4f;
        [SerializeField] protected float hideDuration = 0.3f;
        
        protected GameStateMachine stateMachine;
        protected CanvasGroup canvasGroup;
        
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            HideImmediate();
        }
        
        public void Initialize(GameStateMachine machine)
        {
            stateMachine = machine;
        }
        
        public virtual void Enter()
        {
            ShowPanel();
        }
        
        public virtual void Exit()
        {
            HidePanel();
        }
        
        public virtual void Tick()
        {
        }
        
        protected virtual void ShowPanel()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad);

            if (!panelRoot)
            {
                return;
            }
            
            panelRoot.localScale = Vector3.one * 0.9f;
            panelRoot.DOScale(1f, showDuration).SetEase(Ease.OutBack);
        }
        
        protected virtual void HidePanel()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.InQuad);
        }
        
        protected void HideImmediate()
        {
            if (!canvasGroup)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        protected void ChangeState(GameState newState)
        {
            stateMachine.ChangeState(newState);
        }
    }
}