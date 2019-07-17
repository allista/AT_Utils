//   UIWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri
using UnityEngine;
using System.Collections;

namespace AT_Utils
{
    [PersistState]
    public abstract class UIWindowBase<T> : ICachedState where T : MonoBehaviour
    {
        readonly UIBundle bundle;

        protected virtual string prefab_name => typeof(T).Name;
        GameObject prefab;

        public T Controller { get; private set; }

        [ConfigOption]
        protected Vector3 pos = Vector3.zero;

        [ConfigOption]
        protected bool initialized;

        protected UIWindowBase(UIBundle bundle)
        {
            this.bundle = bundle;
        }

        protected abstract void init_controller();

        bool in_progress;
        public IEnumerator Show()
        {
            if(in_progress || Controller != null)
                yield break;
            in_progress = true;
            if(prefab == null)
            {
                foreach(var _ in bundle.LoadAsset(prefab_name))
                    yield return null;
                prefab = bundle.GetAsset(prefab_name);
                if(prefab == null)
                    goto end;
            }
            var obj = Object.Instantiate(prefab, DialogCanvasUtil.DialogCanvasRect);
            Controller = obj.GetComponent<T>();
            obj.SetActive(false);
            if(Controller == null)
            {
                Utils.Log("{} does not have {} component: {}",
                          obj, typeof(T).Name, obj.GetComponents<MonoBehaviour>());
                Object.Destroy(obj);
                goto end;
            }
            init_controller();
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

        public void SyncState()
        {
            if(Controller != null)
                pos = Controller.transform.localPosition;
        }

        public void Close()
        {
            if(Controller != null)
            {
                pos = Controller.transform.localPosition;
                GameObject gameObject;
                (gameObject = Controller.gameObject).SetActive(false);
                Object.Destroy(gameObject);
                Controller = null;
            }
        }

        public bool IsShown =>
        !in_progress && Controller != null;

        public void Toggle(MonoBehaviour monoBehaviour)
        {
            if(IsShown)
                Close();
            else
                Show(monoBehaviour);
        }
    }
}
