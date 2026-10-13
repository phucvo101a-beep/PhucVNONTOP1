using System;
using System.Collections;
using COW;
using COW.GamePlay;
using UnityEngine;

namespace ProjectEspPatch
{
    public static class Logic
    {
        private const int EspMaster = 1;
        private const int EspBox = 2;
        private const int EspTracer = 4;
        private const int EspHealth = 8;
        private const int EspName = 16;
        private const int EspDistance = 32;
        private const int EspMask = 63;

        private const int StateInitialized = 128;
        private const int StateThreeFinger = 256;
        private const int StateDragging = 512;
        private const int TapShift = 12;
        private const int TapMask = 28672;

        public static bool Bootstrap(Player self)
        {
            if (self == null)
            {
                return false;
            }
            bool stealthState;
            try
            {
                stealthState = self.IsInStealth();
                GameObject driver = GameObject.Find("__esp_driver");
                bool createdDriver = false;
                if (driver == null)
                {
                    Camera camera = Camera.main;
                    Transform legacyFov = camera == null
                        ? null
                        : camera.transform.Find("__esp_fov");
                    if (legacyFov != null)
                    {
                        UnityEngine.Object.Destroy(legacyFov.gameObject);
                    }
                    driver = new GameObject("__esp_driver");
                    createdDriver = true;
                }

                Component component = driver.GetComponent(typeof(SceneEditBoxSelectTool));
                if (component == null)
                {
                    component = driver.AddComponent(typeof(SceneEditBoxSelectTool));
                }
                if (component == null)
                {
                    if (createdDriver)
                    {
                        UnityEngine.Object.Destroy(driver);
                    }
                }
                else
                {
                    if (createdDriver)
                    {
                        UnityEngine.Object.DontDestroyOnLoad(driver);
                    }
                }
            }
            catch (Exception)
            {
                // Rendering setup must not break the queried player state.
                stealthState = false;
            }
            return stealthState;
        }

