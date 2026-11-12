using System.Collections.Concurrent;
using System.Collections.Generic;
using RhythmRift;
using RiftOfTheNecroManager;
using UnityEngine;

namespace Helpers.RiftArchipelago {

    internal class NightmareGloom {
        private System.Random randomGenerator = new System.Random();
        private int minSecondsBeforeChange = 20;
        private int maxSecondsBeforeChange = 30;
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
            animator = gloomLanes[lane];
            if((bool) animator) {
                animator.SetTrigger("GloomOn");
            }


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
        private static int lastPlayerHealth = 0;

        public static void AddTrap(string trapName) {
            trapQueue.Enqueue(trapName);
        }

        internal static void RunTraps(RRStageController controller) {
            if(!trapQueue.IsEmpty) {
                trapQueue.TryDequeue(out string trap);

                if(trap == "Mystery Trap") {
                    if(confuseTimer <= 0) {
                        controller._enigmaAnimator.SetTrigger("EnigmaOn");
                    }

                    confuseTimer += 12;
                }

                if(trap == "Gloom Trap") {
                    if(gloomTimer <= 0) {
                        for (int i = 0; i < 3; i++) {
                            Animator animator = controller._obscureAnimators[i];

                            if ((bool)animator) {
                                animator.SetTrigger("GloomOn");
                            }
                        }
                    }

                    gloomTimer += 12;
                }

                if(trap == "Nightmare Trap") {
                    nightmares.Add(new NightmareGloom(controller));
                }

                if(trap == "Coda Trap") {
                    codaTimer += 12;
                }

                if(trap == "Glass Trap") {
                    if(glassTimer <= 0) {
                        lastPlayerHealth = controller._playerController.CurrentHealth;
                        controller._playerController.OverrideHealth(1, 1);
                        controller.UpdateUI();
                    }

                    glassTimer += 12;
                }
            }

            if(confuseTimer > 0) {
                confuseTimer--;
            }
            else if(confuseTimer == 0) {
                controller._enigmaAnimator.SetTrigger("EnigmaOff");

                confuseTimer--;
            }

            if(gloomTimer > 0) {
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
            }

            if(codaTimer > 0) {
                codaTimer--;
            }

            if(glassTimer > 0) {
                glassTimer--;
            }
            else if(glassTimer == 0) {
                controller._playerController.OverrideHealth(lastPlayerHealth, 10);
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

        public static void ResetTraps() {
            confuseTimer = -1;
            gloomTimer = -1;
            nightmares.Clear();
            codaTimer = -1;
            glassTimer = -1;
        }
    }
}