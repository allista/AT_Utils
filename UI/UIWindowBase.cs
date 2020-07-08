//   UIWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using UnityEngine;
using System.Collections;
using AT_Utils.UI;
using UnityEngine.EventSystems;

namespace AT_Utils
{
    [PersistState]
    public abstract class UIWindowBase<T> : ICachedState where T : MonoBehaviour
    {
        readonly UIBundle bundle;
        private readonly string inputLockName;

        protected virtual string prefab_name => typeof(T).Name;
        GameObject prefab;
        private bool prefabNotFound;

        public T Controller { get; private set; }

        [ConfigOption] protected Vector3 pos = Vector3.zero;

        [ConfigOption] protected bool initialized;

        protected UIWindowBase(UIBundle bundle)
        {
            this.bundle = bundle;
            inputLockName = $"{prefab_name}-{base.GetHashCode():X}";
            GameEvents.onGameStateSave.Add(onGameSaved);
        }

        ~UIWindowBase()
        {
            GameEvents.onGameStateSave.Remove(onGameSaved);
            Utils.LockControls(inputLockName, false);
            this.SaveState();
        }

        protected virtual void init_controller()
        {
            if(!(Controller is ScreenBoundRect window))
                return;
            window.onPointerEnterEvent.AddListener(onPointerEnter);
            window.onPointerExitEvent.AddListener(onPointerExit);
        }

        protected virtual void onGamePause() {}
        protected virtual void onGameUnpause() {}
        protected virtual void onGameSaved(ConfigNode node) => this.SaveState();

        bool in_progress;
        protected virtual void onPointerEnter(PointerEventData eventData)
        {
            Utils.LockControls(inputLockName);
        }

        protected virtual void onPointerExit(PointerEventData eventData)
        {
            Utils.LockControls(inputLockName, false);
        }


        public IEnumerator Show()
        {
            if(in_progress || Controller != null || bundle.BundleNotFound || prefabNotFound)
                yield break;
            in_progress = true;
            if(prefab == null)
            {
                this.LoadState();
                foreach(var _ in bundle.LoadAsset(prefab_name))
                    yield return null;
                prefab = bundle.GetAsset(prefab_name);
                if(prefab == null)
                {
                    Utils.Error($"Prefab {prefab_name} is not found in {bundle}");
                    prefabNotFound = true;
                    goto end;
                }
            }
            var obj = Object.Instantiate(prefab, DialogCanvasUtil.DialogCanvasRect);
            Controller = obj.GetComponent<T>();
            obj.SetActive(false);
            if(Controller == null)
            {
                Utils.Error("{} does not have {} component: {}",
                    obj,
                    typeof(T).Name,
                    obj.GetComponents<MonoBehaviour>());
                Object.Destroy(obj);
                goto end;
            }
            init_controller();
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
            obj.SetActive(true);
            if(!initialized)
            {
                var rectT = (RectTransform)obj.transform;
                var rect = rectT.rect;
                var pivot = rectT.pivot;
                pos = new Vector3(-rect.width * pivot.x, rect.height * pivot.y);
                initialized = true;
            }
            obj.transform.localPosition = pos;
            end:
            in_progress = false;
        }

        public void Show(MonoBehaviour monoBehaviour)
        {
            if(!in_progress && monoBehaviour != null)
                monoBehaviour.StartCoroutine(Show());
        }

        public virtual void SyncState()
        {
            if(Controller != null)
                pos = Controller.transform.localPosition;
        }

        public void Close()
        {
            if(Controller == null)
                return;
            this.SaveState();
            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGamePause.Remove(onGameUnpause);
            Utils.LockControls(inputLockName, false);
            GameObject gameObject;
            (gameObject = Controller.gameObject).SetActive(false);
            Object.Destroy(gameObject);
            Controller = null;
        }

        public bool IsShown => !in_progress && Controller != null;

        public void Toggle(MonoBehaviour monoBehaviour, bool condition = true)
        {
            if(IsShown)
                Close();
            else if(condition)
                Show(monoBehaviour);
        }
    }
}
