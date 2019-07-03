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
            var obj = Object.Instantiate(prefab);
            Controller = obj.GetComponent<T>();
            obj.SetActive(false);
            if(Controller == null)
            {
                Utils.Log("{} does not have {} component: {}",
                          obj, typeof(T).Name, obj.GetComponents<MonoBehaviour>());
                Object.Destroy(obj);
                goto end;
            }
            obj.transform.SetParent(DialogCanvasUtil.DialogCanvasRect);
            init_controller();
            obj.SetActive(true);
            if(!initialized)
            {
                obj.transform.localPosition = new Vector3(-Screen.width, 0);
                Rect rect;
                while(true)
                {
                    rect = ((RectTransform)obj.transform).rect;
                    if(rect.width.Equals(0))
                        yield return null;
                    else
                        break;
                }
                pos = new Vector3(-rect.width / 2, rect.height / 2);
                initialized = true;
            }
            obj.transform.localPosition = pos;
        end:
            in_progress = false;
        }

        public void Show(MonoBehaviour monoBehaviour)
        {
            if(!in_progress)
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
                Controller.gameObject.SetActive(false);
                Object.Destroy(Controller.gameObject);
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
