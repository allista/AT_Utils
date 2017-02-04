//   SubmodelResizer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri
using UnityEngine;

namespace AT_Utils
{
    public class SubmodelResizer : PartModule
    {
        //part.cfg option
        [KSPField] public string SubmodelName = "model";
        [KSPField] public string DisplayName = "Scale";

        //part slider to set the scale
        [KSPField(isPersistant=true, guiActive = true, guiActiveEditor=true, guiName="Scale", guiFormat="F4")]
        [UI_FloatEdit(scene=UI_Scene.All, minValue=0.5f, maxValue=10, incrementLarge=1.0f, incrementSmall=0.1f, incrementSlide=0.001f, sigFigs = 4)]
        public float CurrentScale = 1.0f;

        Vector3 orig_scale = Vector3.one;
        Transform submodel, prefab_submodel;
        float last_scale = -1;

        void update_orig_scale()
        {
            if(part.partInfo != null && part.partInfo.partPrefab != null)
            {
                //get original local scale of the submodel
                prefab_submodel = part.partInfo.partPrefab.FindModelTransform(SubmodelName);
                if(prefab_submodel != null)
                {
                    orig_scale = prefab_submodel.localScale;
                    //get submodel of the part
                    submodel = part.FindModelTransform(SubmodelName);
                    //set scale field name
                    Fields["CurrentScale"].guiName = DisplayName;
                }
                else 
                    Debug.LogWarningFormat(this, "There's no '{0}' transform in this part.", SubmodelName);
            }
        }

        void rescale()
        {
            if(submodel == null) return;
            submodel.localScale = orig_scale * CurrentScale;
            submodel.hasChanged = true;
            part.transform.hasChanged = true;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            update_orig_scale();
            rescale();
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            update_orig_scale();
            rescale();
        }

        void Update()
        {
            if(CurrentScale.Equals(last_scale)) return;
            rescale();
            last_scale = CurrentScale;
        }
    }
}