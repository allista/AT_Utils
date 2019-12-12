//   ModuleGroundAnchor.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public class ATGroundAnchor : PartModule
    {
        [KSPField] public string AnimatorID = string.Empty;
        IAnimator animator;

        [KSPField] public bool Controllable = true;
        [KSPField (isPersistant = true)] protected bool isAttached;

        [KSPField] public string attachSndPath = string.Empty;
        [KSPField] public string detachSndPath = string.Empty;
        public FXGroup fxSndAttach, fxSndDetach;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            animator = part.GetAnimator(AnimatorID);
            if(!string.IsNullOrEmpty(attachSndPath))
                Utils.createFXSound(part, fxSndAttach, attachSndPath, false);
            if(!string.IsNullOrEmpty(detachSndPath))
                Utils.createFXSound(part, fxSndDetach, detachSndPath, false);
            if(isAttached)
                setup_ground_contact();
            update_part_events();
        }

        void OnPartPack() => detatch_anchor();
        void OnPartUnpack()
        {
            if(isAttached)
            {
                setup_ground_contact();
                ForceAttach();
            }
        }

        void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight ||
                vessel == null || !vessel.loaded || vessel.packed)
                return;
            if(isAttached)
            {
                setup_ground_contact();
                DumpVelocity();
            }
        }

        void setup_ground_contact()
        {
            part.PermanentGroundContact = true;
            if(vessel != null) 
                vessel.permanentGroundContact = true;
        }

        protected virtual void on_anchor_attached() 
        {
            if(fxSndAttach.audio != null)
                fxSndAttach.audio.Play();
            if(animator != null)
                animator.Open();
        }

        protected virtual void on_anchor_detached()
        {
            if(fxSndDetach.audio != null) 
                fxSndDetach.audio.Play();
            if(animator != null)
                animator.Close();
        }

        protected virtual void detatch_anchor() {}

        bool can_attach()
        {
            //always check relative velocity and acceleration
            if(!vessel.Landed) 
            {
                Utils.Message("There's nothing to attach the anchor to");
                return false;
            }
            if(vessel.GetSrfVelocity().sqrMagnitude > 1f)
            {
                Utils.Message("Cannot attach the anchor while mooving");
                return false;
            }
            return true;
        }

        void update_part_events()
        {
            Events["Attach"].active = Controllable && !isAttached;
            Events["Detach"].active = Controllable && isAttached;
        }

        public void DumpVelocity()
        {
            if(vessel == null || !vessel.loaded) return;
            for(int i = 0, nparts = vessel.parts.Count; i < nparts; i++)
            {
                var r = vessel.parts[i].Rigidbody;
                if(r == null) continue;
                r.angularVelocity *= 0;
                r.velocity *= 0;
            }
        }

        public void ForceAttach()
        {
            detatch_anchor();
            DumpVelocity();
            if(!isAttached) 
                on_anchor_attached();
            isAttached = true;
            update_part_events();
        }

        [KSPEvent (guiActive = true, guiName = "Attach Anchor", active = true)]
        public void Attach()
        {
            if(can_attach()) 
                ForceAttach();
        }

        [KSPEvent (guiActive = true, guiName = "Detach Anchor", active = false)]
        public void Detach()
        {
            detatch_anchor();
            if(isAttached) 
                on_anchor_detached();
            isAttached = false;
            update_part_events();
        }
    }
}
