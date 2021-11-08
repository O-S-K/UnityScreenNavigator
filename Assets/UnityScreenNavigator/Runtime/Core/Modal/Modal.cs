﻿using System.Collections;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Animation;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

#if USN_USE_ASYNC_METHODS
using System;
using System.Threading.Tasks;
#endif

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    [DisallowMultipleComponent]
    public class Modal : MonoBehaviour
    {
        [SerializeField] private bool _usePrefabNameAsIdentifier = true;

        [SerializeField] [EnabledIf(nameof(_usePrefabNameAsIdentifier), false)]
        private string _identifier;

        [SerializeField]
        private ModalTransitionAnimationContainer _animationContainer = new ModalTransitionAnimationContainer();

        private CanvasGroup _canvasGroup;
        private bool _isInitialized;
        private RectTransform _parentTransform;
        private RectTransform _rectTransform;

        public string Identifier
        {
            get => _identifier;
            set => _identifier = value;
        }

        public ModalTransitionAnimationContainer AnimationContainer => _animationContainer;

        public bool Interactable
        {
            get => _canvasGroup.interactable;
            set => _canvasGroup.interactable = value;
        }

#if USN_USE_ASYNC_METHODS
        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }
#else
        public virtual IEnumerator Initialize()
        {
            yield break;
        }
#endif

#if USN_USE_ASYNC_METHODS
        public virtual Task WillPushEnter()
        {
            return Task.CompletedTask;
        }
#else
        public virtual IEnumerator WillPushEnter()
        {
            yield break;
        }
#endif

        public virtual void DidPushEnter()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual Task WillPushExit()
        {
            return Task.CompletedTask;
        }
#else
        public virtual IEnumerator WillPushExit()
        {
            yield break;
        }
#endif

        public virtual void DidPushExit()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual Task WillPopEnter()
        {
            return Task.CompletedTask;
        }
#else
        public virtual IEnumerator WillPopEnter()
        {
            yield break;
        }
#endif

        public virtual void DidPopEnter()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual Task WillPopExit()
        {
            return Task.CompletedTask;
        }
#else
        public virtual IEnumerator WillPopExit()
        {
            yield break;
        }
#endif

        public virtual void DidPopExit()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual Task Cleanup()
        {
            return Task.CompletedTask;
        }
#else
        public virtual IEnumerator Cleanup()
        {
            yield break;
        }
#endif

        internal AsyncProcessHandle AfterLoad(RectTransform parentTransform)
        {
            if (!_isInitialized)
            {
                _rectTransform = (RectTransform)transform;
                _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
                _isInitialized = true;
            }

            _identifier = _usePrefabNameAsIdentifier ? gameObject.name.Replace("(Clone)", string.Empty) : _identifier;
            _parentTransform = parentTransform;
            _rectTransform.FillParent(_parentTransform);
            _canvasGroup.alpha = 0.0f;

            return CoroutineManager.Instance.Run(CreateCoroutine(Initialize()));
        }

        internal AsyncProcessHandle BeforeEnter(bool push, Modal partnerModal)
        {
            return CoroutineManager.Instance.Run(BeforeEnterRoutine(push, partnerModal));
        }

        private IEnumerator BeforeEnterRoutine(bool push, Modal partnerModal)
        {
            if (push)
            {
                gameObject.SetActive(true);
                _rectTransform.FillParent(_parentTransform);
                _canvasGroup.alpha = 0.0f;
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                _canvasGroup.interactable = false;
            }

            var routine = push ? WillPushEnter() : WillPopEnter();
            var handle = CoroutineManager.Instance.Run(CreateCoroutine(routine));

            while (!handle.IsTerminated)
            {
                yield return null;
            }
        }

        internal AsyncProcessHandle Enter(bool push, bool playAnimation, Modal partnerModal)
        {
            return CoroutineManager.Instance.Run(EnterRoutine(push, playAnimation, partnerModal));
        }

        private IEnumerator EnterRoutine(bool push, bool playAnimation, Modal partnerModal)
        {
            if (push)
            {
                _canvasGroup.alpha = 1.0f;

                if (playAnimation)
                {
                    var anim = _animationContainer.GetAnimation(true, partnerModal?.Identifier);
                    if (anim == null)
                    {
                        anim = UnityScreenNavigatorSettings.Instance.GetDefaultModalTransitionAnimation(true);
                    }

                    anim.SetPartner(partnerModal?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    yield return CoroutineManager.Instance.Run(anim.CreatePlayRoutine());
                }

                _rectTransform.FillParent(_parentTransform);
            }
        }

        internal void AfterEnter(bool push, Modal partnerModal)
        {
            if (push)
            {
                DidPushEnter();
            }
            else
            {
                DidPopEnter();
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                _canvasGroup.interactable = true;
            }
        }

        internal AsyncProcessHandle BeforeExit(bool push, Modal partnerModal)
        {
            return CoroutineManager.Instance.Run(BeforeExitRoutine(push, partnerModal));
        }

        private IEnumerator BeforeExitRoutine(bool push, Modal partnerModal)
        {
            if (!push)
            {
                gameObject.SetActive(true);
                _rectTransform.FillParent(_parentTransform);
                _canvasGroup.alpha = 1.0f;
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                _canvasGroup.interactable = false;
            }

            var routine = push ? WillPushExit() : WillPopExit();
            var handle = CoroutineManager.Instance.Run(CreateCoroutine(routine));

            while (!handle.IsTerminated)
            {
                yield return null;
            }
        }

        internal AsyncProcessHandle Exit(bool push, bool playAnimation, Modal partnerModal)
        {
            return CoroutineManager.Instance.Run(ExitRoutine(push, playAnimation, partnerModal));
        }

        private IEnumerator ExitRoutine(bool push, bool playAnimation, Modal partnerModal)
        {
            if (!push)
            {
                if (playAnimation)
                {
                    var anim = _animationContainer.GetAnimation(false, partnerModal?._identifier);
                    if (anim == null)
                    {
                        anim = UnityScreenNavigatorSettings.Instance.GetDefaultModalTransitionAnimation(false);
                    }

                    anim.SetPartner(partnerModal?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    yield return CoroutineManager.Instance.Run(anim.CreatePlayRoutine());
                }

                _canvasGroup.alpha = 0.0f;
            }
        }

        internal void AfterExit(bool push, Modal partnerModal)
        {
            if (push)
            {
                DidPushExit();
            }
            else
            {
                DidPopExit();
            }
        }

        internal AsyncProcessHandle BeforeRelease()
        {
            return CoroutineManager.Instance.Run(CreateCoroutine(Cleanup()));
        }

#if USN_USE_ASYNC_METHODS
        private IEnumerator CreateCoroutine(Task target)
#else
        private IEnumerator CreateCoroutine(IEnumerator target)
#endif
        {
#if USN_USE_ASYNC_METHODS
            async void WaitTaskAndCallback(Task task, Action callback)
            {
                await task;
                callback?.Invoke();
            }
            
            var isCompleted = false;
            WaitTaskAndCallback(target, () =>
            {
                isCompleted = true;
            });
            return new WaitUntil(() => isCompleted);
#else
            return target;
#endif
        }
    }
}