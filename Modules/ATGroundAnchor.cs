//   ModuleGroundAnchor.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AT_Utils
{
    public class ATGroundAnchor : PartModule
    {
        [KSPField] public string AnimatorID = string.Empty;
        IAnimator animator;

        [KSPField] public bool Controllable = true;
        [KSPField(isPersistant = true)] protected bool isAttached;
        private Coroutine engage_coro;
        private bool engaged;

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
                attach_anchor();
            update_part_events();
        }

        /// <summary>
        /// This is a receiver of the component message sent from Part
        /// </summary>
        private void OnPartPack() => detach_anchor();

        /// <summary>
        /// This is a receiver of the component message sent from Part
        /// </summary>
        private void OnPartUnpack()
        {
            if(isAttached)
                attach_anchor();
        }

        private void FixedUpdate()
        {
            if(!isAttached
               || !engaged
               || !HighLogic.LoadedSceneIsFlight
               || vessel == null
               || !vessel.loaded
               || vessel.packed)
                return;
            setup_ground_contact();
            DumpVelocity();
        }

        private void setup_ground_contact()
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

        protected virtual void detach_anchor()
        {
            if(engage_coro != null)
            {
                StopCoroutine(engage_coro);
                engage_coro = null;
            }
            engaged = false;
        }

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

        protected virtual void attach_anchor()
        {
            if(engage_coro == null)
                engage_coro = StartCoroutine(engage_on_ground_contact());
        }

        private IEnumerator<YieldInstruction> engage_on_ground_contact()
        {
            engaged = false;
            while(!engaged)
            {
                engaged = vessel.Parts.Any(p => p.GroundContact);
                yield return null;
            }
            setup_ground_contact();
            update_part_events();
        }

        private void update_part_events()
        {
            var evt = Events[nameof(ToggleAnchor)];
            evt.active = Controllable;
            evt.guiName = isAttached
                ? "Detach Anchor"
                : "Attach Anchor";
        }

        public void DumpVelocity()
        {
            if(vessel == null || !vessel.loaded)
                return;
            for(int i = 0, nparts = vessel.parts.Count; i < nparts; i++)
            {
                var r = vessel.parts[i].Rigidbody;
                if(r == null)
                    continue;
                r.angularVelocity *= 0;
                r.velocity *= 0;
            }
        }

        public void ForceAttach()
        {
            if(!isAttached)
            {
                attach_anchor();
                on_anchor_attached();
            }
            isAttached = true;
            update_part_events();
        }

        public void Detach()
        {
            if(isAttached)
            {
                detach_anchor();
                on_anchor_detached();
            }
            isAttached = false;
            update_part_events();
        }

        [KSPEvent(guiActive = true, guiName = "Attach Anchor", active = true)]
        public void ToggleAnchor()
        {
            if(isAttached)
                Detach();
            else if(can_attach())
                ForceAttach();
        }
    }
}
