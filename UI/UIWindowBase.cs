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
    public abstract class UIWindowBase<T> where T : MonoBehaviour
    {
        readonly UIBundle bundle;

        protected virtual string prefab_name => typeof(T).Name;
        GameObject prefab;

        public T Controller { get; private set; }

        [ConfigOption]
        protected Vector3 pos = Vector3.zero;

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
            bool first_start = false;
            if(prefab == null)
            {
                foreach(var _ in bundle.LoadAsset(prefab_name))
                    yield return null;
                prefab = bundle.GetAsset(prefab_name);
                if(prefab == null)
                    goto end;
                first_start = true;
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
            init_controller();
            obj.transform.SetParent(DialogCanvasUtil.DialogCanvasRect);
            obj.SetActive(true);
            if(first_start)
            {
                obj.transform.localPosition = new Vector3(-Screen.width, 0);
                Rect rect = new Rect();
                while(rect.width.Equals(0))
                {
                    rect = (obj.transform as RectTransform).rect;
                    yield return null;
                }
                pos = new Vector3(-rect.width / 2, rect.height / 2);
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