        public static void Draw(SceneEditBoxSelectTool self)
        {
            if (self == null)
            {
                return;
            }
            Event currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            GameObject driverObject = self.gameObject;
            if (driverObject == null || driverObject.name != "__esp_driver")
            {
                return;
            }

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            float panelWidth = Mathf.Clamp((float)screenWidth * 0.42f, 360f, 500f);
            float panelHeight = 370f;
            int state = (int)self.DIGCJIKHGJP.x;
            bool menuOpen = self.AFHAOIAPPNB;

            if ((state & StateInitialized) == 0)
            {
                state = StateInitialized | EspMask;
                self.OHPFLEDNOJK = new Vector2(
                    ((float)screenWidth - panelWidth) * 0.5f,
                    ((float)screenHeight - panelHeight) * 0.5f);
                self.DIGCJIKHGJP = new Vector2((float)state, 0f);
                menuOpen = false;
            }

            int mask = state & EspMask;
            int tapCount = (state & TapMask) >> TapShift;
            float lastTapTime = self.DIGCJIKHGJP.y;
            bool threeFingerActive = (state & StateThreeFinger) != 0;
            bool threeFingersDown = Input.touchCount >= 3;

            if (threeFingersDown && !threeFingerActive)
            {
                state |= StateThreeFinger;
            }
            else if (!threeFingersDown && threeFingerActive)
            {
                state &= ~StateThreeFinger;
                float nowTime = Time.unscaledTime;
                if (lastTapTime > 0f && nowTime - lastTapTime > 3f)
                {
                    tapCount = 0;
                }
                tapCount++;
                if (tapCount >= 5)
                {
                    tapCount = 0;
                    menuOpen = true;
                }
                state = (state & ~TapMask) | (tapCount << TapShift);
                lastTapTime = nowTime;
            }

            float maxPanelX = (float)screenWidth - panelWidth - 8f;
            float maxPanelY = (float)screenHeight - panelHeight - 8f;
            if (maxPanelX < 8f) maxPanelX = 8f;
            if (maxPanelY < 8f) maxPanelY = 8f;
            float panelX = Mathf.Clamp(self.OHPFLEDNOJK.x, 8f, maxPanelX);
            float panelY = Mathf.Clamp(self.OHPFLEDNOJK.y, 8f, maxPanelY);
            float rowHeight = 43f;
            float firstRowY = panelY + 58f;
            float footerY = panelY + panelHeight - 55f;
            Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
            Rect headerRect = new Rect(panelX, panelY, panelWidth, 49f);
            Rect closeRect = new Rect(panelX + panelWidth - 43f, panelY + 9f, 32f, 30f);
            Rect row1Rect = new Rect(panelX + 18f, firstRowY, panelWidth - 36f, 40f);
            Rect row2Rect = new Rect(panelX + 18f, firstRowY + rowHeight, panelWidth - 36f, 40f);
            Rect row3Rect = new Rect(panelX + 18f, firstRowY + rowHeight * 2f, panelWidth - 36f, 40f);
            Rect row4Rect = new Rect(panelX + 18f, firstRowY + rowHeight * 3f, panelWidth - 36f, 40f);
            Rect row5Rect = new Rect(panelX + 18f, firstRowY + rowHeight * 4f, panelWidth - 36f, 40f);
            Rect row6Rect = new Rect(panelX + 18f, firstRowY + rowHeight * 5f, panelWidth - 36f, 40f);
            Rect resetRect = new Rect(panelX + 12f, footerY, (panelWidth - 32f) * 0.5f, 42f);
            Rect hideRect = new Rect(
                panelX + 20f + (panelWidth - 32f) * 0.5f, footerY,
                (panelWidth - 32f) * 0.5f, 42f);

            int rowCount = menuOpen ? 6 : 0;

            bool dragging = (state & StateDragging) != 0;
            bool releasedDrag = false;
            Vector2 pointer = currentEvent.mousePosition;
            if (menuOpen && currentEvent.type == EventType.MouseDown
                && headerRect.Contains(pointer) && !closeRect.Contains(pointer))
            {
                state |= StateDragging;
                dragging = true;
                currentEvent.Use();
            }
            else if (menuOpen && currentEvent.type == EventType.MouseDrag && dragging)
            {
                Vector2 delta = currentEvent.delta;
                panelX = Mathf.Clamp(panelX + delta.x, 8f, maxPanelX);
                panelY = Mathf.Clamp(panelY + delta.y, 8f, maxPanelY);
                self.OHPFLEDNOJK = new Vector2(panelX, panelY);
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseUp && dragging)
            {
                state &= ~StateDragging;
                dragging = false;
                releasedDrag = true;
                currentEvent.Use();
            }

            if (menuOpen && currentEvent.type == EventType.MouseUp
                && !dragging && !releasedDrag)
            {
                if (closeRect.Contains(pointer))
                {
                    menuOpen = false;
                }
                else if (resetRect.Contains(pointer))
                {
                    mask = EspMask;
                }
                else if (hideRect.Contains(pointer))
                {
                    menuOpen = false;
                }
                else
                {
                    int selectedRow = -1;
                    if (row1Rect.Contains(pointer)) selectedRow = 0;
                    else if (row2Rect.Contains(pointer)) selectedRow = 1;
                    else if (row3Rect.Contains(pointer)) selectedRow = 2;
                    else if (row4Rect.Contains(pointer)) selectedRow = 3;
                    else if (row5Rect.Contains(pointer)) selectedRow = 4;
                    else if (row6Rect.Contains(pointer)) selectedRow = 5;

                    if (selectedRow >= 0 && selectedRow < rowCount)
                    {
                        int espBit = selectedRow == 0 ? EspMaster
                            : (selectedRow == 1 ? EspBox
                            : (selectedRow == 2 ? EspTracer
                            : (selectedRow == 3 ? EspHealth
                            : (selectedRow == 4 ? EspName : EspDistance))));
                        mask ^= espBit;
                    }
                }
                if (panelRect.Contains(pointer))
                {
                    currentEvent.Use();
                }
            }

            state = (state & ~EspMask) | mask;
            self.AFHAOIAPPNB = menuOpen;
            self.OHPFLEDNOJK = new Vector2(panelX, panelY);
            self.DIGCJIKHGJP = new Vector2((float)state, lastTapTime);

            if (currentEvent.type != EventType.Repaint)
            {
                return;
            }

            Matrix4x4 savedMatrix = GUI.matrix;
            Color savedColor = GUI.color;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                Texture2D pixel = Texture2D.whiteTexture;
                if (pixel != null
                    && screenWidth > 0 && screenHeight > 0)
                {
                    Camera camera = Camera.main;
                    Player localPlayer = GameFacade.CurrentLocalPlayer();
                    Transform localRoot = localPlayer == null ? null : localPlayer.RootTransform;

                    if ((mask & EspMaster) != 0)
                    {
                        JMAGGLCNGIG match = GameFacade.CurrentMatch();
                        IList players = match == null ? null : match.ILPNGCMEFFI();
                        GameObject localObject = localPlayer == null
                            ? null : localPlayer.gameObject;
                        if (camera != null && localPlayer != null
                            && localObject != null && localObject.activeInHierarchy
                            && localRoot != null && players != null)
                        {
                            for (int index = 0; index < players.Count; index++)
                            {
                                try
                                {
                                Player player = players[index] as Player;
                                if (player == null)
                                {
                                    continue;
                                }
                                GameObject playerObject = player.gameObject;
                                if (playerObject == null || !playerObject.activeInHierarchy)
                                {
                                    continue;
                                }
                                Transform root = player.RootTransform;
                                if (player.IsLocalPlayer()
                                    || player.IsLocalTeammate(false) || !player.IsVisible())
                                {
                                    continue;
                                }

                                bool dying = player.IsDieing;
                                int health = player.CurHP;
                                int maximumHealth = player.MaxHP;
                                if (health <= 0 && !dying)
                                {
                                    continue;
                                }
                                Transform head = player.GetHeadTF();
                                if (root == null || head == null)
                                {
                                    continue;
                                }

                                float distance = Vector3.Distance(localRoot.position, root.position);
                                if (distance > 250f)
                                {
                                    continue;
                                }
                                Vector3 feetScreen = camera.WorldToScreenPoint(root.position);
                                Vector3 headScreen = camera.WorldToScreenPoint(head.position);
                                if (feetScreen.z <= 0f || headScreen.z <= 0f)
                                {
                                    continue;
                                }
                                if (float.IsNaN(feetScreen.x) || float.IsNaN(feetScreen.y)
                                    || float.IsNaN(feetScreen.z) || float.IsNaN(headScreen.x)
                                    || float.IsNaN(headScreen.y) || float.IsNaN(headScreen.z)
                                    || float.IsInfinity(feetScreen.x) || float.IsInfinity(feetScreen.y)
                                    || float.IsInfinity(headScreen.x) || float.IsInfinity(headScreen.y))
                                {
                                    continue;
                                }

                                float height = Mathf.Abs(headScreen.y - feetScreen.y) * 1.0909091f;
                                if (height < 2f)
                                {
                                    continue;
                                }
                                float width = height * 0.5f;
                                float left = headScreen.x - width * 0.5f;
                                float top = (float)screenHeight - headScreen.y;
                                float thickness = 1.5f;
                                if (float.IsNaN(left) || float.IsNaN(top)
                                    || float.IsInfinity(left) || float.IsInfinity(top)
                                    || left > (float)screenWidth || left + width < 0f
                                    || top > (float)screenHeight || top + height < 0f)
                                {
                                    continue;
                                }
                                if ((mask & EspBox) != 0)
                                {
                                    GUI.color = Color.white;
                                    GUI.DrawTexture(new Rect(left, top, width, thickness), pixel);
                                    GUI.DrawTexture(new Rect(
                                        left, top + height - thickness, width, thickness), pixel);
                                    GUI.DrawTexture(new Rect(left, top, thickness, height), pixel);
                                    GUI.DrawTexture(new Rect(
                                        left + width - thickness, top, thickness, height), pixel);
                                }

                                if ((mask & EspTracer) != 0)
                                {
                                    float startX = (float)screenWidth * 0.5f;
                                    float startY = 80f;
                                    float endX = left + width * 0.5f;
                                    float endY = top + height / 35f;
                                    float tracerX = endX - startX;
                                    float tracerY = endY - startY;
                                    float tracerLength = Mathf.Sqrt(
                                        tracerX * tracerX + tracerY * tracerY);
                                    float tracerAngle = Mathf.Atan2(
                                        tracerY, tracerX) * 57.29578f;
                                    GUI.color = Color.white;
                                    GUI.matrix = Matrix4x4.TRS(
                                        new Vector3(startX, startY, 0f),
                                        Quaternion.Euler(0f, 0f, tracerAngle),
                                        Vector3.one);
                                    GUI.DrawTexture(new Rect(
                                        0f, -thickness * 0.5f,
                                        tracerLength, thickness), pixel);
                                    GUI.matrix = Matrix4x4.identity;
                                }

                                if ((mask & EspHealth) != 0)
                                {
                                    float healthRatio = maximumHealth > 0
                                        ? Mathf.Clamp01((float)health / (float)maximumHealth)
                                        : 0f;
                                    float healthX = left + width + 2f;
                                    GUI.color = new Color(0f, 0f, 0f, 0.9f);
                                    GUI.DrawTexture(new Rect(healthX, top, 5f, height), pixel);
                                    GUI.color = dying || healthRatio < 0.3f
                                        ? new Color(1f, 0f, 0f, 1f)
                                        : (healthRatio <= 0.6f
                                            ? new Color(1f, 1f, 0f, 1f)
                                            : new Color(0f, 1f, 0f, 1f));
                                    GUI.DrawTexture(new Rect(
                                        healthX + 1f,
                                        top + height - height * healthRatio,
                                        3f,
                                        height * healthRatio), pixel);
                                }

                                float labelScale = 0.72f;
                                if ((mask & EspName) != 0)
                                {
                                    string nickname = player.NickName;
                                    if (nickname == null) nickname = "?";
                                    GUI.color = dying
                                        ? new Color(1f, 0f, 0f, 1f)
                                        : new Color(1f, 1f, 0f, 1f);
                                    float nicknameWidth = Mathf.Clamp(
                                        (float)nickname.Length * 6.2f, width, 220f);
                                    GUI.matrix = Matrix4x4.TRS(
                                        new Vector3(
                                            left + width * 0.5f - nicknameWidth * 0.5f,
                                            top - 14f,
                                            0f),
                                        Quaternion.identity,
                                        new Vector3(labelScale, labelScale, 1f));
                                    GUI.Label(new Rect(
                                        0f, 0f,
                                        nicknameWidth / labelScale,
                                        18f / labelScale), nickname);
                                    GUI.matrix = Matrix4x4.identity;
                                }

                                if ((mask & EspDistance) != 0)
                                {
                                    int distanceValue = (int)distance;
                                    if (distanceValue < 0) distanceValue = 0;
                                    if (distanceValue > 999) distanceValue = 999;
                                    int digitCount = distanceValue >= 100 ? 3
                                        : (distanceValue >= 10 ? 2 : 1);
                                    float glyphWidth = 6f;
                                    float glyphHeight = 10f;
                                    float glyphThickness = 1.5f;
                                    float glyphAdvance = 8f;
                                    float meterWidth = 7f;
                                    float distanceWidth = digitCount * glyphAdvance
                                        + meterWidth + 2f;
                                    float distanceX = left + width * 0.5f
                                        - distanceWidth * 0.5f;
                                    float distanceY = top + height + 5f;
                                    GUI.color = Color.white;
                                    int divisor = digitCount == 3 ? 100
                                        : (digitCount == 2 ? 10 : 1);
                                    for (int digitIndex = 0; digitIndex < digitCount; digitIndex++)
                                    {
                                        int digit = (distanceValue / divisor) % 10;
                                        int segments = digit == 0 ? 63
                                            : (digit == 1 ? 6
                                            : (digit == 2 ? 91
                                            : (digit == 3 ? 79
                                            : (digit == 4 ? 102
                                            : (digit == 5 ? 109
                                            : (digit == 6 ? 125
                                            : (digit == 7 ? 7
                                            : (digit == 8 ? 127 : 111))))))));
                                        float glyphX = distanceX + digitIndex * glyphAdvance;
                                        float middleY = distanceY + glyphHeight * 0.5f
                                            - glyphThickness * 0.5f;
                                        if ((segments & 1) != 0)
                                            GUI.DrawTexture(new Rect(
                                                glyphX, distanceY, glyphWidth, glyphThickness), pixel);
                                        if ((segments & 2) != 0)
                                            GUI.DrawTexture(new Rect(
                                                glyphX + glyphWidth - glyphThickness,
                                                distanceY, glyphThickness, glyphHeight * 0.5f), pixel);
                                        if ((segments & 4) != 0)
                                            GUI.DrawTexture(new Rect(
                                                glyphX + glyphWidth - glyphThickness,
                                                middleY, glyphThickness, glyphHeight * 0.5f), pixel);
                                        if ((segments & 8) != 0)
                                            GUI.DrawTexture(new Rect(
                                                glyphX, distanceY + glyphHeight - glyphThickness,
                                                glyphWidth, glyphThickness), pixel);
                                        if ((segments & 16) != 0)
                                            GUI.DrawTexture(new Rect(
                                                glyphX, middleY, glyphThickness, glyphHeight * 0.5f), pixel);
                                        if ((segments & 32) != 0)
                                            GUI.DrawTexture(new Rect(
                                                glyphX, distanceY, glyphThickness, glyphHeight * 0.5f), pixel);
                                        if ((segments & 64) != 0)
                                            GUI.DrawTexture(new Rect(
                                                glyphX, middleY, glyphWidth, glyphThickness), pixel);
                                        divisor /= 10;
                                    }
                                    float meterX = distanceX + digitCount * glyphAdvance + 1f;
                                    float meterY = distanceY + 2f;
                                    GUI.DrawTexture(new Rect(
                                        meterX, meterY, glyphThickness, glyphHeight - 2f), pixel);
                                    GUI.DrawTexture(new Rect(
                                        meterX + 3f, meterY, glyphThickness, glyphHeight - 2f), pixel);
                                    GUI.DrawTexture(new Rect(
                                        meterX + glyphThickness, meterY,
                                        2f, glyphThickness), pixel);
                                    GUI.DrawTexture(new Rect(
                                        meterX + glyphThickness, meterY + 4f,
                                        2f, glyphThickness), pixel);
                                    GUI.matrix = Matrix4x4.identity;
                                }
                                }
                                catch (Exception)
                                {
                                    // A recycled entity is skipped without aborting the frame.
                                }
                            }
                        }
                    }

                    if (menuOpen)
                    {
                        GUI.matrix = Matrix4x4.identity;
                        GUI.color = new Color(0.018f, 0.02f, 0.025f, 0.98f);
                        GUI.DrawTexture(panelRect, pixel);
                        GUI.color = new Color(0.09f, 0.1f, 0.12f, 1f);
                        GUI.DrawTexture(headerRect, pixel);
                        GUI.color = new Color(0.75f, 0.78f, 0.84f, 1f);
                        float titleWidth = 96f;
                        GUI.Label(new Rect(
                            panelX + panelWidth * 0.5f - titleWidth * 0.5f,
                            panelY + 14f, titleWidth, 24f), "ESP CONTROL");

                        GUI.color = new Color(0.5f, 0.12f, 0.14f, 1f);
                        GUI.DrawTexture(closeRect, pixel);
                        GUI.color = Color.white;
                        GUI.Label(new Rect(
                            closeRect.x + 11f, closeRect.y + 5f, 18f, 20f), "X");

                        string[] labels = new string[6];
                        labels[0] = "Enable ESP";
                        labels[1] = "Player box";
                        labels[2] = "Top tracer";
                        labels[3] = "Health bar";
                        labels[4] = "Player name";
                        labels[5] = "Distance";

                        int[] bits = new int[6];
                        bits[0] = EspMaster;
                        bits[1] = EspBox;
                        bits[2] = EspTracer;
                        bits[3] = EspHealth;
                        bits[4] = EspName;
                        bits[5] = EspDistance;

                        for (int row = 0; row < rowCount; row++)
                        {
                            Rect rowRect = row == 0 ? row1Rect
                                : (row == 1 ? row2Rect
                                : (row == 2 ? row3Rect
                                : (row == 3 ? row4Rect
                                : (row == 4 ? row5Rect : row6Rect))));

                            GUI.color = new Color(0.045f, 0.048f, 0.058f, 1f);
                            GUI.DrawTexture(rowRect, pixel);
                            GUI.color = new Color(0.82f, 0.84f, 0.88f, 1f);
                            GUI.Label(new Rect(
                                rowRect.x + 14f, rowRect.y + 10f,
                                rowRect.width - 90f, 22f),
                                labels[row]);

                            bool enabled = (mask & bits[row]) != 0;
                            Rect track = new Rect(
                                rowRect.x + rowRect.width - 66f,
                                rowRect.y + 7f, 54f, 26f);
                            GUI.color = enabled
                                ? new Color(0.92f, 0.94f, 0.97f, 1f)
                                : new Color(0.16f, 0.17f, 0.2f, 1f);
                            GUI.DrawTexture(track, pixel);
                            GUI.color = enabled
                                ? new Color(0.055f, 0.06f, 0.075f, 1f)
                                : new Color(0.92f, 0.94f, 0.97f, 1f);
                            float knobX = enabled ? track.x + 31f : track.x + 3f;
                            GUI.DrawTexture(new Rect(
                                knobX, track.y + 3f, 20f, 20f), pixel);
                        }

                        GUI.color = new Color(0.93f, 0.94f, 0.97f, 1f);
                        GUI.DrawTexture(resetRect, pixel);
                        GUI.color = new Color(0.04f, 0.045f, 0.055f, 1f);
                        GUI.Label(new Rect(
                            resetRect.x + resetRect.width * 0.5f - 50f,
                            resetRect.y + 11f, 110f, 22f), "RESET DEFAULTS");

                        GUI.color = new Color(0.035f, 0.038f, 0.046f, 1f);
                        GUI.DrawTexture(hideRect, pixel);
                        GUI.color = new Color(0.65f, 0.67f, 0.72f, 1f);
                        GUI.Label(new Rect(
                            hideRect.x + hideRect.width * 0.5f - 35f,
                            hideRect.y + 11f, 80f, 22f), "HIDE MENU");
                    }
                }
            }
            catch (Exception)
            {
                // A transient Unity object is skipped for this event.
            }

            GUI.matrix = savedMatrix;
            GUI.color = savedColor;
        }
    }
}
