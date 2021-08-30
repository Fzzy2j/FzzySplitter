using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.Threading;

namespace FzzyTools.UI.Components
{
    public class Aimbot
    {
        private FzzyComponent fzzy;

        public Aimbot(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        private int targetOffset = -1;

        private bool ChooseTarget()
        {
            float closestDistance = 1000000000000F;
            float[] playerPos = new float[3];
            int closestOffset = -1;

            playerPos[0] = fzzy.values["x"].Current;
            playerPos[1] = fzzy.values["y"].Current;
            playerPos[2] = fzzy.values["z"].Current;

            // offset 4D4 health
            // offset 5E4 team
            // 4 = enemy
            // 3 = ally
            // 2 = enemy titan?
            // offset 490 position
            // offset 20C4 crit position?

            for (int i = 0; i < 100; i++)
            {
                var baseAdr = 0x01064698;
                var offset = 0x8 * i;
                var health =
                    new DeepPointer("server.dll", baseAdr, new int[] {offset, 0x4D4}).Deref<int>(FzzyComponent.process);
                if (health == 0) continue;
                var team =
                    new DeepPointer("server.dll", baseAdr, new int[] {offset, 0x5E4}).Deref<int>(FzzyComponent.process);
                var isAlive =
                    new DeepPointer("server.dll", baseAdr, new int[] {offset, 0xEC})
                        .Deref<int>(FzzyComponent.process) == 3;
                if (!isAlive) continue;

                float[] position = new float[3];

                //lcmCalc = lcmCalc + "," + zeroIndexEntity.ToInt64();

                if (team != 4 && team != 2) continue; // ignore non-enemies

                /*var x = new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x490 }).Deref<float>(FzzyComponent.process);
                var y = new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x494 }).Deref<float>(FzzyComponent.process);
                var z = new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x498 }).Deref<float>(FzzyComponent.process);*/
                var x = new DeepPointer("server.dll", baseAdr, new int[] {offset, 0x20C4}).Deref<float>(FzzyComponent
                    .process);
                var y = new DeepPointer("server.dll", baseAdr, new int[] {offset, 0x20C8}).Deref<float>(FzzyComponent
                    .process);
                var z = new DeepPointer("server.dll", baseAdr, new int[] {offset, 0x20CC}).Deref<float>(FzzyComponent
                    .process);

                position[0] = x;
                position[1] = y;
                position[2] = z;

                float distance = DistanceSquared(playerPos, position);

                if (distance < closestDistance)
                {
                    closestOffset = offset;
                    closestDistance = distance;
                }
            }

            targetOffset = closestOffset;
            return targetOffset != -1;
        }

        private bool Lock()
        {
            if (targetOffset == -1) return false;
            float[] playerPos = new float[3];

            playerPos[0] = fzzy.values["x"].Current;
            playerPos[1] = fzzy.values["y"].Current;
            playerPos[2] = fzzy.values["z"].Current;

            // offset 4D4 health
            // offset 5E4 team
            // 4 = enemy
            // 3 = ally
            // 2 = enemy titan?
            // offset 490 position
            // offset 20C4 crit position?

            var baseAdr = 0x01064698;
            var health =
                new DeepPointer("server.dll", baseAdr, new int[] {targetOffset, 0x4D4}).Deref<int>(
                    FzzyComponent.process);
            if (health == 0) return false;
            var isAlive =
                new DeepPointer("server.dll", baseAdr, new int[] {targetOffset, 0xEC})
                    .Deref<int>(FzzyComponent.process) == 3;
            if (!isAlive) ChooseTarget();

            float[] targetPos = new float[3];
            targetPos[0] =
                new DeepPointer("server.dll", baseAdr, new int[] {targetOffset, 0x20C4}).Deref<float>(FzzyComponent
                    .process);
            targetPos[1] =
                new DeepPointer("server.dll", baseAdr, new int[] {targetOffset, 0x20C8}).Deref<float>(FzzyComponent
                    .process);
            targetPos[2] =
                new DeepPointer("server.dll", baseAdr, new int[] {targetOffset, 0x20CC}).Deref<float>(FzzyComponent
                    .process) - 7;

            if (targetPos[0] == 0) return false;
            LookAt(playerPos, targetPos, out var yaw, out var pitch);
            fzzy.values["yaw"].Current = yaw - fzzy.values["viewThunkHorizontal"].Current -
                                         fzzy.values["recoilHorizontal"].Current;
            fzzy.values["pitch"].Current = pitch - fzzy.values["viewThunkVertical"].Current -
                                           fzzy.values["recoilVertical"].Current;

            fzzy.values["viewThunkHorizontal"].EndTick();
            fzzy.values["viewThunkVertical"].EndTick();
            fzzy.values["recoilHorizontal"].EndTick();
            fzzy.values["recoilVertical"].EndTick();
            fzzy.values["x"].EndTick();
            fzzy.values["y"].EndTick();
            fzzy.values["z"].EndTick();
            return true;
        }

        private float oldYaw;
        private float oldPitch;

        private Thread aimbotThread;

        public bool AimbotRunning => aimbotThread != null;

        public void Tick()
        {
            if (fzzy.values["timescale"].Current >= 1 || aimbotThread != null) return;
            if (fzzy.values["holdingM3"].Current && !fzzy.values["holdingM3"].Old)
            {
                oldYaw = fzzy.values["yaw"].Current;
                oldPitch = fzzy.values["pitch"].Current;
                ChooseTarget();
                startMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                aimbotThread = new Thread(DoAimbot);
                aimbotThread.Start();
            }
        }

        private long startMillis;

        private int padding = 60;

        private void DoAimbot()
        {
            while (true)
            {
                if (!Lock())
                    if (!ChooseTarget())
                        break;
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startMillis > padding / 2)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.KEY_M);
                }

                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startMillis > padding)
                {
                    fzzy.values["yaw"].Current = oldYaw;
                    fzzy.values["pitch"].Current = oldPitch;
                    break;
                }
            }

            aimbotThread = null;
        }

        private float DistanceSquared(float[] v1, float[] v2)
        {
            return (float) (Math.Pow(v1[0] - v2[0], 2) + Math.Pow(v1[1] - v2[1], 2) + Math.Pow(v1[2] - v2[2], 2));
        }

        private void LookAt(float[] v1, float[] v2, out float yaw, out float pitch)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];

            yaw = (float) Math.Atan2(dy, dx);

            float dxy = (float) Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            pitch = (float) -Math.Atan(dz / dxy);

            yaw = (float) (yaw * (180 / Math.PI));
            pitch = (float) (pitch * (180 / Math.PI));
        }
    }
}