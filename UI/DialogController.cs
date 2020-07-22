using System.Collections;
using AT_Utils.UI;
using UnityEngine;

namespace AT_Utils
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class DialogController : DialogFactory
    {
        private const string dialogPrefabName = "SimpleDialog";
        private static UIBundle bundle => AT_UtilsGlobals.Instance.AssetBundle;
        protected override RectTransform dialogParent => DialogCanvasUtil.DialogCanvasRect;

        private void Start()
        {
            StartCoroutine(getPrefab());
        }

        private IEnumerator getPrefab()
        {
            if(prefab != null)
                yield break;
            foreach(var step in bundle.LoadAsset(dialogPrefabName))
                yield return step;
            prefab = bundle.GetAsset(dialogPrefabName);
            if(prefab == null)
                Destroy(gameObject);
            prefab.SetActive(false);
        }

        protected override void setupDialog(UI.SimpleDialog dialog)
        {
            base.setupDialog(dialog);
            var lockName = $"{dialogPrefabName}-{dialog.GetInstanceID():X}";
            dialog.onPointerEnterEvent.AddListener(_ => Utils.LockControls(lockName));
            dialog.onPointerExitEvent.AddListener(_ => Utils.LockControls(lockName, false));
            dialog.onClose.AddListener(() => Utils.LockControls(lockName, false));
        }
    }
}
