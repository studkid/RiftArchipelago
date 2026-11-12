using System.Collections.Concurrent;
using System.Collections.Generic;
using RhythmRift;
using RiftOfTheNecroManager;
using UnityEngine;

namespace Helpers.RiftArchipelago {

    internal class NightmareGloom {
        private System.Random randomGenerator = new System.Random();
        private int minSecondsBeforeChange = 10;
        private int maxSecondsBeforeChange = 20;
        private int changeTimer = 0;
        public int durationTimer { get; private set; }
        public int lane { get; private set; }
        private List<Animator> gloomLanes;

        public NightmareGloom(RRStageController controller) {
            SetRandomNightmareDelay();
            durationTimer = 60;
            gloomLanes = controller._obscureAnimators;
            SetNewNightmareRow();
        }

        public void ProcessNightmare() {
            changeTimer--;
            durationTimer--;

            if(durationTimer <= 0) return;

            if(changeTimer <= 0) SetNewNightmareRow();
        }

        private void SetNewNightmareRow() {
            Animator animator = gloomLanes[lane];
            if((bool) animator) {
                animator.SetTrigger("GloomOff");
            }

            lane = randomGenerator.Next(0, gloomLanes.Count);
            SetRandomNightmareDelay();
        }

        private void SetRandomNightmareDelay() {
            changeTimer = randomGenerator.Next(minSecondsBeforeChange, maxSecondsBeforeChange);
        }
    }

    internal static class TrapHelper {
        
        private static ConcurrentQueue<string> trapQueue = new ConcurrentQueue<string>();
        private static int confuseTimer = -1;
        private static int gloomTimer = -1;
        private static int codaTimer = -1;
        private static int glassTimer = -1;
        private static List<NightmareGloom> nightmares = new List<NightmareGloom>();

        public static void AddTrap(string trapName) {
            trapQueue.Enqueue(trapName);
        }

        internal static void RunTraps(RRStageController controller) {
            if(!trapQueue.IsEmpty) {
                trapQueue.TryDequeue(out string trap);

                if(trap == "Mystery Trap") {
                    confuseTimer =+ 12;
                }

                if(trap == "Gloom Trap") {
                    gloomTimer =+ 12;
                }

                if(trap == "Nightmare Trap") {
                    nightmares.Add(new NightmareGloom(controller));
                    Log.Info("Creating Nightmare: " + nightmares.Count);
                }

                if(trap == "Coda Trap") {
                    codaTimer =+ 12;
                }

                if(trap == "Glass Trap") {
                    glassTimer =+ 12;
                }
            }

            if(confuseTimer > 0) {
                controller._enigmaAnimator.SetTrigger("EnigmaOn");

                confuseTimer--;
                Log.Info("Enigma Counter: " + confuseTimer);
            }
            else if(confuseTimer == 0) {
                controller._enigmaAnimator.SetTrigger("EnigmaOff");

                confuseTimer--;
                Log.Info("Enigma Off!");
            }

            if(gloomTimer > 0) {
                for (int i = 0; i < 3; i++) {
                    Animator animator = controller._obscureAnimators[i];

                    if ((bool)animator) {
                        animator.SetTrigger("GloomOn");
                    }
            }

                gloomTimer--;
            }
            else if(gloomTimer == 0) {
                for (int i = 0; i < 3; i++) {
                    Animator animator = controller._obscureAnimators[i];

                    if ((bool)animator) {
                        animator.SetTrigger("GloomOff");
                    }
                }
                    
                gloomTimer--;
            }

            List<NightmareGloom> refNightmares = new List<NightmareGloom>(nightmares);
            foreach(NightmareGloom nightmare in refNightmares) {
                nightmare.ProcessNightmare();

                if(nightmare.durationTimer <= 0) {
                    Animator animator = controller._obscureAnimators[nightmare.lane];
                    if((bool) animator) {
                        animator.SetTrigger("GloomOff");
                    }
                    nightmares.Remove(nightmare);
                } 
                else {
                    Animator animator = controller._obscureAnimators[nightmare.lane];
                    if((bool) animator) {
                        animator.SetTrigger("GloomOn");
                    }
                }
            }

            if(codaTimer > 0) {
                codaTimer--;
            }

            if(glassTimer > 0) {
                controller._playerController.OverrideHealth(1, 1);
                controller.UpdateUI();

                glassTimer--;
            }
            else if(glassTimer == 0) {
                controller._playerController.OverrideHealth(10, 10);
                controller.UpdateUI();

                glassTimer--;
            }
        }

        public static int getTimer(string timer) {
            if(timer == "Enigma") {
                return confuseTimer;
            }

            if(timer == "Perfectionist") {
                return codaTimer;
            }

            return -1;
        }
    }
}